/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Health;
using App.Metrics.Timer;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles
{
    /// <summary>
    /// A <see cref="IProfileCache"/> implementation using <see cref="MemoryCache"/> to cache fetched information
    /// </summary>
    internal class ProfileCache : SdkCache, IProfileCache
    {
        private static readonly ILogger LogCache = SdkLoggerFactory.GetLoggerForCache(typeof(ProfileCache));

        /// <summary>
        /// A <see cref="MemoryCache"/> used to store fetched information
        /// </summary>
        private readonly MemoryCache _cache;

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Value indicating whether the current instance was already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// The bag of currently fetching player profiles
        /// </summary>
        private readonly ConcurrentDictionary<URN, DateTime> _fetchedPlayerProfiles = new ConcurrentDictionary<URN, DateTime>();

        /// <summary>
        /// The bag of currently fetching competitor profiles
        /// </summary>
        private readonly ConcurrentDictionary<URN, DateTime> _fetchedCompetitorProfiles = new ConcurrentDictionary<URN, DateTime>();
        
        /// <summary>
        /// The bag of currently merging ids
        /// </summary>
        private readonly ConcurrentDictionary<URN, DateTime> _mergeUrns = new ConcurrentDictionary<URN, DateTime>();

        /// <summary>
        /// The semaphore used for export/import operation
        /// </summary>
        private readonly SemaphoreSlim _exportSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// The cache item policy
        /// </summary>
        private readonly CacheItemPolicy _simpleTeamCacheItemPolicy = new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.MaxValue, Priority = CacheItemPriority.NotRemovable, RemovedCallback = OnCacheItemRemoval};

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileCache"/> class
        /// </summary>
        /// <param name="cache">A <see cref="MemoryCache"/> used to store fetched information</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to fetch data</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        public ProfileCache(MemoryCache cache,
                            IDataRouterManager dataRouterManager,
                            ICacheManager cacheManager)
            : base(cacheManager, null)
        {
            Guard.Argument(cache, nameof(cache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _cache = cache;
            _dataRouterManager = dataRouterManager;
            _isDisposed = false;
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
        /// Asynchronously gets a <see cref="PlayerProfileCI"/> representing the profile for the specified player
        /// </summary>
        /// <param name="playerId">A <see cref="URN"/> specifying the id of the player for which to get the profile</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <returns>A <see cref="Task{PlayerProfileCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "<Pending>")]
        public async Task<PlayerProfileCI> GetPlayerProfileAsync(URN playerId, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(playerId, nameof(playerId)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull();
            if (!cultures.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(cultures));
            }

            var timerOptions = new TimerOptions { Context = "ProfileCache", Name = "GetPlayerProfileAsync", MeasurementUnit = Unit.Requests };
            using (SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(timerOptions, $"{playerId}"))
            {
                WaitTillIdIsAvailable(_fetchedPlayerProfiles, playerId);

                PlayerProfileCI cachedItem;
                try
                {
                    cachedItem = (PlayerProfileCI) _cache.Get(playerId.ToString());
                    var wantedCultures = cultures.ToList();
                    var missingLanguages = LanguageHelper.GetMissingCultures(wantedCultures, cachedItem?.Names.Keys.ToList()).ToList();
                    if (!missingLanguages.Any())
                    {
                        return cachedItem;
                    }

                    // try to fetch for competitor, to avoid requests by each player
                    if (cachedItem?.CompetitorId != null)
                    {
                        var competitorCI = (CompetitorCI) _cache.Get(cachedItem.CompetitorId.ToString());
                        if (competitorCI != null &&
                            (competitorCI.LastTimeCompetitorProfileFetched < DateTime.Now.AddSeconds(-30)
                             || LanguageHelper.GetMissingCultures(wantedCultures, competitorCI.CultureCompetitorProfileFetched.ToList()).Any()))
                        {
                            ExecutionLog.LogDebug($"Fetching competitor profile for competitor {competitorCI.Id} instead of player {cachedItem.Id} for languages=[{string.Join(",", missingLanguages.Select(s => s.TwoLetterISOLanguageName))}].");

                            try
                            {
                                WaitTillIdIsAvailable(_fetchedCompetitorProfiles, cachedItem.CompetitorId);

                                var cultureTasks = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetCompetitorAsync(competitorCI.Id, c, null));
                                await Task.WhenAll(cultureTasks.Values).ConfigureAwait(false);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            finally
                            {
                                if (!_isDisposed)
                                {
                                    ReleaseId(_fetchedCompetitorProfiles, cachedItem.CompetitorId);
                                }
                            }

                            cachedItem = (PlayerProfileCI) _cache.Get(playerId.ToString());
                            missingLanguages = LanguageHelper.GetMissingCultures(wantedCultures, cachedItem?.Names.Keys.ToList()).ToList();
                            if (!missingLanguages.Any())
                            {
                                return cachedItem;
                            }
                        }
                    }
                    
                    var cultureTaskDictionary = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetPlayerProfileAsync(playerId, c, null));
                    await Task.WhenAll(cultureTaskDictionary.Values).ConfigureAwait(false);

                    cachedItem = (PlayerProfileCI) _cache.Get(playerId.ToString());
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
                    ReleaseId(_fetchedPlayerProfiles, playerId);
                }
                
                return cachedItem;
            }
        }

        /// <summary>
        /// Asynchronously gets <see cref="CompetitorCI"/> representing the profile for the specified competitor
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> specifying the id of the competitor for which to get the profile</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages in which the information should be available</param>
        /// <returns>A <see cref="Task{PlayerProfileCI}"/> representing the asynchronous operation</returns>
        /// <exception cref="CacheItemNotFoundException">The requested item was not found in cache and could not be obtained from the API</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4457:Parameter validation in \"async\"/\"await\" methods should be wrapped", Justification = "<Pending>")]
        public async Task<CompetitorCI> GetCompetitorProfileAsync(URN competitorId, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(competitorId, nameof(competitorId)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull();
            if (!cultures.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(cultures));
            }

            var timerOptions = new TimerOptions { Context = "ProfileCache", Name = "GetCompetitorProfileAsync", MeasurementUnit = Unit.Requests };
            using (SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(timerOptions, $"{competitorId}"))
            {
                WaitTillIdIsAvailable(_fetchedCompetitorProfiles, competitorId);

                CompetitorCI cachedItem;
                try
                {
                    cachedItem = (CompetitorCI) _cache.Get(competitorId.ToString());
                    var missingLanguages = LanguageHelper.GetMissingCultures(cultures.ToList(), cachedItem?.Names.Keys.ToList()).ToList();
                    if (!missingLanguages.Any())
                    {
                        return cachedItem;
                    }
                    
                    var cultureTasks = missingLanguages.ToDictionary(c => c, c => _dataRouterManager.GetCompetitorAsync(competitorId, c, null));
                    await Task.WhenAll(cultureTasks.Values).ConfigureAwait(false);
                    cachedItem = (CompetitorCI) _cache.Get(competitorId.ToString());
                }
                catch (Exception ex)
                {
                    if (ex is DeserializationException || ex is MappingException)
                    {
                        throw new CacheItemNotFoundException("An error occurred while fetching competitor profile not found in cache", competitorId.ToString(), ex);
                    }

                    throw;
                }
                finally
                {
                    ReleaseId(_fetchedCompetitorProfiles, competitorId);
                }
                return cachedItem;
            }
        }

        private void WaitTillIdIsAvailable(ConcurrentDictionary<URN, DateTime> bag, URN id)
        {
            var expireDate = DateTime.Now.AddSeconds(5);
            while (bag.ContainsKey(id) && DateTime.Now < expireDate)
            {
                Thread.Sleep(15);
            }
            if (!bag.ContainsKey(id))
            {
                bag.TryAdd(id, DateTime.Now);
            }
        }

        private void ReleaseId(ConcurrentDictionary<URN, DateTime> bag, URN id)
        {
            var expireDate = DateTime.Now.AddSeconds(5);
            while (bag.ContainsKey(id) && DateTime.Now < expireDate)
            {
                if (!bag.TryRemove(id, out _))
                {
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = new HealthCheck(CacheName, StartHealthCheck);
        }

        private ValueTask<HealthCheckResult> StartHealthCheck()
        {
            var keys = _cache.Select(w => w.Key).ToList();
            var details = $" [Players: {keys.Count(c => c.Contains("player"))}, CompetitorsIds: {keys.Count(c => c.Contains("competitor"))}, Teams: {keys.Count(c => c.Equals("team"))}, SimpleTeams: {keys.Count(URN.IsSimpleTeam)}]";

            return _cache.Any()
                ? new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy($"Cache has { _cache.Count() } items{ details}."))
                : new ValueTask<HealthCheckResult>(HealthCheckResult.Unhealthy("Cache is empty."));
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.Fixture,
                                     DtoType.MatchTimeline,
                                     DtoType.Competitor,
                                     DtoType.CompetitorProfile,
                                     DtoType.SimpleTeamProfile,
                                     DtoType.PlayerProfile,
                                     DtoType.SportEventSummary,
                                     DtoType.TournamentInfo,
                                     DtoType.RaceSummary,
                                     DtoType.MatchSummary,
                                     DtoType.TournamentInfoList,
                                     DtoType.TournamentSeasons,
                                     DtoType.SportEventSummaryList
                                 };
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(URN id, CacheItemType cacheItemType)
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
        /// <param name="id">A <see cref="URN" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(URN id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All
                || cacheItemType == CacheItemType.Competitor
                || cacheItemType == CacheItemType.Player)
            {
                return _cache.Contains(id.ToString());
            }
            return false;
        }

        private static void OnCacheItemRemoval(CacheEntryRemovedArguments arguments)
        {
            if (arguments.RemovedReason != CacheEntryRemovedReason.Removed && arguments.RemovedReason != CacheEntryRemovedReason.CacheSpecificEviction)
            {
                LogCache.LogDebug($"{arguments.RemovedReason} from cache: {arguments.CacheItem.Key}");
            }
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
        protected override bool CacheAddDtoItem(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.MatchSummary:
                    if (SaveCompetitorsFromSportEvent(item, culture))
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDTO), item.GetType());
                    }
                    break;
                case DtoType.RaceSummary:
                    if (SaveCompetitorsFromSportEvent(item, culture))
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfo:
                    if (SaveCompetitorsFromSportEvent(item, culture))
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentInfoDTO), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummary:
                    if (SaveCompetitorsFromSportEvent(item, culture))
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDTO), item.GetType());
                    }
                    break;
                case DtoType.Sport:
                    break;
                case DtoType.Category:
                    break;
                case DtoType.Tournament:
                    break;
                case DtoType.PlayerProfile:
                    var playerProfile = item as PlayerProfileDTO;
                    if (playerProfile != null)
                    {
                        AddPlayerProfile(playerProfile, null, culture, true);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(PlayerProfileDTO), item.GetType());
                    }
                    break;
                case DtoType.Competitor:
                    var competitor = item as CompetitorDTO;
                    if (competitor != null)
                    {
                        AddCompetitor(id, competitor, culture, true);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CompetitorDTO), item.GetType());
                    }
                    break;
                case DtoType.CompetitorProfile:
                    var competitorProfile = item as CompetitorProfileDTO;
                    if (competitorProfile != null)
                    {
                        AddCompetitorProfile(id, competitorProfile, culture, true);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CompetitorProfileDTO), item.GetType());
                    }
                    break;
                case DtoType.SimpleTeamProfile:
                    var simpleTeamProfile = item as SimpleTeamProfileDTO;
                    if (simpleTeamProfile != null)
                    {
                        AddCompetitorProfile(id, simpleTeamProfile, culture, true);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SimpleTeamProfileDTO), item.GetType());
                    }
                    break;
                case DtoType.MarketDescription:
                    break;
                case DtoType.SportEventStatus:
                    break;
                case DtoType.MatchTimeline:
                    var matchTimeline = item as MatchTimelineDTO;
                    if (matchTimeline != null)
                    {
                        saved = SaveCompetitorsFromSportEvent(matchTimeline.SportEvent, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchTimelineDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentSeasons:
                    var tournamentSeason = item as TournamentSeasonsDTO;
                    if (tournamentSeason?.Tournament != null)
                    {
                        SaveCompetitorsFromSportEvent(tournamentSeason.Tournament, culture);
                        saved = true;
                    }
                    break;
                case DtoType.Fixture:
                    if (SaveCompetitorsFromSportEvent(item, culture))
                    {
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(FixtureDTO), item.GetType());
                    }
                    break;
                case DtoType.SportList:
                    break;
                case DtoType.SportEventSummaryList:
                    var sportEventSummaryList = item as EntityList<SportEventSummaryDTO>;
                    if (sportEventSummaryList != null)
                    {
                        foreach (var sportEventSummary in sportEventSummaryList.Items)
                        {
                            SaveCompetitorsFromSportEvent(sportEventSummary, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDTO), item.GetType());
                    }
                    break;
                case DtoType.MarketDescriptionList:
                    break;
                case DtoType.VariantDescription:
                    break;
                case DtoType.VariantDescriptionList:
                    break;
                case DtoType.Lottery:
                    break;
                case DtoType.LotteryDraw:
                    break;
                case DtoType.LotteryList:
                    break;
                case DtoType.BookingStatus:
                    break;
                case DtoType.SportCategories:
                    break;
                case DtoType.AvailableSelections:
                    break;
                case DtoType.TournamentInfoList:
                    var ts = item as EntityList<TournamentInfoDTO>;
                    if (ts != null)
                    {
                        foreach (var t1 in ts.Items)
                        {
                            SaveCompetitorsFromSportEvent(t1, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<TournamentInfoDTO>), item.GetType());
                    }
                    break;
                default:
                    ExecutionLog.LogWarning($"Trying to add unchecked dto type: {dtoType} for id: {id}.");
                    break;
            }
            return saved;
        }

        private bool SaveCompetitorsFromSportEvent(object item, CultureInfo culture)
        {
            if (item is FixtureDTO fixture)
            {
                if (fixture.Competitors != null && fixture.Competitors.Any())
                {
                    foreach (var teamCompetitorDTO in fixture.Competitors)
                    {
                        AddTeamCompetitor(teamCompetitorDTO.Id, teamCompetitorDTO, culture, true);
                    }
                }
                return true;
            }

            if (item is MatchDTO match)
            {
                if (match.Competitors != null && match.Competitors.Any())
                {
                    foreach (var teamCompetitorDTO in match.Competitors)
                    {
                        AddTeamCompetitor(teamCompetitorDTO.Id, teamCompetitorDTO, culture, true);
                    }
                }
                return true;
            }

            if(item is StageDTO stage)
            {
                if (stage.Competitors != null && stage.Competitors.Any())
                {
                    foreach (var teamCompetitorDTO in stage.Competitors)
                    {
                        AddTeamCompetitor(teamCompetitorDTO.Id, teamCompetitorDTO, culture, true);
                    }
                }
                return true;
            }

            if (item is TournamentInfoDTO tour)
            {
                if (tour.Competitors != null && tour.Competitors.Any())
                {
                    foreach (var competitorDTO in tour.Competitors)
                    {
                        AddCompetitor(competitorDTO.Id, competitorDTO, culture, true);
                    }
                }
                if(!tour.Groups.IsNullOrEmpty())
                {
                    foreach (var tourGroup in tour.Groups)
                    {
                        if(!tourGroup.Competitors.IsNullOrEmpty())
                        {
                            foreach (var tourGroupCompetitorDTO in tourGroup.Competitors)
                            {
                                AddCompetitor(tourGroupCompetitorDTO.Id, tourGroupCompetitorDTO, culture, true);
                            }
                        }
                    }
                }
                return true;
            }

            return false;
        }

        private void AddTeamCompetitor(URN id, TeamCompetitorDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI)_cache.Get(id.ToString());
                    var teamCI = ci as TeamCompetitorCI;
                    if (teamCI != null)
                    {
                        if (useSemaphore)
                        {
                            WaitTillIdIsAvailable(_mergeUrns, id);
                        }
                        teamCI.Merge(item, culture);
                    }
                    else
                    {
                        if (useSemaphore)
                        {
                            WaitTillIdIsAvailable(_mergeUrns, id);
                        }
                        teamCI = new TeamCompetitorCI(ci);
                        teamCI.Merge(item, culture);
                        _cache.Set(id.ToString(), teamCI, GetCorrectCacheItemPolicy(id));
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error adding team competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        ReleaseId(_mergeUrns, id);
                    }
                }
            }
            else
            {
                _cache.Add(id.ToString(), new TeamCompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(id));
            }

            if (item?.Players != null && item.Players.Any())
            {
                foreach (var player in item.Players)
                {
                    AddPlayerCompetitor(player, item.Id, culture, false);
                }
            }
        }

        private void AddTeamCompetitor(ExportableTeamCompetitorCI item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = URN.Parse(item.Id);
                try
                {
                    var ci = (CompetitorCI) _cache.Get(item.Id);
                    var teamCI = ci as TeamCompetitorCI;
                    if (teamCI != null)
                    {
                        WaitTillIdIsAvailable(_mergeUrns, itemId);
                        teamCI.Import(item);
                    }
                    else
                    {
                        WaitTillIdIsAvailable(_mergeUrns, itemId);
                        teamCI = new TeamCompetitorCI(ci);
                        teamCI.Import(item);
                        _cache.Set(item.Id, teamCI, GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error importing team competitor for id={item.Id}.");
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        ReleaseId(_mergeUrns, itemId);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id, new TeamCompetitorCI(item, _dataRouterManager), GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
            }
        }

        private void AddCompetitor(URN id, CompetitorDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI) _cache.Get(id.ToString());
                    if (useSemaphore)
                    {
                        WaitTillIdIsAvailable(_mergeUrns, id);
                    }
                    ci?.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error adding competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        ReleaseId(_mergeUrns, id);
                    }
                }
            }
            else
            {
                _cache.Add(id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(id));
            }

            if (item?.Players != null && item.Players.Any())
            {
                foreach (var player in item.Players)
                {
                    AddPlayerCompetitor(player, item.Id, culture, false);
                }
            }
        }

        private void AddCompetitor(ExportableCompetitorCI item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = URN.Parse(item.Id);
                try
                {
                    var ci = (CompetitorCI) _cache.Get(item.Id);
                    WaitTillIdIsAvailable(_mergeUrns, itemId);
                    ci?.Import(item);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error importing competitor for id={item.Id}.");
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        ReleaseId(_mergeUrns, itemId);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(item.Id))
                {
                    _cache.Add(item.Id, new CompetitorCI(item, _dataRouterManager), GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
                }
            }
        }

        private void AddCompetitorProfile(URN id, CompetitorProfileDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI) _cache.Get(id.ToString());
                    if (useSemaphore)
                    {
                        WaitTillIdIsAvailable(_mergeUrns, id);
                    }
                    ci?.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error adding competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        ReleaseId(_mergeUrns, id);
                    }
                }

                if (item?.Players != null && item.Players.Any())
                {
                    foreach (var player in item.Players)
                    {
                        AddPlayerProfile(player, id, culture, false);
                    }
                }
            }
            else
            {
                _cache.Add(id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(id));

                if (item.Players != null && item.Players.Any())
                {
                    foreach (var player in item.Players)
                    {
                        AddPlayerProfile(player, id, culture, false);
                    }
                }
            }
        }

        private void AddCompetitorProfile(URN id, SimpleTeamProfileDTO item, CultureInfo culture, bool useSemaphore)
        {
            if (_cache.Contains(id.ToString()))
            {
                try
                {
                    var ci = (CompetitorCI)_cache.Get(id.ToString());
                    if (useSemaphore)
                    {
                        WaitTillIdIsAvailable(_mergeUrns, id);
                    }
                    ci?.Merge(item, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error adding competitor for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        ReleaseId(_mergeUrns, id);
                    }
                }
            }
            else
            {
                _cache.Add(id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(id));
            }
        }

        private void AddPlayerProfile(PlayerProfileDTO item, URN competitorId, CultureInfo culture, bool useSemaphore)
        {
            if (item == null)
            {
                return;
            }
            if (_cache.Contains(item.Id.ToString()))
            {
                try
                {
                    var ci = (PlayerProfileCI) _cache.Get(item.Id.ToString());
                    if (useSemaphore)
                    {
                        WaitTillIdIsAvailable(_mergeUrns, item.Id);
                    }
                    ci?.Merge(item, competitorId, culture);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error adding player profile for id={item.Id}, dto type={item.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        ReleaseId(_mergeUrns, item.Id);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id.ToString(), new PlayerProfileCI(item, competitorId, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
            }
        }

        private void AddPlayerProfile(ExportablePlayerProfileCI item)
        {
            if (_cache.Contains(item.Id))
            {
                var itemId = URN.Parse(item.Id);
                try
                {
                    var ci = (PlayerProfileCI) _cache.Get(item.Id);
                    WaitTillIdIsAvailable(_mergeUrns, itemId);
                    ci?.Import(item);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error importing player profile for id={item.Id}.");
                }
                finally
                {
                    if (!_isDisposed)
                    {
                        ReleaseId(_mergeUrns, itemId);
                    }
                }
            }
            else
            {
                _cache.Add(item.Id, new PlayerProfileCI(item, _dataRouterManager), GetCorrectCacheItemPolicy(URN.Parse(item.Id)));
            }
        }

        private void AddPlayerCompetitor(PlayerCompetitorDTO item, URN competitorId, CultureInfo culture, bool useSemaphore)
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
                        var ci = (PlayerProfileCI)_cache.Get(item.Id.ToString());
                        if (useSemaphore)
                        {
                            WaitTillIdIsAvailable(_mergeUrns, item.Id);
                        }

                        ci?.Merge(item, competitorId, culture);
                    }
                    else
                    {
                        var ci = (CompetitorCI)_cache.Get(item.Id.ToString());
                        if (useSemaphore)
                        {
                            WaitTillIdIsAvailable(_mergeUrns, item.Id);
                        }

                        ci?.Merge(item, culture);
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, $"Error adding player profile for id={item.Id}, dto type={item.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
                }
                finally
                {
                    if (useSemaphore && !_isDisposed)
                    {
                        ReleaseId(_mergeUrns, item.Id);
                    }
                }
            }
            else
            {
                if (item.Id.Type.Equals(SdkInfo.PlayerProfileIdentifier, StringComparison.InvariantCultureIgnoreCase))
                {
                    _cache.Add(item.Id.ToString(), new PlayerProfileCI(item, competitorId, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
                }
                else
                {
                    _cache.Add(item.Id.ToString(), new CompetitorCI(item, culture, _dataRouterManager), GetCorrectCacheItemPolicy(item.Id));
                }
            }
        }

        private CacheItemPolicy GetCorrectCacheItemPolicy(URN id)
        {
            return id.IsSimpleTeam()
                       ? _simpleTeamCacheItemPolicy
                       : new CacheItemPolicy {SlidingExpiration = OperationManager.ProfileCacheTimeout, RemovedCallback = OnCacheItemRemoval};
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableCI"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableCI>> ExportAsync()
        {
            IEnumerable<IExportableCI> exportable; 
            try
            {
                await _exportSemaphore.WaitAsync();
                exportable = _cache.Select(i => (IExportableCI) i.Value);
            }
            finally
            {
                _exportSemaphore.ReleaseSafe();
            }

            var tasks = exportable.Select(e =>
            {
                var task = e.ExportAsync();
                task.ConfigureAwait(false);
                return task;
            });

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Imports provided items into the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableCI"/> to be inserted into the cache</param>
        public Task ImportAsync(IEnumerable<ExportableCI> items)
        {
            foreach (var exportable in items)
            {
                if (exportable is ExportableTeamCompetitorCI exportableTeamCompetitor)
                {
                    AddTeamCompetitor(exportableTeamCompetitor);
                    continue;
                }
                if (exportable is ExportableCompetitorCI exportableCompetitor)
                {
                    AddCompetitor(exportableCompetitor);
                    continue;
                }
                if (exportable is ExportablePlayerProfileCI exportablePlayerProfile)
                {
                    AddPlayerProfile(exportablePlayerProfile);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{K, V}"/> containing all cache item types in the cache and their counts</returns>
        public IReadOnlyDictionary<string, int> CacheStatus()
        {
            List<KeyValuePair<string, object>> items;
            try
            {
                _exportSemaphore.Wait();
                items = _cache.ToList();
            }
            finally
            {
                _exportSemaphore.ReleaseSafe();
            }

            return new Dictionary<string, int>
            {
                {typeof(TeamCompetitorCI).Name, items.Count(i => i.Value.GetType() == typeof(TeamCompetitorCI))},
                {typeof(CompetitorCI).Name, items.Count(i => i.Value.GetType() == typeof(CompetitorCI))},
                {typeof(PlayerProfileCI).Name, items.Count(i => i.Value.GetType() == typeof(PlayerProfileCI))}
            };
        }

        HealthCheckResult IHealthStatusProvider.StartHealthCheck()
        {
            return HealthCheckResult.Healthy("default");
        }
    }
}
