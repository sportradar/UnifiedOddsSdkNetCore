/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Health;
using App.Metrics.Timer;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using ITimer = Sportradar.OddsFeed.SDK.Common.Internal.ITimer;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
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
        /// A <see cref="IEnumerable{CultureInfo}"/> specifying cultures in which the sport data is periodically fetched
        /// </summary>
        private readonly IEnumerable<CultureInfo> _requiredCultures;

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
        /// A <see cref="IDictionary{URN, SportCI}"/> used to cache fetched sports
        /// </summary>
        internal readonly IDictionary<URN, SportCI> Sports;

        /// <summary>
        /// A <see cref="IDictionary{URN, CategoryCI}"/> used to cache fetched categories
        /// </summary>
        internal readonly IDictionary<URN, CategoryCI> Categories;

        /// <summary>
        /// Value indicating whether the required data was already fetched automatically(by internal timer)
        /// </summary>
        private bool _wasDataAutoFetched;

        /// <summary>
        /// Value specifying whether the current instance is disposed
        /// </summary>
        private bool _isDisposed;

        private readonly ITimer _timer;

        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDataCache"/> class
        /// </summary>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to fetch all required data</param>
        /// <param name="timer">A <see cref="ITimer"/> instance used to periodically fetch sport related data</param>
        /// <param name="cultures">A list of <see cref="CultureInfo"/> of the cached data</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> containing also tournament data</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        public SportDataCache(IDataRouterManager dataRouterManager,
                              ITimer timer,
                              IEnumerable<CultureInfo> cultures,
                              ISportEventCache sportEventCache,
                              ICacheManager cacheManager)
            : base(cacheManager)
        {
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(timer, nameof(timer)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();

            _dataRouterManager = dataRouterManager;
            _requiredCultures = cultures as ReadOnlyCollection<CultureInfo> ?? new ReadOnlyCollection<CultureInfo>(cultures.ToList());

            FetchedCultures = new HashSet<CultureInfo>();
            Sports = new ConcurrentDictionary<URN, SportCI>();
            Categories = new ConcurrentDictionary<URN, CategoryCI>();
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
        /// <param name="sender">A <see cref="ITimer"/> invoking the method</param>
        /// <param name="e">The <see cref="EventArgs"/> providing additional information about the event which invoked the method</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "<Pending>")]
        private async void OnTimerElapsed(object sender, EventArgs e)
        {

            if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
            {
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
                    await FetchAndMergeAll(cultureInfos, _wasDataAutoFetched).ConfigureAwait(false);
                    ExecutionLog.LogInformation($"Sport data for languages [{string.Join(",", cultureInfos)}] successfully fetched and merged.");
                    _wasDataAutoFetched = true;
                }
            }
            catch (FeedSdkException ex)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch sport data for: {string.Join(",", cultureInfos)}. Exception was: {ex}");
            }
            catch (ObjectDisposedException)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch sport data for: {string.Join(",", cultureInfos)}. DataProvider was already disposed.");
            }
            catch (TaskCanceledException)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch sport data for: {string.Join(",", cultureInfos)}. Task canceled. DataProvider was already disposed.");
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch sport data for: {string.Join(",", cultureInfos)}. Exception: {ex}");
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }
        }

        /// <summary>
        /// Fetches the data for languages specified by <code>cultures</code> and merges / adds it to internally used dictionaries.
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages in which to fetch the data</param>
        /// <param name="clearExistingData">Value indicating whether the internally used dictionaries should be cleared before fetched data is added</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        private async Task FetchAndMergeAll(IEnumerable<CultureInfo> cultures, bool clearExistingData)
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            var cultureInfos = cultures as IReadOnlyList<CultureInfo> ?? cultures.ToList();
            var timerOptions = new TimerOptions { Context = "SportDataCache", Name = "GetAll", MeasurementUnit = Unit.Requests };
            using (SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(timerOptions))
            {
                if (clearExistingData)
                {
                    FetchedCultures.Clear();
                    Categories.Clear();
                    Sports.Clear();
                    _sportEventCache.DeleteSportEventsFromCache(DateTime.MaxValue);
                }

                var fetchTasks = cultureInfos.Select(c => _dataRouterManager.GetAllSportsAsync(c)).ToList();
                fetchTasks.AddRange(cultureInfos.Select(c => _dataRouterManager.GetAllTournamentsForAllSportAsync(c)).ToList());
                fetchTasks.AddRange(cultureInfos.Select(c => _dataRouterManager.GetAllLotteriesAsync(c, true)).ToList());

                await Task.WhenAll(fetchTasks).ConfigureAwait(false);

                foreach (var culture in cultureInfos)
                {
                    FetchedCultures.Add(culture);
                }
            }
        }


        /// <summary>
        /// Gets a <see cref="SportData"/> representing the parent sport of the tournament specified by <code>tournamentId</code> in the languages specified by <code>cultures</code>, or a null reference
        /// if the specified sport does not exist, or it(or one of it's children) is not available in one of the requested languages
        /// </summary>
        /// <remarks>
        /// The returned <see cref="SportData"/> represents a sport with flattened hierarchy information - only one category and one tournament are found in the returned instance.
        /// </remarks>
        /// <param name="tournamentId">A <see cref="URN"/> specifying the tournament whose parent sport to get</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultrureInfo}"/> specifying the languages to which the sport must be translated</param>
        /// <param name="fetchTournamentIfMissing">Indicates if the tournament should be fetched if not obtained via all tournaments request</param>
        /// <returns>A <see cref="SportData"/> representing the requested sport translated into requested languages</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = "<Pending>")]
        private SportData GetSportForTournamentFromCache(URN tournamentId, IEnumerable<CultureInfo> cultures, bool fetchTournamentIfMissing)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            TournamentInfoCI cachedTournament = null;

            if (!_sportEventCache.CacheHasItem(tournamentId, CacheItemType.Tournament))
            {
                if (fetchTournamentIfMissing)
                {
                    try
                    {
                        cachedTournament = (TournamentInfoCI) _sportEventCache.GetEventCacheItem(tournamentId);
                        var unused = cachedTournament.GetCompetitorsIdsAsync(cultureList).Result;
                    }
                    catch (Exception e)
                    {
                        ExecutionLog.LogWarning($"Error obtaining data for newly created tournament {tournamentId}.", e);
                        return null;
                    }
                }
            }
            else
            {
                cachedTournament = (TournamentInfoCI) _sportEventCache.GetEventCacheItem(tournamentId);
                if (fetchTournamentIfMissing)
                {
                    try
                    {
                        var unused = cachedTournament.GetCompetitorsIdsAsync(cultureList).Result;
                    }
                    catch (Exception e)
                    {
                        ExecutionLog.LogWarning($"Error obtaining data for newly created tournament {tournamentId}.", e);
                    }
                }
            }

            if (!(cachedTournament != null && cachedTournament.HasTranslationsFor(cultureList)))
            {
                return null;
            }

            CategoryCI cachedCategory;
            if (!(Categories.TryGetValue(cachedTournament.GetCategoryIdAsync().Result, out cachedCategory) && cachedCategory.HasTranslationsFor(cultureList)))
            {
                return null;
            }

            SportCI cachedSport;
            if (!(Sports.TryGetValue(cachedCategory.SportId, out cachedSport) && cachedSport.HasTranslationsFor(cultureList)))
            {
                return null;
            }

            var category = new CategoryData(
                                cachedCategory.Id,
                                cachedCategory.Name.Where(k => cultureList.Contains(k.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                cachedCategory.CountryCode,
                                new[] { cachedTournament.Id });
            return new SportData(
                                cachedSport.Id,
                                cachedSport.Name.Where(kvp => cultureList.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                new[] { category });
        }

        /// <summary>
        /// Gets a <see cref="SportData"/> representing the sport specified by <code>id</code> in the languages specified by <code>cultures</code>, or a null reference
        /// if the specified sport does not exist, or it(or one of it's children) is not available in one of the requested languages
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of the sport to get</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultrureInfo}"/> specifying the languages to which the sport must be translated</param>
        /// <returns>A <see cref="SportData"/> representing the requested sport translated into requested languages. </returns>
        private async Task<SportData> GetSportFromCacheAsync(URN id, IEnumerable<CultureInfo> cultures)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            await FetchSportCategoriesIfNeededAsync(id, cultureList).ConfigureAwait(false);

            List<CategoryData> categories = null;

            lock (_mergeLock)
            {
                SportCI cachedSport;
                if (!(Sports.TryGetValue(id, out cachedSport) && cachedSport.HasTranslationsFor(cultureList)))
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
                            if (!(Categories.TryGetValue(categoryId, out var cachedCategory) && cachedCategory.HasTranslationsFor(cultureList)))
                            {
                                ExecutionLog.LogWarning($"An error occurred while retrieving sport from cache.For sportId = {id} and lang =[{string.Join(",", cultureList)}] we are missing category {categoryId}.");
                                continue;
                            }

                            categories.Add(new CategoryData(
                                cachedCategory.Id,
                                cachedCategory.Name.Where(t => cultureList.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                cachedCategory.CountryCode,
                                cachedCategory.TournamentIds ?? new List<URN>()));
                        }
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogWarning($"An error occurred while retrieving sport from cache. id={id} and lang=[{string.Join(",", cultureList)}].", e);
                }

                return new SportData(
                    cachedSport.Id,
                    cachedSport.Name.Where(t => cultureList.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    categories);
            }
        }

        /// <summary>
        /// Fetch sport categories for sport specified by <code>id</code> in the languages specified by <code>cultures</code> if needed
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of the sport</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultrureInfo}"/> specifying the languages to which the categories must be translated</param>
        private async Task FetchSportCategoriesIfNeededAsync(URN id, IList<CultureInfo> cultures)
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
        /// Asynchronously gets a <see cref="IEnumerable{SportData}"/> representing sport hierarchies for all sports supported by the feed.
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<SportData>> GetSportsAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            //Just lock - don't even check if all the required data is available
            if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
            {
                return null;
            }
            var missingCultures = cultureList.Where(c => !FetchedCultures.Contains(c)).ToList();

            try
            {
                // we have all available data - return the requested info
                if (!missingCultures.Any())
                {
                    var sports = Sports.Keys.Select(sportId =>
                                                    {
                                                        var sportFromCacheAsync = GetSportFromCacheAsync(sportId, cultureList);
                                                        sportFromCacheAsync.ConfigureAwait(false);
                                                        return sportFromCacheAsync;
                                                    }).ToList();
                    return await Task.WhenAll(sports).ConfigureAwait(false);
                }

                await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                return await Task.WhenAll(Sports.Keys.Select(sportId =>
                                                             {
                                                                 var sportFromCacheAsync = GetSportFromCacheAsync(sportId, cultureList);
                                                                 sportFromCacheAsync.ConfigureAwait(false);
                                                                 return sportFromCacheAsync;
                                                             }).ToList());
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch sports data for: {string.Join(",", missingCultures)}. Exception: {ex}");
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
        /// <param name="id">A <see cref="URN"/> specifying the id of the sport</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        public async Task<SportData> GetSportAsync(URN id, IEnumerable<CultureInfo> cultures)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();
            var sport = await GetSportFromCacheAsync(id, cultureList).ConfigureAwait(false);
            if (sport != null)
            {
                return sport;
            }

            if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
            {
                return null;
            }

            try
            {
                var missingCultures = cultureList.Where(c => !FetchedCultures.Contains(c)).ToList();
                if (missingCultures.Any())
                {
                    await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                }
                return await GetSportFromCacheAsync(id, cultureList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch sport data for: id={id}, cultures={string.Join(",", cultureList)}. Exception: {ex}");
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
        /// <param name="id">A <see cref="URN"/> specifying the id of the category</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        public async Task<CategoryData> GetCategoryAsync(URN id, IEnumerable<CultureInfo> cultures)
        {
            if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
            {
                return null;
            }

            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();
            var missingCultures = cultureList;
            CategoryCI categoryCI;
            try
            {
                if (Categories.TryGetValue(id, out categoryCI))
                {
                    if (categoryCI.HasTranslationsFor(cultureList))
                    {
                        return new CategoryData(id, categoryCI.Name.Where(t => cultureList.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), categoryCI.CountryCode, categoryCI.TournamentIds);
                    }
                    missingCultures = cultureList.Where(c => !FetchedCultures.Contains(c)).ToList();
                }
                if (missingCultures.Any())
                {
                    await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch category data for: id={id}, cultures={string.Join(",", cultureList)}. Exception: {ex}");
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }

            return Categories.TryGetValue(id, out categoryCI)
                ? new CategoryData(id, categoryCI.Name.Where(t => cultureList.Contains(t.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value), categoryCI.CountryCode, categoryCI.TournamentIds)
                : null;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="SportData"/> representing sport associated with the tournament specified by it's id. Note that the hierarchy will only contain the
        /// specified tournament and it's parent category not all categories / tournaments in the hierarchy
        /// </summary>
        /// <param name="tournamentId">A <see cref="URN"/> specifying the id of the tournament whose parent sport should be retrieved</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages in which the data is returned</param>
        /// <returns>A <see cref="Task{SportData}"/> representing the asynchronous operation</returns>
        public async Task<SportData> GetSportForTournamentAsync(URN tournamentId, IEnumerable<CultureInfo> cultures)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            if (!await _semaphore.WaitAsyncSafe().ConfigureAwait(false))
            {
                return null;
            }

            var missingCultures = cultureList.Where(c => !FetchedCultures.Contains(c)).ToList();
            try
            {
                if (missingCultures.Any())
                {
                    await FetchAndMergeAll(missingCultures, false).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.LogWarning($"An exception occurred while attempting to fetch sport data for tournament: id={tournamentId}, cultures={string.Join(",", cultureList)}. Exception: {ex}");
                return null;
            }
            finally
            {
                _semaphore.ReleaseSafe();
            }

            var sport = GetSportForTournamentFromCache(tournamentId, cultureList, true);
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

        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            // later 
        }

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult"/>
        /// </summary>
        public HealthCheckResult StartHealthCheck()
        {
            return Sports.Any()
                ? HealthCheckResult.Healthy($"Sports has {Sports.Count} items, Categories has {Categories.Count} items.")
                : HealthCheckResult.Unhealthy("Dictionary is empty.");
        }

        /// <summary>
        /// Adds the item to the cache
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be added</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        protected override bool CacheAddDtoItem(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.Category:
                    var category = item as CategoryDTO;
                    if (category != null)
                    {
                        AddCategory(id, category, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(CategoryDTO), item.GetType());
                    }
                    break;
                case DtoType.Competitor:
                    break;
                case DtoType.CompetitorProfile:
                    break;
                case DtoType.SimpleTeamProfile:
                    break;
                case DtoType.Fixture:
                    var fixture = item as FixtureDTO;
                    if (fixture?.Tournament != null)
                    {
                        AddSport(fixture.SportId, fixture.Tournament.Sport, culture);
                        AddCategory(fixture.Tournament.Category.Id, fixture.Tournament.Category, fixture.SportId, new List<URN> { fixture.Tournament.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(FixtureDTO), item.GetType());
                    }
                    break;
                case DtoType.MarketDescription:
                    break;
                case DtoType.MatchSummary:
                    var match = item as MatchDTO;
                    if (match?.Tournament != null)
                    {
                        AddSport(match.SportId, match.Tournament.Sport, culture);
                        AddCategory(match.Tournament.Category.Id, match.Tournament.Category, match.SportId, new List<URN> { match.Tournament.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDTO), item.GetType());
                    }
                    break;
                case DtoType.MatchTimeline:
                    var timeline = item as MatchTimelineDTO;
                    if (timeline?.SportEvent != null)
                    {
                        AddDataFromSportEventSummary(timeline.SportEvent, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchTimelineDTO), item.GetType());
                    }
                    break;
                case DtoType.PlayerProfile:
                    break;
                case DtoType.RaceSummary:
                    var stageDTO = item as StageDTO;
                    if (stageDTO != null)
                    {
                        if (stageDTO.SportEventStatus != null)
                        {
                            AddDataFromSportEventSummary(stageDTO, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDTO), item.GetType());
                    }
                    break;
                case DtoType.Sport:
                    var sport = item as SportDTO;
                    if (sport != null)
                    {
                        AddSport(id, sport, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDTO), item.GetType());
                    }
                    break;
                case DtoType.SportList:
                    var sportList = item as EntityList<SportDTO>;
                    if (sportList != null)
                    {
                        foreach (var s in sportList.Items)
                        {
                            AddSport(s.Id, s, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportDTO>), item.GetType());
                    }
                    break;
                case DtoType.SportEventStatus:
                    break;
                case DtoType.SportEventSummary:
                    var summary = item as SportEventSummaryDTO;
                    if (summary != null)
                    {
                        AddDataFromSportEventSummary(summary, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDTO), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummaryList:
                    var summaryList = item as EntityList<SportEventSummaryDTO>;
                    if (summaryList != null)
                    {
                        foreach (var s in summaryList.Items)
                        {
                            AddDataFromSportEventSummary(s, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportEventSummaryDTO>), item.GetType());
                    }
                    break;
                case DtoType.Tournament:
                    var tour = item as TournamentDTO;
                    if (tour != null)
                    {
                        AddSport(tour.Sport.Id, tour.Sport, culture);
                        AddCategory(tour.Category.Id, tour.Category, tour.Sport.Id, new List<URN> { tour.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfo:
                    var tourInfo = item as TournamentInfoDTO;
                    if (tourInfo != null)
                    {
                        AddSport(tourInfo.SportId, tourInfo.Sport, culture);
                        AddCategory(tourInfo.Category.Id, tourInfo.Category, tourInfo.Sport.Id, new List<URN> { tourInfo.Id }, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentSeasons:
                    var tourSeasons = item as TournamentSeasonsDTO;
                    if (tourSeasons?.Tournament != null)
                    {
                        var tourSeasonsTournament = tourSeasons.Tournament;
                        AddSport(tourSeasonsTournament.SportId, tourSeasonsTournament.Sport, culture);
                        AddCategory(tourSeasonsTournament.Category.Id, tourSeasonsTournament.Category, tourSeasonsTournament.Sport.Id, new List<URN> { tourSeasonsTournament.Id }, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentInfoDTO), item.GetType());
                    }
                    break;
                case DtoType.MarketDescriptionList:
                    break;
                case DtoType.VariantDescription:
                    break;
                case DtoType.VariantDescriptionList:
                    break;
                case DtoType.Lottery:
                    var lottery = item as LotteryDTO;
                    if (lottery != null)
                    {
                        AddDataFromSportEventSummary(lottery, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(LotteryDTO), item.GetType());
                    }
                    break;
                case DtoType.LotteryDraw:
                    var lotteryDraw = item as DrawDTO;
                    if (lotteryDraw != null)
                    {
                        AddDataFromSportEventSummary(lotteryDraw, culture);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(DrawDTO), item.GetType());
                    }
                    break;
                case DtoType.LotteryList:
                    var lotteryList = item as EntityList<LotteryDTO>;
                    if (lotteryList != null)
                    {
                        foreach (var s in lotteryList.Items)
                        {
                            AddDataFromSportEventSummary(s, culture);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<LotteryDTO>), item.GetType());
                    }
                    break;
                case DtoType.SportCategories:
                    var sportCategories = item as SportCategoriesDTO;
                    if (sportCategories != null)
                    {
                        AddSport(sportCategories.Sport.Id, sportCategories, culture);
                        AddCategories(sportCategories, culture);
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportCategoriesDTO), item.GetType());
                    }
                    break;
                case DtoType.BookingStatus:
                    break;
                case DtoType.AvailableSelections:
                    break;
                case DtoType.TournamentInfoList:
                    break;
                default:
                    ExecutionLog.LogWarning($"Trying to add unchecked dto type: {dtoType} for id: {id}.");
                    break;
            }

            return saved;
        }

        private void AddDataFromSportEventSummary(SportEventSummaryDTO dto, CultureInfo culture)
        {
            var match = dto as MatchDTO;
            if (match?.Tournament != null)
            {
                AddSport(match.SportId, match.Tournament.Sport, culture);
                AddCategory(match.Tournament.Category.Id, match.Tournament.Category, match.SportId, new List<URN> { match.Tournament.Id }, culture);
                return;
            }

            if (dto is TournamentInfoDTO tour)
            {
                AddSport(tour.SportId, tour.Sport, culture);
                AddCategory(tour.Category.Id, tour.Category, tour.SportId, new List<URN> { tour.Id }, culture);

                if (tour.TournamentInfo != null)
                {
                    AddDataFromSportEventSummary(tour.TournamentInfo, culture);
                }
                return;
            }

            var stage = dto as StageDTO;
            if (stage?.Tournament != null)
            {
                if (stage.Tournament?.Sport != null)
                {
                    AddSport(stage.Tournament.Sport.Id, stage.Tournament.Sport, culture);
                }

                if (stage.Tournament.Category != null)
                {
                    AddCategory(stage.Tournament.Category.Id, stage.Tournament.Category, stage.SportId, new List<URN> {stage.Id}, culture);
                }
                return;
            }

            if (dto is DrawDTO draw)
            {
                if (draw.Lottery != null)
                {
                    AddSport(draw.Lottery.Sport.Id, draw.Lottery.Sport, culture);
                    AddCategory(draw.Lottery.Category.Id, draw.Lottery.Category, draw.SportId, null, culture);
                }
                return;
            }

            if (dto is LotteryDTO lottery)
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
        /// <param name="id">A <see cref="URN" /> representing the id of the item in the cache to be purged</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(URN id, CacheItemType cacheItemType)
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
        /// <param name="id">A <see cref="URN" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(URN id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.Sport || cacheItemType == CacheItemType.All)
            {
                return Sports.ContainsKey(id) || Categories.ContainsKey(id);
            }

            if (cacheItemType == CacheItemType.Category)
            {
                return Categories.ContainsKey(id);
            }
            return false;
        }

        private void AddSport(URN id, SportDTO item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Sports.ContainsKey(id))
                    {
                        Sports.TryGetValue(id, out var ci);
                        ci?.Merge(new SportCI(item, _dataRouterManager, culture), culture);
                    }
                    else
                    {
                        Sports.Add(id, new SportCI(item, _dataRouterManager, culture));
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
                    ExecutionLog.LogError($"Error saving SportDTO for {id} and lang={culture.TwoLetterISOLanguageName}.", e);
                }
            }
        }
        private void AddSport(URN id, SportEntityDTO item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Sports.ContainsKey(id))
                    {
                        Sports.TryGetValue(id, out var ci);
                        ci?.Merge(new SportCI(new SportDTO(item.Id.ToString(), item.Name, (IEnumerable<tournamentExtended>) null), _dataRouterManager, culture), culture);
                    }
                    else
                    {
                        Sports.Add(id, new SportCI(new SportDTO(item.Id.ToString(), item.Name, (IEnumerable<tournamentExtended>) null), _dataRouterManager, culture));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError($"Error saving SportEntityDTO for {id} and lang={culture.TwoLetterISOLanguageName}.", e);
                }
            }
        }
        private void AddSport(URN id, SportCategoriesDTO item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Sports.ContainsKey(id))
                    {
                        Sports.TryGetValue(id, out var ci);
                        ci?.Merge(new SportCI(new SportDTO(item.Sport.Id.ToString(), item.Sport.Name, item.Categories), _dataRouterManager, culture), culture);
                    }
                    else
                    {
                        Sports.Add(id, new SportCI(new SportDTO(item.Sport.Id.ToString(), item.Sport.Name, item.Categories), _dataRouterManager, culture));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError($"Error saving SportEntityDTO for {id} and lang={culture.TwoLetterISOLanguageName}.", e);
                }
            }
        }

        private void AddSport(ExportableSportCI item)
        {
            lock (_mergeLock)
            {
                try
                {
                    var id = URN.Parse(item.Id);
                    if (Sports.ContainsKey(id))
                    {
                        Sports.TryGetValue(id, out var ci);
                        ci?.Merge(new SportCI(item), item.Name.Keys.First());
                    }
                    else
                    {
                        Sports.Add(id, new SportCI(item));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError($"Error importing  ExportableSportCI for {item.Id}.", e);
                }
            }
        }

        /// <summary>
        /// Adds the category
        /// </summary>
        /// <param name="id">The identifier of the CategoryId or SportId !!!</param>
        /// <param name="item">The category dto item</param>
        /// <param name="culture">The culture</param>
        private void AddCategory(URN id, CategoryDTO item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                try
                {
                    if (Categories.ContainsKey(item.Id))
                    {
                        Categories.TryGetValue(item.Id, out var ci);
                        ci?.Merge(new CategoryCI(item, culture, item.Tournaments?.FirstOrDefault()?.Sport.Id ?? id), culture);
                    }
                    else
                    {
                        Categories.Add(item.Id, new CategoryCI(item, culture, item.Tournaments?.FirstOrDefault()?.Sport.Id ?? id));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError($"Error saving CategoryDTO for {id} and lang={culture.TwoLetterISOLanguageName}.", e);
                }
            }
        }

        private void AddCategory(URN id, CategorySummaryDTO item, URN sportId, IEnumerable<URN> tournamentIds, CultureInfo culture)
        {

            lock (_mergeLock)
            {
                try
                {
                    if (Categories.ContainsKey(id))
                    {
                        Categories.TryGetValue(id, out var ci);
                        ci?.Merge(new CategoryCI(item, culture, sportId, tournamentIds), culture);
                    }
                    else
                    {
                        Categories.Add(id, new CategoryCI(item, culture, sportId, tournamentIds));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError($"Error saving CategorySummaryDTO for {id} and lang={culture.TwoLetterISOLanguageName}.", e);
                }
            }
        }

        private void AddCategory(ExportableCategoryCI item)
        {
            lock (_mergeLock)
            {
                try
                {
                    var id = URN.Parse(item.Id);
                    if (Categories.ContainsKey(id))
                    {
                        Categories.TryGetValue(id, out var ci);
                        ci?.Merge(new CategoryCI(item), item.Name.Keys.First());
                    }
                    else
                    {
                        Categories.Add(id, new CategoryCI(item));
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogError($"Error importing ExportableCategoryCI for {item.Id}.", e);
                }
            }
        }

        private void AddCategories(SportCategoriesDTO item, CultureInfo culture)
        {
            lock (_mergeLock)
            {
                foreach (var category in item.Categories)
                {
                    try
                    {
                        if (Categories.ContainsKey(category.Id))
                        {
                            CategoryCI ci;
                            Categories.TryGetValue(category.Id, out ci);
                            ci?.Merge(new CategoryCI(category, culture, item.Sport.Id), culture);
                        }
                        else
                        {
                            Categories.Add(category.Id, new CategoryCI(category, culture, item.Sport.Id));
                        }
                    }
                    catch (Exception e)
                    {
                        ExecutionLog.LogError($"Error saving CategoryDTO for {category.Id} and lang={culture.TwoLetterISOLanguageName}.", e);
                    }
                }
            }
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableCI"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableCI>> ExportAsync()
        {
            List<IExportableCI> exportables;
            lock (_mergeLock)
            {
                exportables = Sports.Values.Cast<IExportableCI>().Concat(Categories.Values).ToList();
            }

            var tasks = exportables.Select(e =>
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
                if (exportable is ExportableSportCI exportableSport)
                {
                    AddSport(exportableSport);
                    continue;
                }

                if (exportable is ExportableCategoryCI exportableCategory)
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
                    {typeof(SportCI).Name, Sports.Count},
                    {typeof(CategoryCI).Name, Categories.Count}
                };
            }
        }
    }
}

