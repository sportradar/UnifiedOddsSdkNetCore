// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// An <see cref="IProfileCache"/> implementation using <see cref="ICacheStore{T}"/> to cache fetched information
    /// </summary>
    internal class ProfileCache : SdkCache, IProfileCache
    {
        /// <summary>
        /// A <see cref="ICacheStore{T}"/> used to store fetched information
        /// </summary>
        private readonly ICacheStore<string> _cache;

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// Value indicating whether the current instance was already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// The bag of currently fetching player profiles
        /// </summary>
        private readonly ConcurrentDictionary<Urn, DateTime> _fetchedPlayerProfiles = new ConcurrentDictionary<Urn, DateTime>();

        /// <summary>
        /// The bag of currently fetching competitor profiles
        /// </summary>
        private readonly ConcurrentDictionary<Urn, DateTime> _fetchedCompetitorProfiles = new ConcurrentDictionary<Urn, DateTime>();

        /// <summary>
        /// The bag of currently merging ids
        /// </summary>
        private readonly ConcurrentDictionary<Urn, DateTime> _mergeUrns = new ConcurrentDictionary<Urn, DateTime>();

        /// <summary>
        /// The semaphore used for export/import operation
        /// </summary>
        private readonly SemaphoreSlim _exportSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileCache"/> class
        /// </summary>
        /// <param name="cache">A <see cref="ICacheStore{T}"/> used to store fetched information</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to fetch data</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to fetch event summary data</param>
        /// <param name="loggerFactory">The logger factory for creating Cache and Execution logs</param>
        public ProfileCache(ICacheStore<string> cache,
                            IDataRouterManager dataRouterManager,
                            ICacheManager cacheManager,
                            ISportEventCache sportEventCache,
                            ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
            Guard.Argument(cache, nameof(cache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();

            _cache = cache;
            _dataRouterManager = dataRouterManager;
            _sportEventCache = sportEventCache;
            _isDisposed = false;
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.Fixture, DtoType.MatchTimeline, DtoType.Competitor, DtoType.CompetitorProfile,
                                     DtoType.SimpleTeamProfile, DtoType.PlayerProfile, DtoType.SportEventSummary, DtoType.TournamentInfo,
                                     DtoType.RaceSummary, DtoType.MatchSummary, DtoType.TournamentInfoList, DtoType.TournamentSeasons,
                                     DtoType.SportEventSummaryList
                                 };
        }

        public bool IsDisposed()
        {
            return _isDisposed;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing)
            {
                _fetchedPlayerProfiles.Clear();
                _fetchedCompetitorProfiles.Clear();
                _mergeUrns.Clear();
                _exportSemaphore.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="PlayerProfileCacheItem"/> representing the profile for the specified player
        /// </summary>
        /// <param name="playerId">A <see cref="Urn"/> specifying the id of the player for which to get the profile</param>
        /// <param name="wantedLanguages">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if player profile should be fetched when <see cref="PlayerProfileCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A <see cref="Task{PlayerProfileCacheItem}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        public Task<PlayerProfileCacheItem> GetPlayerProfileAsync(Urn playerId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing)
        {
            Guard.Argument(playerId, nameof(playerId)).NotNull();
            Guard.Argument(wantedLanguages, nameof(wantedLanguages)).NotNull();
            if (wantedLanguages.IsNullOrEmpty())
            {
                throw new ArgumentException("Missing wanted languages", nameof(wantedLanguages));
            }

            return GetPlayerProfileInternalAsync(playerId, wantedLanguages, fetchIfMissing);
        }

        private async Task<PlayerProfileCacheItem> GetPlayerProfileInternalAsync(Urn playerId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing)
        {
            using (new TelemetryTracker(UofSdkTelemetry.ProfileCacheGetPlayerProfile))
            {
                await WaitTillIdIsAvailableAsync(_fetchedPlayerProfiles, playerId).ConfigureAwait(false);

                PlayerProfileCacheItem cachedItem;
                try
                {
                    cachedItem = (PlayerProfileCacheItem)_cache.Get(playerId.ToString());
                    if (cachedItem != null && !fetchIfMissing)
                    {
                        return cachedItem;
                    }
                    var missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, cachedItem?.Names.Keys.ToList());
                    if (!missingLanguages.Any())
                    {
                        return cachedItem;
                    }

                    // try to fetch for competitor, to avoid requests by each player
                    if (cachedItem?.CompetitorId != null)
                    {
                        await GetCompetitorProfileInsteadOfPlayerProfile(playerId, cachedItem.CompetitorId, missingLanguages.ToList());
                    }

                    cachedItem = (PlayerProfileCacheItem)_cache.Get(playerId.ToString());
                    missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, cachedItem?.Names.Keys.ToList());
                    if (missingLanguages.Any())
                    {
                        var cultureTaskDictionary = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetPlayerProfileAsync(playerId, c, null));
                        await Task.WhenAll(cultureTaskDictionary.Values).ConfigureAwait(false);
                    }
                    cachedItem = (PlayerProfileCacheItem)_cache.Get(playerId.ToString());
                }
                catch (Exception ex)
                {
                    if (ex is DeserializationException || ex is MappingException)
                    {
                        throw new CacheItemNotFoundException($"An error occurred while fetching player profile for player {playerId} in cache", playerId.ToString(), ex);
                    }

                    throw;
                }
                finally
                {
                    await ReleaseIdAsync(_fetchedPlayerProfiles, playerId).ConfigureAwait(false);
                }

                return cachedItem;
            }
        }

        private async Task GetCompetitorProfileInsteadOfPlayerProfile(Urn playerId, Urn competitorId, IReadOnlyCollection<CultureInfo> wantedCultures)
        {
            if (competitorId == null || wantedCultures.IsNullOrEmpty())
            {
                return;
            }
            var competitorCacheItem = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
            if (competitorCacheItem == null)
            {
                ExecutionLog.LogDebug("Fetching competitor profile for competitor {CompetitorId} instead of player {PlayerId} for languages=[{Languages}]", competitorId, playerId, LanguageHelper.GetCultureList(wantedCultures));
                _ = await GetCompetitorProfileAsync(competitorId, wantedCultures, true).ConfigureAwait(false);
            }
            else
            {
                var missingCultures = competitorCacheItem.GetMissingProfileCultures(wantedCultures);
                if (!missingCultures.IsNullOrEmpty())
                {
                    ExecutionLog.LogDebug("Fetching competitor profile for competitor {CompetitorId} instead of player {PlayerId} for languages=[{Languages}]", competitorId, playerId, LanguageHelper.GetCultureList(missingCultures));
                    _ = await GetCompetitorProfileAsync(competitorId, missingCultures.ToList(), true).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Asynchronously gets <see cref="CompetitorCacheItem"/> representing the profile for the specified competitor
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="wantedLanguages">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <param name="fetchIfMissing">Indicated if competitor profile should be fetched when <see cref="CompetitorCacheItem"/> is missing name for specified cultures</param>
        /// <returns>A <see cref="Task{PlayerProfileCacheItem}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        public Task<CompetitorCacheItem> GetCompetitorProfileAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing)
        {
            Guard.Argument(competitorId, nameof(competitorId)).NotNull();
            Guard.Argument(wantedLanguages, nameof(wantedLanguages)).NotNull();
            if (!wantedLanguages.Any())
            {
                throw new ArgumentException("Missing wanted languages", nameof(wantedLanguages));
            }

            return GetCompetitorProfileInternalAsync(competitorId, wantedLanguages, fetchIfMissing);
        }

        private async Task<CompetitorCacheItem> GetCompetitorProfileInternalAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing)
        {
            using (new TelemetryTracker(UofSdkTelemetry.ProfileCacheGetCompetitorProfile))
            {
                await WaitTillIdIsAvailableAsync(_fetchedCompetitorProfiles, competitorId).ConfigureAwait(false);

                CompetitorCacheItem cachedItem;
                try
                {
                    cachedItem = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
                    if (cachedItem != null && !fetchIfMissing)
                    {
                        return cachedItem;
                    }
                    var missingLanguages = wantedLanguages.ToList();
                    if (cachedItem != null)
                    {
                        missingLanguages = cachedItem.GetMissingProfileCultures(wantedLanguages).ToList();
                    }
                    if (missingLanguages.Any())
                    {
                        var cultureTasks = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetCompetitorAsync(competitorId, c, null));
                        await Task.WhenAll(cultureTasks.Values).ConfigureAwait(false);
                    }
                    cachedItem = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
                }
                catch (Exception ex)
                {
                    if (ex is DeserializationException || ex is MappingException)
                    {
                        throw new CacheItemNotFoundException($"An error occurred while fetching competitor profile for {competitorId}", competitorId.ToString(), ex);
                    }

                    throw;
                }
                finally
                {
                    await ReleaseIdAsync(_fetchedCompetitorProfiles, competitorId).ConfigureAwait(false);
                }
                return cachedItem;
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<CultureInfo, string>> GetPlayerNamesAsync(Urn playerId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing)
        {
            var names = new Dictionary<CultureInfo, string>();
            var cachedItem = (PlayerProfileCacheItem)_cache.Get(playerId.ToString());
            var missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, cachedItem?.Names.Keys);
            if (cachedItem != null && !missingLanguages.Any())
            {
                names = new Dictionary<CultureInfo, string>(cachedItem.Names);
                return names;
            }

            if (fetchIfMissing)
            {
                await GetPlayerProfileAsync(playerId, missingLanguages.ToList(), true).ConfigureAwait(false);
                cachedItem = (PlayerProfileCacheItem)_cache.Get(playerId.ToString());
                if (cachedItem != null)
                {
                    names = new Dictionary<CultureInfo, string>(cachedItem.Names);
                }
            }

            missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, names.Keys);
            if (missingLanguages.Any())
            {
                foreach (var missingLanguage in missingLanguages)
                {
                    names[missingLanguage] = cachedItem != null && cachedItem.Names.ContainsKey(missingLanguage)
                                                 ? cachedItem.GetName(missingLanguage)
                                                 : string.Empty;
                }
            }

            return names;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<CultureInfo, string>> GetCompetitorNamesAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> wantedLanguages, bool fetchIfMissing)
        {
            var names = new Dictionary<CultureInfo, string>();

            var competitorCacheItem = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
            var missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, competitorCacheItem?.Names.Keys);
            if (competitorCacheItem != null && !missingLanguages.Any())
            {
                names = new Dictionary<CultureInfo, string>(competitorCacheItem.Names);
                return names;
            }

            if (fetchIfMissing)
            {
                if (competitorCacheItem?.AssociatedSportEventId != null)
                {
                    await ExecutionHelper.SafeAsync(FetchSummaryForAssociatedEvent, missingLanguages, competitorCacheItem.AssociatedSportEventId).ConfigureAwait(false);

                    missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, competitorCacheItem.Names.Keys);
                    if (!missingLanguages.Any())
                    {
                        names = new Dictionary<CultureInfo, string>(competitorCacheItem.Names);
                        return names;
                    }
                }
                await GetCompetitorProfileAsync(competitorId, missingLanguages.ToList(), true).ConfigureAwait(false);
                competitorCacheItem = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
                if (competitorCacheItem != null)
                {
                    names = new Dictionary<CultureInfo, string>(competitorCacheItem.Names);
                }
            }

            missingLanguages = LanguageHelper.GetMissingCultures(wantedLanguages, names.Keys);
            if (missingLanguages.Any())
            {
                foreach (var missingLanguage in missingLanguages)
                {
                    names[missingLanguage] = competitorCacheItem != null && competitorCacheItem.Names.ContainsKey(missingLanguage)
                                                 ? competitorCacheItem.GetName(missingLanguage)
                                                 : string.Empty;
                }
            }

            return names;
        }

        private async Task FetchSummaryForAssociatedEvent(ICollection<CultureInfo> wantedLanguages, Urn associatedEventId)
        {
            var sportEvent = _sportEventCache.GetEventCacheItem(associatedEventId);
            await sportEvent.GetNamesAsync(wantedLanguages);
        }

        /// <inheritdoc />
        public async Task<string> GetPlayerNameAsync(Urn playerId, CultureInfo wantedLanguage, bool fetchIfMissing)
        {
            var playerNames = await GetPlayerNamesAsync(playerId, new[] { wantedLanguage }, fetchIfMissing).ConfigureAwait(false);
            return playerNames[wantedLanguage];
        }

        /// <inheritdoc />
        public async Task<string> GetCompetitorNameAsync(Urn competitorId, CultureInfo wantedLanguage, bool fetchIfMissing)
        {
            var competitorNames = await GetCompetitorNamesAsync(competitorId, new[] { wantedLanguage }, fetchIfMissing).ConfigureAwait(false);
            return competitorNames[wantedLanguage];
        }

        private async Task WaitTillIdIsAvailableAsync(ConcurrentDictionary<Urn, DateTime> bag, Urn id)
        {
            var stopwatch = Stopwatch.StartNew();
            var expireDate = DateTime.Now.AddSeconds(30);
            while (bag.ContainsKey(id) && DateTime.Now < expireDate)
            {
                await Task.Delay(25).ConfigureAwait(false);
            }
            if (!bag.ContainsKey(id))
            {
                bag.TryAdd(id, DateTime.Now);
            }

            if (stopwatch.ElapsedMilliseconds > 100)
            {
                ExecutionLog.LogDebug("WaitTillIdIsAvailable for {ProfileId} took {ElapsedTime} ms", id.ToString(), stopwatch.ElapsedMilliseconds.ToString());
            }
        }

        private async Task ReleaseIdAsync(ConcurrentDictionary<Urn, DateTime> bag, Urn id)
        {
            var expireDate = DateTime.Now.AddSeconds(5);
            while (bag.ContainsKey(id) && DateTime.Now < expireDate)
            {
                if (!bag.TryRemove(id, out _))
                {
                    await Task.Delay(25).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All
                || cacheItemType == CacheItemType.Competitor
                || cacheItemType == CacheItemType.Player)
            {
                _cache.Remove(id.ToString());
            }
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(Urn id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All
                || cacheItemType == CacheItemType.Competitor
                || cacheItemType == CacheItemType.Player)
            {
                return _cache.Contains(id.ToString());
            }
            return false;
        }

        /// <summary>
        /// Adds the dto item to cache
        /// </summary>
        /// <param name="id">The identifier of the object</param>
        /// <param name="item">The item</param>
        /// <param name="culture">The culture</param>
        /// <param name="dtoType">Type of the dto</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if added, <c>false</c> otherwise</returns>
        //NOSONAR
        protected override async Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.MatchSummary:
                    var competitorsSaved1 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved1)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDto), item.GetType());
                    }
                    break;
                case DtoType.RaceSummary:
                    var competitorsSaved2 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved2)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDto), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfo:
                    var competitorsSaved3 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved3)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentInfoDto), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummary:
                    var competitorsSaved4 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved4)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDto), item.GetType());
                    }
                    break;
                case DtoType.PlayerProfile:
                    if (item is PlayerProfileDto playerProfile)
                    {
                        await AddPlayerProfileAsync(playerProfile, null, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(PlayerProfileDto), item.GetType());
                    }
                    break;
                case DtoType.Competitor:
                    if (item is CompetitorDto competitor)
                    {
                        await AddCompetitorAsync(id, competitor, null, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CompetitorDto), item.GetType());
                    }
                    break;
                case DtoType.CompetitorProfile:
                    if (item is CompetitorProfileDto competitorProfile)
                    {
                        await AddCompetitorProfileAsync(id, competitorProfile, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CompetitorProfileDto), item.GetType());
                    }
                    break;
                case DtoType.SimpleTeamProfile:
                    if (item is SimpleTeamProfileDto simpleTeamProfile)
                    {
                        await AddCompetitorProfileAsync(id, simpleTeamProfile, culture, true).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SimpleTeamProfileDto), item.GetType());
                    }
                    break;
                case DtoType.MatchTimeline:
                    if (item is MatchTimelineDto matchTimeline)
                    {
                        saved = await SaveCompetitorsFromSportEventAsync(matchTimeline.SportEvent, culture).ConfigureAwait(false);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchTimelineDto), item.GetType());
                    }
                    break;
                case DtoType.TournamentSeasons:
                    if (item is TournamentSeasonsDto tournamentSeason)
                    {
                        if (tournamentSeason.Tournament != null)
                        {
                            await SaveCompetitorsFromSportEventAsync(tournamentSeason.Tournament, culture).ConfigureAwait(false);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentSeasonsDto), item.GetType());
                    }
                    break;
                case DtoType.Fixture:
                    var competitorsSaved5 = await SaveCompetitorsFromSportEventAsync(item, culture).ConfigureAwait(false);
                    if (competitorsSaved5)
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(FixtureDto), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummaryList:
                    if (item is EntityList<SportEventSummaryDto> sportEventSummaryList)
                    {
                        var tasks = sportEventSummaryList.Items.Select(s => SaveCompetitorsFromSportEventAsync(s, culture));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDto), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfoList:
                    if (item is EntityList<TournamentInfoDto> ts)
                    {
                        var tasks = ts.Items.Select(s => SaveCompetitorsFromSportEventAsync(s, culture));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<TournamentInfoDto>), item.GetType());
                    }
                    break;
            }
            return saved;
        }

        //NOSONAR
        private async Task<bool> SaveCompetitorsFromSportEventAsync(object item, CultureInfo culture)
        {
            if (item is FixtureDto fixture)
            {
                if (fixture.Competitors != null && fixture.Competitors.Any())
                {
                    var tasks = fixture.Competitors.Select(s => AddTeamCompetitorAsync(s.Id, s, fixture.Id, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                return true;
            }

            if (item is MatchDto match)
            {
                if (match.Competitors != null && match.Competitors.Any())
                {
                    var tasks = match.Competitors.Select(s => AddTeamCompetitorAsync(s.Id, s, match.Id, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                return true;
            }

            if (item is StageDto stage)
            {
                if (stage.Competitors != null && stage.Competitors.Any())
                {
                    var tasks = stage.Competitors.Select(s => AddTeamCompetitorAsync(s.Id, s, stage.Id, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                return true;
            }

            if (item is TournamentInfoDto tour)
            {
                if (tour.Competitors != null && tour.Competitors.Any())
                {
                    var tasks = tour.Competitors.Select(s => AddCompetitorAsync(s.Id, s, tour.Id, culture, true));
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                if (!tour.Groups.IsNullOrEmpty())
                {
                    foreach (var tourGroup in tour.Groups)
                    {
                        if (!tourGroup.Competitors.IsNullOrEmpty())
                        {
                            var tasks = tourGroup.Competitors.Select(s => AddCompetitorAsync(s.Id, s, tour.Id, culture, true));
                            await Task.WhenAll(tasks).ConfigureAwait(false);
                        }
                    }
                }
                return true;
            }

            return false;
        }

        //NOSONAR
        private async Task AddTeamCompetitorAsync(Urn competitorId, TeamCompetitorDto item, Urn associatedSportEventId, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(competitorId.ToString()))
            {
                try
                {
                    var ci = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
                    var teamCacheItem = ci == null ? (TeamCompetitorCacheItem)_cache.Get(competitorId.ToString()) : ci as TeamCompetitorCacheItem;
                    if (teamCacheItem != null)
                    {
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                        }
                        teamCacheItem.Merge(item, culture);
                        teamCacheItem.UpdateAssociatedSportEvent(associatedSportEventId);
                    }
                    else
                    {
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                        }
                        teamCacheItem = new TeamCompetitorCacheItem(ci);
                        teamCacheItem.Merge(item, culture);
                        teamCacheItem.UpdateAssociatedSportEvent(associatedSportEventId);
                        _cache.Add(competitorId.ToString(), teamCacheItem, GetCorrectCacheItemPolicy(competitorId));
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error adding team competitor for id={CompetitorId}, dto type={DtoType} and lang={Language}", competitorId, item?.GetType().Name, culture.TwoLetterISOLanguageName);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var teamCompetitor = new TeamCompetitorCacheItem(item, culture, _dataRouterManager);
                teamCompetitor.UpdateAssociatedSportEvent(associatedSportEventId);
                _cache.Add(competitorId.ToString(), teamCompetitor, GetCorrectCacheItemPolicy(competitorId));
            }

            if (item?.Players != null && item.Players.Any())
            {
                var tasks = item.Players.Select(s => AddPlayerCompetitorAsync(s, item.Id, culture, false));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task AddTeamCompetitorAsync(ExportableTeamCompetitor item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = Urn.Parse(item.Id);
                try
                {
                    var ci = (CompetitorCacheItem)_cache.Get(item.Id);
                    var teamCacheItem = ci == null ? (TeamCompetitorCacheItem)_cache.Get(item.Id) : ci as TeamCompetitorCacheItem;
                    if (teamCacheItem != null)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                        teamCacheItem.Import(item);
                    }
                    else
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                        teamCacheItem = new TeamCompetitorCacheItem(ci);
                        teamCacheItem.Import(item);
                        _cache.Add(item.Id, teamCacheItem, GetCorrectCacheItemPolicy(Urn.Parse(item.Id)));
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error importing team competitor for id={CompetitorId}", item.Id);
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id, new TeamCompetitorCacheItem(item, _dataRouterManager), GetCorrectCacheItemPolicy(Urn.Parse(item.Id)));
            }
        }

        private async Task AddCompetitorAsync(Urn competitorId, CompetitorDto item, Urn associatedSportEventId, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(competitorId.ToString()))
            {
                try
                {
                    var ci = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                    }
                    ci?.Merge(item, culture);
                    ci?.UpdateAssociatedSportEvent(associatedSportEventId);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error adding competitor for id={CompetitorId}, dto type={DtoType} and lang={Language}", competitorId, item?.GetType().Name, culture.TwoLetterISOLanguageName);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                var competitorCi = new CompetitorCacheItem(item, culture, _dataRouterManager);
                competitorCi.UpdateAssociatedSportEvent(associatedSportEventId);
                _cache.Add(competitorId.ToString(), competitorCi, GetCorrectCacheItemPolicy(competitorId));
            }

            if (item?.Players != null && item.Players.Any())
            {
                var tasks = item.Players.Select(s => AddPlayerCompetitorAsync(s, item.Id, culture, false));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task AddCompetitorAsync(ExportableCompetitor item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = Urn.Parse(item.Id);
                try
                {
                    var ci = (CompetitorCacheItem)_cache.Get(item.Id);
                    await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    ci?.Import(item);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error importing competitor for id={CompetitorId}", item.Id);
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(item.Id))
                {
                    _cache.Add(item.Id, new CompetitorCacheItem(item, _dataRouterManager), GetCorrectCacheItemPolicy(Urn.Parse(item.Id)));
                }
            }
        }

        private async Task AddCompetitorProfileAsync(Urn competitorId, CompetitorProfileDto item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(competitorId.ToString()))
            {
                try
                {
                    var ci = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                    }
                    ci?.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error adding competitor for id={CompetitorId}, dto type={DtoType} and lang={Language}", competitorId, item?.GetType().Name, culture.TwoLetterISOLanguageName);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(competitorId.ToString(), new CompetitorCacheItem(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(competitorId));
            }

            if (item?.Players != null && item.Players.Any())
            {
                var tasks = item.Players.Select(s => AddPlayerProfileAsync(s, competitorId, culture, true));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task AddCompetitorProfileAsync(Urn competitorId, SimpleTeamProfileDto item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(competitorId.ToString()))
            {
                try
                {
                    var ci = (CompetitorCacheItem)_cache.Get(competitorId.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                    }
                    ci.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error adding competitor for id={CompetitorId}, dto type={DtoType} and lang={Language}", competitorId, item?.GetType().Name, culture.TwoLetterISOLanguageName);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, competitorId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(competitorId.ToString(), new CompetitorCacheItem(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(competitorId));
            }
        }

        private async Task AddPlayerProfileAsync(PlayerProfileDto item, Urn competitorId, CultureInfo culture, bool useSemaphore)
        {
            if (item == null)
            {
                return;
            }
            if (_cache.Contains(item.Id.ToString()))
            {
                try
                {
                    var ci = (PlayerProfileCacheItem)_cache.Get(item.Id.ToString());
                    if (useSemaphore)
                    {
                        await WaitTillIdIsAvailableAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                    }
                    ci?.Merge(item, competitorId, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error adding player profile for id={PlayerId}, dto type={DtoType} and lang={Language}", item.Id, item.GetType().Name, culture.TwoLetterISOLanguageName);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id.ToString(), new PlayerProfileCacheItem(item, competitorId, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
            }
        }

        private async Task AddPlayerProfileAsync(ExportablePlayerProfile item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = Urn.Parse(item.Id);
                try
                {
                    var ci = (PlayerProfileCacheItem)_cache.Get(item.Id);
                    await WaitTillIdIsAvailableAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    ci?.Import(item);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error importing player profile for id={PlayerId}", item.Id);
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, itemId).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id, new PlayerProfileCacheItem(item, _dataRouterManager), GetCorrectCacheItemPolicy(Urn.Parse(item.Id)));
            }
        }

        //NOSONAR
        private async Task AddPlayerCompetitorAsync(PlayerCompetitorDto item, Urn competitorId, CultureInfo culture, bool useSemaphore)
        {
            if (item == null)
            {
                return;
            }
            if (_cache.Contains(item.Id.ToString()))
            {
                try
                {
                    if (item.Id.Type.Equals(SdkInfo.PlayerProfileIdentifier, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var ci = (PlayerProfileCacheItem)_cache.Get(item.Id.ToString());
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                        }

                        ci?.Merge(item, competitorId, culture);
                    }
                    else
                    {
                        var ci = (CompetitorCacheItem)_cache.Get(item.Id.ToString());
                        if (useSemaphore)
                        {
                            await WaitTillIdIsAvailableAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                        }

                        ci?.Merge(item, culture);
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Error adding player profile for id={PlayerId}, dto type={DtoType} and lang={LanguageName}", item.Id, item.GetType().Name, culture.TwoLetterISOLanguageName);
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        await ReleaseIdAsync(_mergeUrns, item.Id).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                if (item.Id.Type.Equals(SdkInfo.PlayerProfileIdentifier, StringComparison.InvariantCultureIgnoreCase))
                {
                    _cache.Add(item.Id.ToString(), new PlayerProfileCacheItem(item, competitorId, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
                }
                else
                {
                    _cache.Add(item.Id.ToString(), new CompetitorCacheItem(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
                }
            }
        }

        private CacheItemPriority GetCorrectCacheItemPolicy(Urn id)
        {
            return id.IsSimpleTeam() ? CacheItemPriority.NeverRemove : CacheItemPriority.Normal;
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableBase"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableBase>> ExportAsync()
        {
            IEnumerable<IExportableBase> exportable;
            try
            {
                await _exportSemaphore.WaitAsync();
                exportable = _cache.GetValues().Select(i => (IExportableBase)i);
            }
            finally
            {
                _exportSemaphore.ReleaseSafe();
            }

            var tasks = exportable.Select(e => e.ExportAsync());

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Imports provided items into the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableBase"/> to be inserted into the cache</param>
        public Task ImportAsync(IEnumerable<ExportableBase> items)
        {
            var tasks = new List<Task>();
            foreach (var exportable in items)
            {
                if (exportable is ExportableTeamCompetitor exportableTeamCompetitor)
                {
                    var task = AddTeamCompetitorAsync(exportableTeamCompetitor);
                    tasks.Add(task);
                    continue;
                }
                if (exportable is ExportableCompetitor exportableCompetitor)
                {
                    var task = AddCompetitorAsync(exportableCompetitor);
                    tasks.Add(task);
                    continue;
                }
                if (exportable is ExportablePlayerProfile exportablePlayerProfile)
                {
                    var task = AddPlayerProfileAsync(exportablePlayerProfile);
                    tasks.Add(task);
                }
            }

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{K, V}"/> containing all cache item types in the cache and their counts</returns>
        public IReadOnlyDictionary<string, int> CacheStatus()
        {
            IReadOnlyCollection<object> items;
            try
            {
                _exportSemaphore.Wait();
                items = _cache.GetValues();
            }
            finally
            {
                _exportSemaphore.ReleaseSafe();
            }

            return new Dictionary<string, int>
                   {
                       { nameof(TeamCompetitorCacheItem), items.Count(i => i.GetType() == typeof(TeamCompetitorCacheItem)) }, { nameof(CompetitorCacheItem), items.Count(i => i.GetType() == typeof(CompetitorCacheItem)) },
                       { nameof(PlayerProfileCacheItem), items.Count(i => i.GetType() == typeof(PlayerProfileCacheItem)) }
                   };
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var keys = _cache.GetKeys();
            var details =
                $" [Players: {keys.Count(c => c.Contains("player")).ToString()}, Competitors: {keys.Count(c => c.Contains("competitor")).ToString()}, Teams: {keys.Count(c => c.Equals("team")).ToString()}, SimpleTeams: {keys.Count(Urn.IsSimpleTeam).ToString()}]";

            return Task.FromResult(HealthCheckResult.Healthy($"Cache has {_cache.Count().ToString()} items{details}"));
        }
    }
}
