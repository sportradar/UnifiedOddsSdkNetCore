// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
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
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// A cache storing sport, category and tournament related information
    /// </summary>
    /// <seealso cref="ISportDataCache" />
    internal class SportDataCache : SdkCache, ISportDataCache
    {
        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying cultures in which the sport data is periodically fetched
        /// </summary>
        private readonly IReadOnlyCollection<CultureInfo> _requiredCultures;

        /// <summary>
        /// The <see cref="ISet{CultureInfo}"/> specifying cultures in which all sport data is currently available
        /// </summary>
        internal readonly ISet<CultureInfo> FetchedCultures;

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> implementation to avoid multiple fetches from different threads
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// A <see cref="object"/> used to synchronize merging on cache item
        /// </summary>
        private readonly object _mergeLock = new object();

        /// <summary>
        /// A <see cref="IDictionary{Urn, SportCacheItem}"/> used to cache fetched sports
        /// </summary>
        internal readonly IDictionary<Urn, SportCacheItem> Sports;

        /// <summary>
        /// A <see cref="IDictionary{Urn, CategoryCacheItem}"/> used to cache fetched categories
        /// </summary>
        internal readonly IDictionary<Urn, CategoryCacheItem> Categories;

        /// <summary>
        /// Value indicating whether the required data was already fetched automatically(by internal timer)
        /// </summary>
        private bool _wasDataAutoFetched;

        /// <summary>
        /// Value specifying whether the current instance is disposed
        /// </summary>
        private bool _isDisposed;

        private readonly ISdkTimer _timer;

        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDataCache"/> class
        /// </summary>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to fetch all required data</param>
        /// <param name="timer">A <see cref="ISdkTimer"/> instance used to periodically fetch sport related data</param>
        /// <param name="cultures">A list of <see cref="CultureInfo"/> of the cached data</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> containing also tournament data</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="loggerFactory">The logger factory for creating Cache and Execution logs</param>
        public SportDataCache(IDataRouterManager dataRouterManager,
                              ISdkTimer timer,
                              IReadOnlyCollection<CultureInfo> cultures,
                              ISportEventCache sportEventCache,
                              ICacheManager cacheManager,
                              ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(timer, nameof(timer)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();

            _dataRouterManager = dataRouterManager;
            _requiredCultures = cultures;

            FetchedCultures = new HashSet<CultureInfo>();
            Sports = new ConcurrentDictionary<Urn, SportCacheItem>();
            Categories = new ConcurrentDictionary<Urn, CategoryCacheItem>();
            _sportEventCache = sportEventCache;

            _isDisposed = false;

            _timer = timer;
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        /// <summary>
        /// Fetches the data for pre-configured languages and merges it to internally used dictionaries. First time the method is invoked it fetches the data only for missing languages.
        /// On subsequent calls data for all configured languages is fetched and dictionary are cleared before fetched data is added / merged
        /// </summary>
        /// <param name="sender">A <see cref="ISdkTimer"/> invoking the method</param>
        /// <param name="e">The <see cref="EventArgs"/> providing additional information about the event which invoked the method</param>
        private async void OnTimerElapsed(object sender, EventArgs e)
        {
            if (!await _semaphore.WaitAsyncSafe(TimeSpan.FromMinutes(1)).ConfigureAwait(false))
            {
                _semaphore.ReleaseSafe();
                return;
            }
            IList<CultureInfo> cultureInfos = new List<CultureInfo>();

            try
            {
                var missingLanguages = _wasDataAutoFetched
                    ? _requiredCultures
                    : _requiredCultures.Where(c => !FetchedCultures.Any()).ToList();

                cultureInfos = missingLanguages as IList<CultureInfo> ?? missingLanguages.ToList();
                if (cultureInfos.Any())
                {
                    await FetchAndMergeAll(new ReadOnlyCollection<CultureInfo>(cultureInfos), _wasDataAutoFetched).ConfigureAwait(false);
                    ExecutionLog.LogInformation("Sport data for languages [{Langs}] successfully fetched and merged", string.Join(",", cultureInfos));
                    _wasDataAutoFetched = true;
                }
            }
            catch (FeedSdkException ex)
            {
                ExecutionLog.LogWarning(ex, "An exception occurred while attempting to fetch sport data for: {Langs}", string.Join(",", cultureInfos));
            }
            catch (ObjectDisposedException)
            {
                ExecutionLog.LogWarning("An exception occurred while attempting to fetch sport data for: {Langs}. DataProvider was already disposed", string.Join(",", cultureInfos));
            }
            catch (TaskCanceledException)
            {
                ExecutionLog.LogWarning("An exception occurred while attempting to fetch sport data for: {Langs}. Task canceled", string.Join(",", cultureInfos));
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning(ex, "An exception occurred while attempting to fetch sport data for: {Langs}", string.Join(",", cultureInfos));
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }
        }

        /// <summary>
        /// Fetches the data for languages specified by <c>cultures</c> and merges / adds it to internally used dictionaries.
        /// </summary>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}" /> specifying the languages in which to fetch the data</param>
        /// <param name="clearExistingData">Value indicating whether the internally used dictionaries should be cleared before fetched data is added</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        private async Task FetchAndMergeAll(IReadOnlyCollection<CultureInfo> cultures, bool clearExistingData)
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            using (var telemetryTracker = new TelemetryTracker(UofSdkTelemetry.SportDataCacheGetAll))
            {
                if (clearExistingData)
                {
                    _sportEventCache.DeleteSportEventsFromCache(DateTime.Now.AddHours(-12));
                }

                var fetchTasks = cultures.Select(c => _dataRouterManager.GetAllSportsAsync(c)).ToList();
                fetchTasks.AddRange(cultures.Select(c => _dataRouterManager.GetAllTournamentsForAllSportAsync(c)).ToList());
                fetchTasks.AddRange(cultures.Select(c => _dataRouterManager.GetAllLotteriesAsync(c, true)).ToList());

                await Task.WhenAll(fetchTasks).ConfigureAwait(false);

                foreach (var culture in cultures)
                {
                    FetchedCultures.Add(culture);
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="SportData"/> representing the parent sport of the tournament specified by <c>tournamentId</c> in the languages specified by <c>cultures</c>, or a null reference
        /// if the specified sport does not exist, or it(or one of it's children) is not available in one of the requested languages
        /// </summary>
        /// <remarks>
        /// The returned <see cref="SportData"/> represents a sport with flattened hierarchy information - only one category and one tournament are found in the returned instance.
        /// </remarks>
        /// <param name="tournamentId">A <see cref="Urn"/> specifying the tournament whose parent sport to get</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultrureInfo}"/> specifying the languages to which the sport must be translated</param>
        /// <param name="fetchTournamentIfMissing">Indicates if the tournament should be fetched if not obtained via all tournaments request</param>
        /// <returns>A <see cref="SportData"/> representing the requested sport translated into requested languages</returns>
        [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = "<Pending>")]
        private SportData GetSportForTournamentFromCache(Urn tournamentId, IReadOnlyCollection<CultureInfo> cultures, bool fetchTournamentIfMissing)
        {
            TournamentInfoCacheItem cachedTournament = null;

            if (!_sportEventCache.CacheHasItem(tournamentId, CacheItemType.Tournament))
            {
                if (fetchTournamentIfMissing)
                {
                    try
                    {
                        cachedTournament = (TournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(tournamentId);
                        var unused = cachedTournament.GetCompetitorsIdsAsync(cultures).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        ExecutionLog.LogWarning(e, "Error obtaining data for newly created tournament {SportEventId}", tournamentId);
                        return null;
                    }
                }
            }
            else
            {
                cachedTournament = (TournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(tournamentId);
                if (fetchTournamentIfMissing)
                {
                    try
                    {
                        var unused = cachedTournament.GetCompetitorsIdsAsync(cultures).GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        ExecutionLog.LogWarning(e, "Error obtaining data for newly created tournament {SportEventId}", tournamentId);
                    }
                }
            }

            if (!(cachedTournament != null && cachedTournament.HasTranslationsFor(cultures)))
            {
                return null;
            }

            if (!(Categories.TryGetValue(cachedTournament.GetCategoryIdAsync().GetAwaiter().GetResult(), out var cachedCategory) && cachedCategory.HasTranslationsFor(cultures)))
            {
                return null;
            }

            if (!(Sports.TryGetValue(cachedCategory.SportId, out var cachedSport) && cachedSport.HasTranslationsFor(cultures)))
            {
                return null;
            }

            var category = new CategoryData(
                                cachedCategory.Id,
                                cachedCategory.Name.Where(k => cultures.Contains(k.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                cachedCategory.CountryCode,
                                new[] { cachedTournament.Id });
            return new SportData(
                                cachedSport.Id,
                                cachedSport.Name.Where(kvp => cultures.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                new[] { category });
        }

        /// <summary>
        /// Gets a <see cref="SportData"/> representing the sport specified by <c>id</c> in the languages specified by <c>cultures</c>, or a null reference
        /// if the specified sport does not exist, or it(or one of it's children) is not available in one of the requested languages
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of the sport to get</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultrureInfo}"/> specifying the languages to which the sport must be translated</param>
        /// <returns>A <see cref="SportData"/> representing the requested sport translated into requested languages. </returns>
        private async Task<SportData> GetSportFromCacheAsync(Urn id, IReadOnlyCollection<CultureInfo> cultures)
        {
            await FetchSportCategoriesIfNeededAsync(id, cultures).ConfigureAwait(false);

            List<CategoryData> categories = null;

            lock (_mergeLock)
            {
                if (!(Sports.TryGetValue(id, out var cachedSport) && cachedSport.HasTranslationsFor(cultures)))
                {
                    return null;
                }

                try
                {
                    if (cachedSport.CategoryIds != null)
                    {
                        categories = new List<CategoryData>();
                        foreach (var categoryId in cachedSport.CategoryIds)
                        {
                            if (!(Categories.TryGetValue(categoryId, out var cachedCategory) && cachedCategory.HasTranslationsFor(cultures)))
                            {
                                ExecutionLog.LogWarning("An error occurred while retrieving sport from cache. For sportId = {SportId} and lang =[{Langs}] we are missing category {CategoryId}", id, string.Join(",", cultures), categoryId);
                                continue;
                            }

                            categories.Add(new CategoryData(
                                cachedCategory.Id,
                                cachedCategory.Name.Where(t => cultures.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                cachedCategory.CountryCode,
                                cachedCategory.TournamentIds ?? new List<Urn>()));
                        }
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogWarning(e, "An error occurred while retrieving sport from cache. id={SportId} and lang=[{Langs}]", id, string.Join(",", cultures));
                }

                return new SportData(
                    cachedSport.Id,
                    cachedSport.Name.Where(t => cultures.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    categories);
            }
        }

        /// <summary>
        /// Fetch sport categories for sport specified by <c>id</c> in the languages specified by <c>cultures</c> if needed
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of the sport</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultrureInfo}"/> specifying the languages to which the categories must be translated</param>
        private async Task FetchSportCategoriesIfNeededAsync(Urn id, IReadOnlyCollection<CultureInfo> cultures)
        {
            if (!Sports.TryGetValue(id, out var cachedSport))
            {
                return;
            }

            await cachedSport.LoadCategoriesAsync(cultures).ConfigureAwait(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    _semaphore.Dispose();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IReadOnlyCollection{SportData}"/> representing sport hierarchies for all sports supported by the feed.
        /// </summary>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<SportData>> GetSportsAsync(IReadOnlyCollection<CultureInfo> cultures)
        {
            var missingCultures = cultures.Where(c => !FetchedCultures.Contains(c)).ToList();

            try
            {
                if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
                {
                    return null;
                }

                // we have all available data - return the requested info
                if (!missingCultures.Any())
                {
                    var sports = Sports.Keys.Select(sportId =>
                                                    {
                                                        var sportFromCacheAsync = GetSportFromCacheAsync(sportId, cultures);
                                                        sportFromCacheAsync.ConfigureAwait(false);
                                                        return sportFromCacheAsync;
                                                    }).ToList();
                    return await Task.WhenAll(sports).ConfigureAwait(false);
                }

                await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                return await Task.WhenAll(Sports.Keys.Select(sportId =>
                                                             {
                                                                 var sportFromCacheAsync = GetSportFromCacheAsync(sportId, cultures);
                                                                 sportFromCacheAsync.ConfigureAwait(false);
                                                                 return sportFromCacheAsync;
                                                             }).ToList());
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning(ex, "An exception occurred while attempting to fetch sports data for: {Langs}", string.Join(",", missingCultures));
                throw;
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="SportData"/> instance representing the sport hierarchy for the specified sport
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of the sport</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        public async Task<SportData> GetSportAsync(Urn id, IReadOnlyCollection<CultureInfo> cultures)
        {
            var sport = await GetSportFromCacheAsync(id, cultures).ConfigureAwait(false);
            if (sport != null)
            {
                return sport;
            }

            try
            {
                if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
                {
                    return null;
                }

                var missingCultures = cultures.Where(c => !FetchedCultures.Contains(c)).ToList();
                if (missingCultures.Any())
                {
                    await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                }
                return await GetSportFromCacheAsync(id, cultures).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning(ex, "An exception occurred while attempting to fetch sport data for: id={SportId}, cultures={Langs}", id, string.Join(",", cultures));
                throw;
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="CategoryData"/> instance
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of the category</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        public async Task<CategoryData> GetCategoryAsync(Urn id, IReadOnlyCollection<CultureInfo> cultures)
        {
            var missingCultures = cultures;
            CategoryCacheItem categoryCacheItem;
            try
            {
                if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
                {
                    return null;
                }

                if (Categories.TryGetValue(id, out categoryCacheItem))
                {
                    if (categoryCacheItem.HasTranslationsFor(cultures))
                    {
                        return new CategoryData(id, categoryCacheItem.Name.Where(t => cultures.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), categoryCacheItem.CountryCode, categoryCacheItem.TournamentIds);
                    }
                    missingCultures = cultures.Where(c => !FetchedCultures.Contains(c)).ToList();
                }
                if (missingCultures.Any())
                {
                    await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning(ex, "An exception occurred while attempting to fetch category data for: id={CategoryId}, cultures={Langs}", id, string.Join(",", cultures));
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }

            return Categories.TryGetValue(id, out categoryCacheItem)
                ? new CategoryData(id, categoryCacheItem.Name.Where(t => cultures.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), categoryCacheItem.CountryCode, categoryCacheItem.TournamentIds)
                : null;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="SportData"/> representing sport associated with the tournament specified by it's id. Note that the hierarchy will only contain the
        /// specified tournament and it's parent category not all categories / tournaments in the hierarchy
        /// </summary>
        /// <param name="tournamentId">A <see cref="Urn"/> specifying the id of the tournament whose parent sport should be retrieved</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        public async Task<SportData> GetSportForTournamentAsync(Urn tournamentId, IReadOnlyCollection<CultureInfo> cultures)
        {
            var missingCultures = cultures.Where(c => !FetchedCultures.Contains(c)).ToList();

            try
            {
                if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
                {
                    return null;
                }

                if (missingCultures.Any())
                {
                    await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning(ex, "An exception occurred while attempting to fetch sport data for tournament: id={TournamentId}, cultures={Langs}", tournamentId, string.Join(",", cultures));
                return null;
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }

            var sport = GetSportForTournamentFromCache(tournamentId, cultures, true);
            return sport;
        }

        /// <summary>
        /// Loads all tournaments for all sports asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        public Task LoadAllTournamentsForAllSportsAsync()
        {
            OnTimerElapsed(null, null);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        protected override Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            if (_isDisposed)
            {
                return Task.FromResult(false);
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.Category:
                    if (item is CategoryDto category)
                    {
                        AddCategory(id, category, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CategoryDto), item.GetType());
                    }
                    break;
                case DtoType.Competitor:
                    break;
                case DtoType.CompetitorProfile:
                    break;
                case DtoType.SimpleTeamProfile:
                    break;
                case DtoType.Fixture:
                    var fixture = item as FixtureDto;
                    if (fixture?.Tournament != null)
                    {
                        AddSport(fixture.SportId, fixture.Tournament.Sport, fixture.Tournament.Category.Id, culture);
                        AddCategory(fixture.Tournament.Category.Id, fixture.Tournament.Category, fixture.SportId, new List<Urn> { fixture.Tournament.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(FixtureDto), item.GetType());
                    }
                    break;
                case DtoType.MarketDescription:
                    break;
                case DtoType.MatchSummary:
                    var match = item as MatchDto;
                    if (match?.Tournament != null)
                    {
                        AddSport(match.SportId, match.Tournament.Sport, match.Tournament.Category.Id, culture);
                        AddCategory(match.Tournament.Category.Id, match.Tournament.Category, match.SportId, new List<Urn> { match.Tournament.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDto), item.GetType());
                    }
                    break;
                case DtoType.MatchTimeline:
                    var timeline = item as MatchTimelineDto;
                    if (timeline?.SportEvent != null)
                    {
                        AddDataFromSportEventSummary(timeline.SportEvent, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchTimelineDto), item.GetType());
                    }
                    break;
                case DtoType.PlayerProfile:
                    break;
                case DtoType.RaceSummary:
                    if (item is StageDto stageDto)
                    {
                        if (stageDto.SportEventStatus != null)
                        {
                            AddDataFromSportEventSummary(stageDto, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDto), item.GetType());
                    }
                    break;
                case DtoType.Sport:
                    if (item is SportDto sport)
                    {
                        AddSport(id, sport, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDto), item.GetType());
                    }
                    break;
                case DtoType.SportList:
                    if (item is EntityList<SportDto> sportList)
                    {
                        foreach (var s in sportList.Items)
                        {
                            AddSport(s.Id, s, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportDto>), item.GetType());
                    }
                    break;
                case DtoType.SportEventStatus:
                    break;
                case DtoType.SportEventSummary:
                    if (item is SportEventSummaryDto summary)
                    {
                        AddDataFromSportEventSummary(summary, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDto), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummaryList:
                    if (item is EntityList<SportEventSummaryDto> summaryList)
                    {
                        foreach (var s in summaryList.Items)
                        {
                            AddDataFromSportEventSummary(s, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportEventSummaryDto>), item.GetType());
                    }
                    break;
                case DtoType.Tournament:
                    if (item is TournamentDto tour)
                    {
                        AddSport(tour.Sport.Id, tour.Sport, tour.Category.Id, culture);
                        AddCategory(tour.Category.Id, tour.Category, tour.Sport.Id, new List<Urn> { tour.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDto), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfo:
                    if (item is TournamentInfoDto tourInfo)
                    {
                        AddSport(tourInfo.SportId, tourInfo.Sport, tourInfo.Category.Id, culture);
                        AddCategory(tourInfo.Category.Id, tourInfo.Category, tourInfo.Sport.Id, new List<Urn> { tourInfo.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDto), item.GetType());
                    }
                    break;
                case DtoType.TournamentSeasons:
                    var tourSeasons = item as TournamentSeasonsDto;
                    if (tourSeasons?.Tournament != null)
                    {
                        var tourSeasonsTournament = tourSeasons.Tournament;
                        AddSport(tourSeasonsTournament.Category.Id, tourSeasonsTournament.Sport, tourSeasonsTournament.Category.Id, culture);
                        AddCategory(tourSeasonsTournament.Category.Id, tourSeasonsTournament.Category, tourSeasonsTournament.Sport.Id, new List<Urn> { tourSeasonsTournament.Id }, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentInfoDto), item.GetType());
                    }
                    break;
                case DtoType.MarketDescriptionList:
                    break;
                case DtoType.VariantDescription:
                    break;
                case DtoType.VariantDescriptionList:
                    break;
                case DtoType.Lottery:
                    if (item is LotteryDto lottery)
                    {
                        AddDataFromSportEventSummary(lottery, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(LotteryDto), item.GetType());
                    }
                    break;
                case DtoType.LotteryDraw:
                    if (item is DrawDto lotteryDraw)
                    {
                        AddDataFromSportEventSummary(lotteryDraw, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(DrawDto), item.GetType());
                    }
                    break;
                case DtoType.LotteryList:
                    if (item is EntityList<LotteryDto> lotteryList)
                    {
                        foreach (var s in lotteryList.Items)
                        {
                            AddDataFromSportEventSummary(s, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<LotteryDto>), item.GetType());
                    }
                    break;
                case DtoType.SportCategories:
                    if (item is SportCategoriesDto sportCategories)
                    {
                        AddSport(sportCategories.Sport.Id, sportCategories, culture);
                        AddCategories(sportCategories, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportCategoriesDto), item.GetType());
                    }
                    break;
                case DtoType.BookingStatus:
                    break;
                case DtoType.AvailableSelections:
                    break;
                case DtoType.TournamentInfoList:
                    break;
                case DtoType.PeriodSummary:
                    break;
                case DtoType.Calculation:
                    break;
                default:
                    ExecutionLog.LogWarning("Trying to add unchecked dto type: {DtoType} for id: {Id}", dtoType, id);
                    break;
            }

            return Task.FromResult(saved);
        }

        private void AddDataFromSportEventSummary(SportEventSummaryDto dto, CultureInfo culture)
        {
            var match = dto as MatchDto;
            if (match?.Tournament != null)
            {
                AddSport(match.SportId, match.Tournament.Sport, match.Tournament.Category.Id, culture);
                AddCategory(match.Tournament.Category.Id, match.Tournament.Category, match.SportId, new List<Urn> { match.Tournament.Id }, culture);
                return;
            }

            if (dto is TournamentInfoDto tour)
            {
                AddSport(tour.SportId, tour.Sport, tour.Category.Id, culture);
                AddCategory(tour.Category.Id, tour.Category, tour.SportId, new List<Urn> { tour.Id }, culture);

                if (tour.TournamentInfo != null)
                {
                    AddDataFromSportEventSummary(tour.TournamentInfo, culture);
                }
                return;
            }

            var stage = dto as StageDto;
            if (stage?.Tournament != null)
            {
                if (stage.Tournament?.Sport != null)
                {
                    AddSport(stage.Tournament.Sport.Id, stage.Tournament.Sport, stage.Tournament.Id, culture);
                }

                if (stage.Tournament?.Category != null)
                {
                    AddCategory(stage.Tournament.Category.Id, stage.Tournament.Category, stage.SportId, new List<Urn> { stage.Id }, culture);
                }
                return;
            }

            if (dto is DrawDto draw)
            {
                if (draw.Lottery != null)
                {
                    AddSport(draw.Lottery.Sport.Id, draw.Lottery.Sport, culture);
                    AddCategory(draw.Lottery.Category.Id, draw.Lottery.Category, draw.SportId, null, culture);
                }
                return;
            }

            if (dto is LotteryDto lottery)
            {
                AddSport(lottery.Sport.Id, lottery.Sport, culture);
                AddCategory(lottery.Category.Id, lottery.Category, lottery.SportId ?? lottery.Sport?.Id, null, culture);
            }
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.Category,
                                     DtoType.Fixture,
                                     DtoType.MatchSummary,
                                     DtoType.MatchTimeline,
                                     DtoType.RaceSummary,
                                     DtoType.Sport,
                                     DtoType.SportCategories,
                                     DtoType.SportList,
                                     DtoType.SportEventSummary,
                                     DtoType.SportEventSummaryList,
                                     DtoType.Tournament,
                                     DtoType.TournamentInfo,
                                     DtoType.TournamentSeasons,
                                     DtoType.Lottery,
                                     DtoType.LotteryDraw,
                                     DtoType.LotteryList
                                 };
        }

        /// <summary>
        /// Purges item from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item in the cache to be purged</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.Sport)
            {
                lock (_mergeLock)
                {
                    Sports.Remove(id);
                }
            }
            else if (cacheItemType == CacheItemType.Category)
            {
                lock (_mergeLock)
                {
                    Categories.Remove(id);
                }
            }
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Easy to read")]
        public override bool CacheHasItem(Urn id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.Sport || cacheItemType == CacheItemType.All)
            {
                return Sports.ContainsKey(id) || Categories.ContainsKey(id);
            }

            return cacheItemType == CacheItemType.Category && Categories.ContainsKey(id);
        }

        private void AddSport(Urn id, SportDto item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Sports.ContainsKey(id) && Sports.TryGetValue(id, out var ci))
                    {
                        ci.Merge(new SportCacheItem(item, _dataRouterManager, culture), culture);

                    }
                    else
                    {
                        Sports.Add(id, new SportCacheItem(item, _dataRouterManager, culture));
                    }

                    if (item.Categories != null)
                    {
                        foreach (var categoryData in item.Categories)
                        {
                            AddCategory(categoryData.Id, categoryData, culture);
                        }
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError(e, "Error saving SportDto for {SportId} and lang={Lang}", id, culture.TwoLetterISOLanguageName);
                }
            }
        }

        private void AddSport(Urn id, SportEntityDto sport, Urn sportCategoryId, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Sports.ContainsKey(id) && Sports.TryGetValue(id, out var ci))
                    {
                        ci.Merge(new SportCacheItem(new SportDto(sport.Id.ToString(), sport.Name, (IEnumerable<tournamentExtended>)null), _dataRouterManager, culture), culture);
                        if (ci.CategoryIds.IsNullOrEmpty() || !ci.CategoryIds.Contains(sportCategoryId))
                        {
                            ci.CategoryIds.Add(sportCategoryId);
                        }
                    }
                    else
                    {
                        var sportCi = new SportCacheItem(new SportDto(sport.Id.ToString(), sport.Name, (IEnumerable<tournamentExtended>)null), _dataRouterManager, culture);
                        sportCi.CategoryIds.Add(sportCategoryId);
                        Sports.Add(id, sportCi);
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError(e, "Error saving SportEntityDto for {SportId} and lang={Lang}", id, culture.TwoLetterISOLanguageName);
                }
            }
        }

        private void AddSport(Urn id, SportCategoriesDto item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Sports.ContainsKey(id))
                    {
                        Sports.TryGetValue(id, out var ci);
                        ci?.Merge(new SportCacheItem(new SportDto(item.Sport.Id.ToString(), item.Sport.Name, item.Categories), _dataRouterManager, culture), culture);
                    }
                    else
                    {
                        Sports.Add(id, new SportCacheItem(new SportDto(item.Sport.Id.ToString(), item.Sport.Name, item.Categories), _dataRouterManager, culture));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError(e, "Error saving SportEntityDto for {SportId} and lang={Lang}", id, culture.TwoLetterISOLanguageName);
                }
            }
        }

        private void AddSport(ExportableSport item)
        {
            lock (_mergeLock)
            {
                try
                {
                    var id = Urn.Parse(item.Id);
                    if (Sports.ContainsKey(id))
                    {
                        Sports.TryGetValue(id, out var ci);
                        ci?.Merge(new SportCacheItem(item), item.Names.Keys.First());
                    }
                    else
                    {
                        Sports.Add(id, new SportCacheItem(item));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError(e, "Error importing ExportableSportCacheItem for {SportId}", item.Id);
                }
            }
        }

        /// <summary>
        /// Adds the category
        /// </summary>
        /// <param name="id">The identifier of the CategoryId or SportId !!!</param>
        /// <param name="item">The category dto item</param>
        /// <param name="culture">The culture</param>
        private void AddCategory(Urn id, CategoryDto item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Categories.ContainsKey(item.Id))
                    {
                        Categories.TryGetValue(item.Id, out var ci);
                        ci?.Merge(new CategoryCacheItem(item, culture, item.Tournaments?.FirstOrDefault()?.Sport.Id ?? id), culture);
                    }
                    else
                    {
                        Categories.Add(item.Id, new CategoryCacheItem(item, culture, item.Tournaments?.FirstOrDefault()?.Sport.Id ?? id));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError(e, "Error saving CategoryDto for {CategoryId} and lang={Lang}", id, culture.TwoLetterISOLanguageName);
                }
            }
        }

        private void AddCategory(Urn id, CategorySummaryDto item, Urn sportId, IEnumerable<Urn> tournamentIds, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Categories.ContainsKey(id))
                    {
                        Categories.TryGetValue(id, out var ci);
                        ci?.Merge(new CategoryCacheItem(item, culture, sportId, tournamentIds), culture);
                    }
                    else
                    {
                        Categories.Add(id, new CategoryCacheItem(item, culture, sportId, tournamentIds));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError(e, "Error saving CategorySummaryDto for {CategoryId} and lang={Lang}", id, culture.TwoLetterISOLanguageName);
                }
            }
        }

        private void AddCategory(ExportableCategory item)
        {
            lock (_mergeLock)
            {
                try
                {
                    var id = Urn.Parse(item.Id);
                    if (Categories.ContainsKey(id))
                    {
                        Categories.TryGetValue(id, out var ci);
                        ci?.Merge(new CategoryCacheItem(item), item.Names.Keys.First());
                    }
                    else
                    {
                        Categories.Add(id, new CategoryCacheItem(item));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError(e, "Error importing ExportableCategory for {CategoryId}", item.Id);
                }
            }
        }

        private void AddCategories(SportCategoriesDto item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                foreach (var category in item.Categories)
                {
                    try
                    {
                        if (Categories.ContainsKey(category.Id))
                        {
                            Categories.TryGetValue(category.Id, out var ci);
                            ci?.Merge(new CategoryCacheItem(category, culture, item.Sport.Id), culture);
                        }
                        else
                        {
                            Categories.Add(category.Id, new CategoryCacheItem(category, culture, item.Sport.Id));
                        }
                    }
                    catch (Exception e)
                    {
                        ExecutionLog.LogError(e, "Error saving CategoryDto for {CategoryId} and lang={Lang}", category.Id, culture.TwoLetterISOLanguageName);
                    }
                }
            }
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableBase"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableBase>> ExportAsync()
        {
            List<IExportableBase> exportables;
            lock (_mergeLock)
            {
                exportables = Sports.Values.Cast<IExportableBase>().Concat(Categories.Values).ToList();
            }

            var tasks = exportables.Select(e => e.ExportAsync());

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Imports provided items into the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableBase"/> to be inserted into the cache</param>
        public Task ImportAsync(IEnumerable<ExportableBase> items)
        {
            foreach (var exportable in items)
            {
                if (exportable is ExportableSport exportableSport)
                {
                    AddSport(exportableSport);
                    continue;
                }

                if (exportable is ExportableCategory exportableCategory)
                {
                    AddCategory(exportableCategory);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing all cache item types in the cache and their counts</returns>
        public IReadOnlyDictionary<string, int> CacheStatus()
        {
            lock (_mergeLock)
            {
                return new Dictionary<string, int>
                {
                    {typeof(SportCacheItem).Name, Sports.Count},
                    {typeof(CategoryCacheItem).Name, Categories.Count}
                };
            }
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(HealthCheckResult.Healthy($"Sports has {Sports.Count.ToString()} items, Categories has {Categories.Count.ToString()} items"));
        }
    }
}
