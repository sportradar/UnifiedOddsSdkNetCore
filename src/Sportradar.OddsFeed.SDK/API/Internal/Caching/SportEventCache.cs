// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

// ReSharper disable InconsistentlySynchronizedField

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// A implementation of the interface <see cref="ISportEventCache"/>
    /// </summary>
    /// <seealso cref="ISportEventCache" />
    internal class SportEventCache : SdkCache, ISportEventCache
    {
        /// <summary>
        /// The list of dates already automatically loaded by the timer
        /// </summary>
        private readonly List<DateTime> _fetchedDates;

        /// <summary>
        /// A <see cref="ICacheStore{T}"/> which will be used to cache the data
        /// </summary>
        internal readonly ICacheStore<string> Cache;

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// >A instance of <see cref="ISportEventCacheItemFactory"/> used to create new <see cref="SportEventCacheItem"/>
        /// </summary>
        private readonly ISportEventCacheItemFactory _sportEventCacheItemFactory;

        /// <summary>
        /// The list of all supported <see cref="CultureInfo"/>
        /// </summary>
        private readonly IReadOnlyCollection<CultureInfo> _cultures;

        /// <summary>
        /// A <see cref="ISdkTimer"/> instance used to trigger periodic cache refresh-es
        /// </summary>
        private readonly ISdkTimer _timer;

        /// <summary>
        /// The timer semaphore slim used reduce concurrency within timer calls
        /// </summary>
        private readonly SemaphoreSlim _timerSemaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// Value specifying whether the current instance is disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// The special tournaments, which are not listed on All tournaments list, but are introduced by events on feed messages
        /// </summary>
        internal readonly ConcurrentBag<Urn> SpecialTournaments;

        internal LockManager LockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCache"/> class
        /// </summary>
        /// <param name="cache">The in-memory cache of sport events</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain data</param>
        /// <param name="sportEventCacheItemFactory">A instance of <see cref="ISportEventCacheItemFactory"/> used to create new <see cref="SportEventCacheItem"/></param>
        /// <param name="timer">The timer</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="loggerFactory">The logger factory for creating Cache and Execution logs</param>
        public SportEventCache(ICacheStore<string> cache,
                               IDataRouterManager dataRouterManager,
                               ISportEventCacheItemFactory sportEventCacheItemFactory,
                               ISdkTimer timer,
                               IReadOnlyCollection<CultureInfo> cultures,
                               ICacheManager cacheManager,
                               ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
            Guard.Argument(cache, nameof(cache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(sportEventCacheItemFactory, nameof(sportEventCacheItemFactory)).NotNull();
            Guard.Argument(timer, nameof(timer)).NotNull();
            if (cultures.IsNullOrEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(cultures));
            }

            Cache = cache;
            _dataRouterManager = dataRouterManager;
            _sportEventCacheItemFactory = sportEventCacheItemFactory;
            _cultures = cultures;

            _fetchedDates = new List<DateTime>();

            _isDisposed = false;

            SpecialTournaments = new ConcurrentBag<Urn>();

            LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200), loggerFactory.CreateLogger<LockManager>());

            _timer = timer;
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            Task.Run(async () =>
                     {
                         await OnTimerElapsedAsync().ConfigureAwait(false);
                         await LockManager.CleanAsync();
                     });
        }

        /// <summary>
        /// Invoked when the internally used timer elapses
        /// </summary>
        private async Task OnTimerElapsedAsync()
        {
            //check what needs to be fetched, then go fetched by culture, (not by date)
            var datesToFetch = new List<DateTime>();

            await _timerSemaphoreSlim.WaitAsync(TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            var date = DateTime.Now;
            for (var i = 0; i < 3; i++)
            {
                if (_fetchedDates.Any(d => (date - d).TotalDays < 1))
                {
                    continue;
                }

                datesToFetch.Add(date);
                _fetchedDates.Add(date);
                date = date.AddDays(1);
            }

            if (!datesToFetch.Any())
            {
                _timerSemaphoreSlim.ReleaseSafe();
                return;
            }

            var culturesToFetch = _cultures.ToDictionary(ci => ci, ci => datesToFetch);

            using (new TelemetryTracker(UofSdkTelemetry.SportEventCacheGetAll))
            {
                foreach (var key in culturesToFetch)
                {
                    try
                    {
                        var tasks = key.Value.Select(d => GetScheduleAsync(d, key.Key)).ToList();
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        ExecutionLog.LogWarning("Periodic events schedule retrieval failed because the instance {DisposedObject} is being disposed", ex.ObjectName);
                    }
                    catch (TaskCanceledException)
                    {
                        ExecutionLog.LogWarning("Periodic events schedule retrieval failed because the instance is being disposed");
                    }
                    catch (FeedSdkException ex)
                    {
                        ExecutionLog.LogWarning(ex, "An exception occurred while attempting to retrieve schedule");
                    }
                    catch (AggregateException ex)
                    {
                        var baseException = ex.GetBaseException();
                        if (baseException.GetType() == typeof(ObjectDisposedException))
                        {
                            ExecutionLog.LogWarning("Error happened during fetching schedule, because the instance {DisposedObject} is being disposed", ((ObjectDisposedException)baseException).ObjectName);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExecutionLog.LogWarning(ex, "An exception occurred while attempting to retrieve schedule");
                    }
                }
            }

            _timerSemaphoreSlim.ReleaseSafe();
        }

        /// <summary>
        /// Asynchronously gets a sport event schedule specified by the <c>id</c> in the language specified by <c>culture</c>
        /// </summary>
        /// <param name="date">The value specifying which schedule to get</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the retrieved schedule</param>
        /// <returns>A <see cref="Task" /> representing the retrieval operation</returns>
        private async Task GetScheduleAsync(DateTime date, CultureInfo culture)
        {
            Guard.Argument(date, nameof(date)).Require(date > DateTime.MinValue);

            var fetchedItem = await _dataRouterManager.GetSportEventsForDateAsync(date, culture).ConfigureAwait(false);

            CacheLog.LogInformation("{ResponseItemCount} sport events retrieved for {Date} and language {Lang}", fetchedItem.Count(), date.ToShortDateString(), culture.TwoLetterISOLanguageName);
        }

        /// <summary>
        /// Gets a <see cref="SportEventCacheItem"/> instance representing cached sport event data
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of the sport event which cached representation to return</param>
        /// <returns>a <see cref="SportEventCacheItem"/> instance representing cached sport event data</returns>
        public ISportEventCacheItem GetEventCacheItem(Urn id)
        {
            LockManager.Wait(id.ToString());
            try
            {
                var item = (SportEventCacheItem)Cache.Get(id.ToString());
                if (item != null)
                {
                    return item;
                }

                item = _sportEventCacheItemFactory.Build(id);

                AddNewCacheItem(item);

                // if there are events for non-standard tournaments (tournaments not on All tournaments for all sports)
                if (item is TournamentInfoCacheItem && !SpecialTournaments.Contains(item.Id))
                {
                    SpecialTournaments.Add(item.Id);
                }

                return item;
            }
            catch (Exception ex)
            {
                ExecutionLog.LogError(ex, "Error getting cache item for id={EventId}", id);
            }
            finally
            {
                LockManager.Release(id.ToString());
            }
            return null;
        }

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{Urn}"/> containing id's of sport events, which belong to the specified tournament
        /// </summary>
        /// <param name="tournamentId">A <see cref="Urn"/> representing the tournament identifier</param>
        /// <param name="culture">The culture to fetch the data</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetEventIdsAsync(Urn tournamentId, CultureInfo culture)
        {
            var ci = culture ?? _cultures.FirstOrDefault();

            var schedule = await _dataRouterManager.GetSportEventsForTournamentAsync(tournamentId, ci, null).ConfigureAwait(false);

            return schedule;
        }

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{Urn}"/> containing id's of sport events, which belong to the specified tournament
        /// </summary>
        /// <param name="tournamentId">A <see cref="Urn"/> representing the tournament identifier</param>
        /// <param name="cultures">A list of <see cref="CultureInfo"/> used to fetch schedules</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task GetEventIdsAsync(Urn tournamentId, IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures?.ToList() ?? _cultures.ToList();

            var tasks = wantedCultures.Select(s => _dataRouterManager.GetSportEventsForTournamentAsync(tournamentId, s, null));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{Urn}"/> containing id's of sport events, which are scheduled for specified date
        /// </summary>
        /// <param name="date">The date for which to retrieve the schedule, or a null reference to get currently live events</param>
        /// <param name="culture">The culture to fetch the data</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetEventIdsAsync(DateTime? date, CultureInfo culture)
        {
            var ci = culture ?? _cultures.FirstOrDefault();

            var schedule = date == null
                               ? await _dataRouterManager.GetLiveSportEventsAsync(ci).ConfigureAwait(false)
                               : await _dataRouterManager.GetSportEventsForDateAsync(date.Value, ci).ConfigureAwait(false);

            return schedule;
        }

        /// <summary>
        /// Adds fixture timestamp to cache so that the next fixture calls for the event goes through non-cached fixture provider
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the event</param>
        public void AddFixtureTimestamp(Urn id)
        {
            var cache = _sportEventCacheItemFactory.GetFixtureTimestampCache();
            cache.Add(id.ToString(), id);
        }

        /// <summary>
        /// Asynchronously gets a list of active <see cref="IEnumerable{TournamentInfoCacheItem}"/>
        /// </summary>
        /// <remarks>Lists all <see cref="TournamentInfoCacheItem"/> that are cached (once schedule is loaded)</remarks>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<TournamentInfoCacheItem>> GetActiveTournamentsAsync(CultureInfo culture = null)
        {
            await OnTimerElapsedAsync().ConfigureAwait(false); // this is almost not needed anymore, since we call all tournaments for all sports :)
            var tourKeys = Cache.GetKeys().Where(w => w.Contains("tournament") || w.Contains("stage") || w.Contains("outright") || w.Contains("simple_tournament"));

            var tours = new List<TournamentInfoCacheItem>();
            var errorBuilder = new StringBuilder();
            foreach (var key in tourKeys)
            {
                try
                {
                    tours.Add((TournamentInfoCacheItem)_sportEventCacheItemFactory.Get(Cache.Get(key)));
                }
                catch (Exception)
                {
                    errorBuilder.Append($",{key}");
                }
            }

            var error = errorBuilder.ToString();
            if (!string.IsNullOrEmpty(error))
            {
                error = error.Substring(1);
            }
            ExecutionLog.LogDebug("Found {ResponseItemCount} tournaments. Errors: {Error}", tours.Count, error);

            return tours;
        }

        /// <summary>
        /// Deletes the sport events from cache which are scheduled before specific DateTime
        /// </summary>
        /// <param name="before">The scheduled DateTime used to delete sport events from cache</param>
        /// <returns>Number of deleted items</returns>
        public int DeleteSportEventsFromCache(DateTime before)
        {
            var deletedItemsCount = 0;
            LockManager.Wait();
            try
            {
                var startCount = Cache.Count();
                foreach (var cacheItems in Cache.GetValues())
                {
                    try
                    {
                        var ci = (SportEventCacheItem)cacheItems;
                        if (ci.Scheduled != null && ci.Scheduled < before && ci.ScheduledEnd == null)
                        {
                            Cache.Remove(ci.Id.ToString());
                        }
                        else if (ci.ScheduledEnd != null && ci.ScheduledEnd < before)
                        {
                            Cache.Remove(ci.Id.ToString());
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                var endCount = Cache.Count();
                deletedItemsCount = startCount - endCount;
                ExecutionLog.LogInformation("Deleted {DeletedItemsCount} items from cache (before={Before})", deletedItemsCount, before);
                return startCount - endCount;
            }
            catch (Exception e)
            {
                ExecutionLog.LogError(e, "Error during DeleteSportEventsFromCache");
            }
            finally
            {
                LockManager.Release();
            }
            return deletedItemsCount;
        }

        /// <summary>
        /// Asynchronous gets a <see cref="Urn"/> of event's sport id
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the event identifier</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task<Urn> GetEventSportIdAsync(Urn id)
        {
            try
            {
                if (!Cache.Contains(id.ToString()))
                {
                    await _dataRouterManager.GetSportEventSummaryAsync(id, _cultures.First(), null).ConfigureAwait(false);
                }

                var item = (SportEventCacheItem)Cache.Get(id.ToString());
                if (item != null)
                {
                    var sportId = await item.GetSportIdAsync().ConfigureAwait(false);
                    if (sportId != null)
                    {
                        return sportId;
                    }
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.LogError(ex, "Error getting cache item sportId for id={EventId}", id);
            }

            return null;
        }

        /// <inheritdoc />
        protected override async Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            if (_isDisposed)
            {
                return false;
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.Category:
                    break;
                case DtoType.Competitor:
                    break;
                case DtoType.CompetitorProfile:
                    break;
                case DtoType.SimpleTeamProfile:
                    break;
                case DtoType.Fixture:
                    var fixture = item as FixtureDto;
                    if (fixture != null)
                    {
                        AddSportEvent(id, fixture, culture, requester, dtoType);
                        saved = true;
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
                    if (match != null)
                    {
                        AddSportEvent(id, match, culture, requester, dtoType);
                        if (match.Tournament != null)
                        {
                            var ti = new TournamentInfoDto(match.Tournament);
                            AddSportEvent(ti.Id, ti, culture, requester, dtoType);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDto), item.GetType());
                    }
                    break;
                case DtoType.MatchTimeline:
                    var timeline = item as MatchTimelineDto;
                    if (timeline != null)
                    {
                        AddMatchTimeLine(timeline, culture, requester, dtoType);
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
                    var stage = item as StageDto;
                    if (stage != null)
                    {
                        AddSportEvent(id, stage, culture, requester, dtoType);
                        if (stage.Tournament != null)
                        {
                            var ti = new TournamentInfoDto(stage.Tournament);
                            AddSportEvent(ti.Id, ti, culture, requester, dtoType);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDto), item.GetType());
                    }
                    break;
                case DtoType.Sport:
                    var sport = item as SportDto;
                    if (sport != null)
                    {
                        await SaveTournamentDataFromSportAsync(sport, culture).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDto), item.GetType());
                    }
                    break;
                case DtoType.SportList:
                    var sportEntityList = item as EntityList<SportDto>;
                    if (sportEntityList != null)
                    {
                        var tasks = sportEntityList.Items.Select(s => SaveTournamentDataFromSportAsync(s, culture));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
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
                    var tourInfo = item as TournamentInfoDto;
                    if (tourInfo != null)
                    {
                        await SaveTournamentDataToSportEventCacheAsync(tourInfo, tourInfo.CurrentSeason?.Id, culture).ConfigureAwait(false);
                        if (tourInfo.Season != null)
                        {
                            await SaveTournamentDataToSportEventCacheAsync(tourInfo, tourInfo.Season?.Id, culture).ConfigureAwait(false);
                        }
                        break;
                    }
                    var summary = item as SportEventSummaryDto;
                    if (summary != null)
                    {
                        AddSportEvent(id, summary, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDto), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummaryList:
                    var summaryList = item as EntityList<SportEventSummaryDto>;
                    if (summaryList != null && !summaryList.Items.IsNullOrEmpty())
                    {
                        var tasks = summaryList.Items.Select(s => SaveSportEventSummaryDtoAsync(s, culture, dtoType, requester));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportEventSummaryDto>), item.GetType());
                    }
                    break;
                case DtoType.Tournament:
                    var t = item as TournamentDto;
                    if (t != null)
                    {
                        var ti = new TournamentInfoDto(t);
                        AddSportEvent(id, ti, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentDto), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfo:
                    var tour = item as TournamentInfoDto;
                    if (tour != null)
                    {
                        AddSportEvent(id, tour, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentInfoDto), item.GetType());
                    }
                    break;
                case DtoType.TournamentSeasons:
                    var tourSeasons = item as TournamentSeasonsDto;
                    if (tourSeasons?.Tournament != null)
                    {
                        AddSportEvent(id, tourSeasons.Tournament, culture, requester, dtoType);
                        var cacheItem = (TournamentInfoCacheItem)_sportEventCacheItemFactory.Get(Cache.Get(id.ToString()));
                        cacheItem?.Merge(tourSeasons, culture, true);

                        if (tourSeasons.Seasons != null && tourSeasons.Seasons.Any())
                        {
                            foreach (var season in tourSeasons.Seasons)
                            {
                                AddSportEvent(season.Id, new TournamentInfoDto(season), culture, null, dtoType);
                            }
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentSeasonsDto), item.GetType());
                    }
                    break;
                case DtoType.MarketDescriptionList:
                    break;
                case DtoType.VariantDescription:
                    break;
                case DtoType.VariantDescriptionList:
                    break;
                case DtoType.Lottery:
                    var lottery = item as LotteryDto;
                    if (lottery != null)
                    {
                        AddSportEvent(id, lottery, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(LotteryDto), item.GetType());
                    }
                    break;
                case DtoType.LotteryDraw:
                    var draw = item as DrawDto;
                    if (draw != null)
                    {
                        AddSportEvent(id, draw, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(DrawDto), item.GetType());
                    }
                    break;
                case DtoType.LotteryList:
                    var lotteryList = item as EntityList<LotteryDto>;
                    if (lotteryList != null && lotteryList.Items.Any())
                    {
                        foreach (var l in lotteryList.Items)
                        {
                            AddSportEvent(l.Id, l, culture, requester, dtoType);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<LotteryDto>), item.GetType());
                    }
                    break;
                case DtoType.BookingStatus:
                    if (Cache.Contains(id.ToString()))
                    {
                        var e = Cache.Get(id.ToString());
                        var comp = e as CompetitionCacheItem;
                        comp?.Book();
                    }
                    break;
                case DtoType.SportCategories:
                    break;
                case DtoType.AvailableSelections:
                    break;
                case DtoType.TournamentInfoList:
                    var ts = item as EntityList<TournamentInfoDto>;
                    if (ts != null)
                    {
                        foreach (var t1 in ts.Items)
                        {
                            AddSportEvent(id, t1, culture, requester, dtoType);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<TournamentInfoDto>), item.GetType());
                    }
                    break;
                case DtoType.PeriodSummary:
                    break;
                case DtoType.Calculation:
                    break;
                default:
                    ExecutionLog.LogWarning("Trying to add unchecked dto type:{DtoType} for id: {EventId}", dtoType, id);
                    break;
            }
            return saved;
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.Fixture,
                                     DtoType.MatchSummary,
                                     DtoType.MatchTimeline,
                                     DtoType.RaceSummary,
                                     DtoType.Sport,
                                     DtoType.SportList,
                                     DtoType.SportEventSummary,
                                     DtoType.SportEventSummaryList,
                                     DtoType.Tournament,
                                     DtoType.TournamentInfo,
                                     DtoType.TournamentSeasons,
                                     DtoType.Lottery,
                                     DtoType.LotteryDraw,
                                     DtoType.LotteryList,
                                     DtoType.BookingStatus,
                                     DtoType.TournamentInfoList
                                 };
        }

        /// <summary>
        /// Purges item from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the id of the sport event which to be deleted</param>
        /// <param name="cacheItemType">The cache item type to be deleted</param>
        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.SportEvent || cacheItemType == CacheItemType.Tournament)
            {
                LockManager.Wait(id.ToString());
                Cache.Remove(id.ToString());
                LockManager.Release(id.ToString());
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
            var result = false;
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.SportEvent || cacheItemType == CacheItemType.Tournament)
            {
                LockManager.Wait(id.ToString());
                result = Cache.Contains(id.ToString());
                LockManager.Release(id.ToString());
            }

            return result;
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
                _timer.Dispose();
                _timerSemaphoreSlim.ReleaseSafe();
                _timerSemaphoreSlim.Dispose();
                Cache.Dispose();
            }
            _isDisposed = true;
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

        private async Task SaveSportEventSummaryDtoAsync(SportEventSummaryDto sportEventSummaryDto, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            var tourInfosDto = sportEventSummaryDto as TournamentInfoDto;
            if (tourInfosDto != null)
            {
                await SaveTournamentDataToSportEventCacheAsync(tourInfosDto, tourInfosDto.CurrentSeason?.Id, culture).ConfigureAwait(false);
                if (tourInfosDto.Season != null)
                {
                    await SaveTournamentDataToSportEventCacheAsync(tourInfosDto, tourInfosDto.Season?.Id, culture).ConfigureAwait(false);
                }
                return;
            }
            AddSportEvent(sportEventSummaryDto.Id, sportEventSummaryDto, culture, requester, dtoType);
        }

        private async Task SaveTournamentDataFromSportAsync(SportDto sportDto, CultureInfo culture)
        {
            if (sportDto.Categories != null)
            {
                foreach (var categoryData in sportDto.Categories)
                {
                    if (categoryData.Tournaments != null)
                    {
                        foreach (var tournamentData in categoryData.Tournaments)
                        {
                            await SaveTournamentDataToSportEventCacheAsync(tournamentData, tournamentData.CurrentSeason?.Id, culture).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private async Task SaveTournamentDataToSportEventCacheAsync(TournamentDto tournamentDto, Urn secondId, CultureInfo culture)
        {
            await CacheAddDtoItemAsync(tournamentDto.Id, tournamentDto, culture, DtoType.Tournament, null).ConfigureAwait(false);

            if (secondId != null && !Equals(tournamentDto.Id, secondId))
            {
                var tourInfoDto = new TournamentInfoDto(tournamentDto);
                var newTournamentDto = new TournamentInfoDto(tourInfoDto, tourInfoDto.Season != null, tourInfoDto.CurrentSeason != null);
                await CacheAddDtoItemAsync(secondId, newTournamentDto, culture, DtoType.TournamentInfo, null).ConfigureAwait(false);
            }
        }

        private async Task SaveTournamentDataToSportEventCacheAsync(TournamentInfoDto tournamentDto, Urn secondId, CultureInfo culture)
        {
            await CacheAddDtoItemAsync(tournamentDto.Id, tournamentDto, culture, DtoType.TournamentInfo, null).ConfigureAwait(false);

            if (secondId != null && !Equals(tournamentDto.Id, secondId))
            {
                var newTournamentDto = new TournamentInfoDto(tournamentDto, tournamentDto.Season != null, tournamentDto.CurrentSeason != null);
                await CacheAddDtoItemAsync(secondId, newTournamentDto, culture, DtoType.TournamentInfo, null).ConfigureAwait(false);
            }
        }

        private void SaveParentStage(Urn parentId, StageDto parentStage, TournamentDto tournamentDto, CultureInfo culture)
        {
            if (parentId == null || parentStage == null)
            {
                return;
            }
            var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(parentId.ToString()));
            if (cacheItem == null)
            {
                var tournamentId = tournamentDto?.Id;
                if (parentId.Equals(tournamentId))
                {
                    var stageDtoFromTournament = new StageDto(tournamentDto);
                    var ci2 = (StageCacheItem)_sportEventCacheItemFactory.Build(stageDtoFromTournament, culture);
                    ci2.Merge(parentStage, culture, false);
                    AddNewCacheItem(ci2);
                }
                else
                {
                    var ci2 = (StageCacheItem)_sportEventCacheItemFactory.Build(parentStage, culture);
                    AddNewCacheItem(ci2);
                }
            }
            else
            {
                if (cacheItem is StageCacheItem stageCacheItem)
                {
                    stageCacheItem.Merge(parentStage, culture, false);
                }
                else
                {
                    // something wrong
                    var _ = cacheItem.GetType().Name;
                }
            }
        }

        private void SaveLotteryDraw(DrawDto draw, CultureInfo culture)
        {
            if (draw == null)
            {
                return;
            }
            var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(draw.Id.ToString()));
            if (cacheItem == null)
            {
                var ci2 = (DrawCacheItem)_sportEventCacheItemFactory.Build(draw, culture);
                AddNewCacheItem(ci2);
            }
            else
            {
                if (cacheItem is DrawCacheItem drawCacheItem)
                {
                    drawCacheItem.Merge(draw, culture, false);
                }
                else
                {
                    //ignored
                }
            }
        }

        private void AddSportEvent(Urn id, SportEventSummaryDto item, CultureInfo culture, ISportEventCacheItem requester, DtoType dtoType)
        {
            TournamentInfoDto tournamentInfoDto = null;
            LockManager.Wait(id.ToString());
            try
            {
                var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(id.ToString()));

                if (requester != null && !Equals(requester, cacheItem) && id.Equals(requester.Id))
                {
                    try
                    {
                        var requesterMerged = false;
                        var fixture = item as FixtureDto;
                        if (fixture != null)
                        {
                            if (requester.Id.TypeGroup == ResourceTypeGroup.Match)
                            {
                                ((MatchCacheItem)requester).MergeFixture(fixture, culture, true);
                            }
                            else if (requester.Id.TypeGroup == ResourceTypeGroup.Stage)
                            {
                                var stageCacheItem = (StageCacheItem)requester;
                                stageCacheItem.MergeFixture(fixture, culture, true);
                                if (fixture.ParentStage != null)
                                {
                                    SaveParentStage(fixture.ParentStage.Id, fixture.ParentStage, fixture.Tournament, culture);
                                }

                                if (!fixture.AdditionalParents.IsNullOrEmpty())
                                {
                                    foreach (var parent in fixture.AdditionalParents)
                                    {
                                        SaveParentStage(parent.Id, parent, fixture.Tournament, culture);
                                    }
                                }
                            }
                            else
                            {
                                ((TournamentInfoCacheItem)requester).MergeFixture(fixture, culture, true);
                            }

                            requesterMerged = true;
                        }

                        if (!requesterMerged)
                        {
                            var match = item as MatchDto;
                            if (match != null)
                            {
                                ((MatchCacheItem)requester).Merge(match, culture, true);
                                requesterMerged = true;
                            }
                        }

                        if (!requesterMerged)
                        {
                            var stage = item as StageDto;
                            if (stage != null)
                            {
                                ((StageCacheItem)requester).Merge(stage, culture, true);
                                if (stage.ParentStage != null)
                                {
                                    SaveParentStage(stage.ParentStage.Id, stage.ParentStage, stage.Tournament, culture);
                                }

                                if (!stage.AdditionalParents.IsNullOrEmpty())
                                {
                                    foreach (var parent in stage.AdditionalParents)
                                    {
                                        SaveParentStage(parent.Id, parent, stage.Tournament, culture);
                                    }
                                }

                                requesterMerged = true;
                            }
                        }

                        if (!requesterMerged)
                        {
                            var tour = item as TournamentInfoDto;
                            if (tour != null)
                            {
                                var stageCacheItem = requester as StageCacheItem;
                                if (stageCacheItem != null)
                                {
                                    stageCacheItem.Merge(tour, culture, true);
                                    requesterMerged = true;
                                }
                                else
                                {
                                    var tourCacheItem = requester as TournamentInfoCacheItem;
                                    if (tourCacheItem != null)
                                    {
                                        tourCacheItem.Merge(tour, culture, true);
                                        requesterMerged = true;
                                    }
                                }
                            }
                        }

                        if (!requesterMerged)
                        {
                            var draw = item as DrawDto;
                            if (draw != null)
                            {
                                ((DrawCacheItem)requester).Merge(draw, culture, true);
                                requesterMerged = true;
                            }
                        }

                        if (!requesterMerged)
                        {
                            var lottery = item as LotteryDto;
                            if (lottery != null)
                            {
                                ((LotteryCacheItem)requester).Merge(lottery, culture, true);
                                if (!lottery.DrawEvents.IsNullOrEmpty())
                                {
                                    foreach (var drawEvent in lottery.DrawEvents)
                                    {
                                        SaveLotteryDraw(drawEvent, culture);
                                    }
                                }

                                requesterMerged = true;
                            }
                        }

                        if (!requesterMerged)
                        {
                            requester.Merge(item, culture, true);
                        }
                    }
                    catch (Exception)
                    {
                        ExecutionLog.LogDebug("Merging failed for {SportEventId} and item type: {SportEventType} and dto type: {DtoType} for requester: {RequesterId}", id, item.GetType().Name, dtoType, requester.Id);
                    }
                }

                if (cacheItem != null)
                {
                    //ExecutionLog.LogDebug($"Saving OLD data for {id} and item type: {item.GetType().Name} and dto type: {dtoType}.");
                    var merged = false;
                    //var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(id.ToString()));
                    var fixture = item as FixtureDto;
                    if (fixture != null)
                    {
                        if (cacheItem.Id.TypeGroup == ResourceTypeGroup.Match)
                        {
                            ((MatchCacheItem)cacheItem).MergeFixture(fixture, culture, true);
                        }
                        else if (cacheItem.Id.TypeGroup == ResourceTypeGroup.Stage)
                        {
                            ((StageCacheItem)cacheItem).MergeFixture(fixture, culture, true);
                            if (fixture.ParentStage != null)
                            {
                                SaveParentStage(fixture.ParentStage.Id, fixture.ParentStage, fixture.Tournament, culture);
                            }

                            if (!fixture.AdditionalParents.IsNullOrEmpty())
                            {
                                foreach (var parent in fixture.AdditionalParents)
                                {
                                    SaveParentStage(parent.Id, parent, fixture.Tournament, culture);
                                }
                            }
                        }
                        else
                        {
                            ((TournamentInfoCacheItem)cacheItem).MergeFixture(fixture, culture, true);
                        }

                        if (fixture.Tournament != null)
                        {
                            tournamentInfoDto = new TournamentInfoDto(fixture.Tournament);
                        }

                        merged = true;
                    }

                    if (!merged)
                    {
                        var stage = item as StageDto;
                        if (stage != null)
                        {
                            ((StageCacheItem)cacheItem).Merge(stage, culture, true);
                            merged = true;
                            if (stage.Tournament != null)
                            {
                                tournamentInfoDto = new TournamentInfoDto(stage.Tournament);
                            }

                            if (stage.ParentStage != null)
                            {
                                SaveParentStage(stage.ParentStage.Id, stage.ParentStage, stage.Tournament, culture);
                            }

                            if (!stage.AdditionalParents.IsNullOrEmpty())
                            {
                                foreach (var parent in stage.AdditionalParents)
                                {
                                    SaveParentStage(parent.Id, parent, stage.Tournament, culture);
                                }
                            }
                        }
                    }

                    if (!merged)
                    {
                        var tour = item as TournamentInfoDto;
                        if (tour != null)
                        {
                            var stageCacheItem = cacheItem as StageCacheItem;
                            if (stageCacheItem != null)
                            {
                                stageCacheItem.Merge(tour, culture, true);
                                merged = true;
                            }
                            else
                            {
                                var tourCacheItem = cacheItem as TournamentInfoCacheItem;
                                if (tourCacheItem != null)
                                {
                                    tourCacheItem.Merge(tour, culture, true);
                                    merged = true;
                                }
                            }
                        }
                    }

                    if (!merged)
                    {
                        var match = item as MatchDto;
                        if (match != null)
                        {
                            ((MatchCacheItem)cacheItem).Merge(match, culture, true);
                            merged = true;
                            if (match.Tournament != null)
                            {
                                tournamentInfoDto = new TournamentInfoDto(match.Tournament);
                            }
                        }
                    }

                    if (!merged)
                    {
                        var draw = item as DrawDto;
                        if (draw != null)
                        {
                            ((DrawCacheItem)cacheItem).Merge(draw, culture, true);
                            merged = true;
                        }
                    }

                    if (!merged)
                    {
                        var lottery = item as LotteryDto;
                        if (lottery != null)
                        {
                            ((LotteryCacheItem)cacheItem).Merge(lottery, culture, true);
                            if (!lottery.DrawEvents.IsNullOrEmpty())
                            {
                                foreach (var drawEvent in lottery.DrawEvents)
                                {
                                    SaveLotteryDraw(drawEvent, culture);
                                }
                            }

                            merged = true;
                        }
                    }

                    if (!merged)
                    {
                        cacheItem.Merge(item, culture, true);
                    }
                }
                else
                {
                    //ExecutionLog.LogDebug($"Saving NEW data for {id} and item type: {item.GetType().Name} and dto type: {dtoType}.");
                    var ci = _sportEventCacheItemFactory.Build(item, culture);
                    if (dtoType == DtoType.SportEventSummary || dtoType == DtoType.LotteryDraw || dtoType == DtoType.MatchSummary)
                    {
                        ci.LoadedSummaries.Add(culture);
                    }
                    else if (dtoType == DtoType.Fixture)
                    {
                        ci.LoadedFixtures.Add(culture);
                    }

                    AddNewCacheItem(ci);
                    if (!ci.Id.Equals(id))
                    {
                        var tInfo = item as TournamentInfoDto;
                        if (tInfo != null)
                        {
                            var newTournamentDto = new TournamentInfoDto(tInfo, tInfo.Season != null, tInfo.CurrentSeason != null);
                            var ci2 = _sportEventCacheItemFactory.Build(newTournamentDto, culture);
                            AddNewCacheItem(ci2);
                        }
                        else
                        {
                            var ci2 = _sportEventCacheItemFactory.Build(item, culture);
                            ci2.Id = id;
                            AddNewCacheItem(ci2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.LogError(ex, "Error adding sport event for id={SportEventId}, dto type={DtoType} and lang={Lang}", id, item?.GetType().Name, culture.TwoLetterISOLanguageName);
            }
            finally
            {
                LockManager.Release(id.ToString());
            }

            // if there are events for non-standard tournaments (tournaments not on All tournaments for all sports)
            // TODO: should we save/merge all, or just adding non-existing???
            if (tournamentInfoDto != null && (SpecialTournaments.Contains(tournamentInfoDto.Id) || !Cache.Contains(tournamentInfoDto.Id.ToString())))
            {
                if (SpecialTournaments.Contains(tournamentInfoDto.Id))
                {
                }
                else
                {
                    SpecialTournaments.Add(tournamentInfoDto.Id);
                }
                AddSportEvent(tournamentInfoDto.Id, tournamentInfoDto, culture, null, dtoType);
            }
        }

        private void AddMatchTimeLine(MatchTimelineDto item, CultureInfo culture, ISportEventCacheItem requester, DtoType dtoType)
        {
            AddSportEvent(item.SportEvent.Id, item.SportEvent, culture, requester, dtoType);

            LockManager.Wait(item.SportEvent.Id.ToString());
            try
            {
                UpdateMatchWithTimeline(item, culture);
            }
            catch (Exception ex)
            {
                ExecutionLog.LogError(ex, "Error adding timeline for id={SportEventId}, dto type={DtoName} and lang={Lang}", item.SportEvent.Id, item.GetType().Name, culture.TwoLetterISOLanguageName);
            }
            finally
            {
                LockManager.Release(item.SportEvent.Id.ToString());
            }
        }

        private void UpdateMatchWithTimeline(MatchTimelineDto item, CultureInfo culture)
        {
            if (item?.BasicEvents == null)
            {
                return;
            }

            var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(item.SportEvent.Id.ToString()));

            var matchCacheItem = cacheItem as MatchCacheItem;
            matchCacheItem?.MergeTimeline(item, culture, true);
        }

        private void AddNewCacheItem(SportEventCacheItem item)
        {
            //TODO: maybe check if already present
            Cache.Add(item.Id.ToString(), item);
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableBase"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableBase>> ExportAsync()
        {
            LockManager.Wait();
            var exportables = Cache.GetValues().Select(i => (IExportableBase)i);
            LockManager.Release();

            var tasks = exportables.Select(e => e.ExportAsync());

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Imports provided items into the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableBase"/> to be inserted into the cache</param>
        public Task ImportAsync(IEnumerable<ExportableBase> items)
        {
            LockManager.Wait();
            try
            {
                foreach (var exportable in items)
                {
                    var sportEventCacheItem = _sportEventCacheItemFactory.Build(exportable);
                    if (sportEventCacheItem != null)
                    {
                        AddNewCacheItem(sportEventCacheItem);
                    }
                }
            }
            catch (Exception e)
            {
                ExecutionLog.LogError(e, "Error importing items");
            }
            finally
            {
                LockManager.Release();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{K, V}"/> containing all cache item types in the cache and their counts</returns>
        public IReadOnlyDictionary<string, int> CacheStatus()
        {
            var types = new[]
                        {
                            typeof(SportEventCacheItem), typeof(CompetitionCacheItem), typeof(DrawCacheItem), typeof(LotteryCacheItem), typeof(TournamentInfoCacheItem), typeof(MatchCacheItem), typeof(StageCacheItem)
                        };

            IReadOnlyCollection<object> items;
            LockManager.Wait();
            items = Cache.GetValues();
            LockManager.Release();

            var status = items.GroupBy(i => i.GetType().Name).ToDictionary(g => g.Key, g => g.Count());

            foreach (var type in types.Select(t => t.Name))
            {
                if (!status.ContainsKey(type))
                {
                    status[type] = 0;
                }
            }
            return status;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var keys = Cache.GetKeys();
            var details =
                $" [Match: {keys.Count(c => c.Contains("match")).ToString()}, Stage: {keys.Count(c => c.Contains("race") || c.Contains("stage")).ToString()}, Season: {keys.Count(c => c.Contains("season")).ToString()}, Tournament: {keys.Count(c => c.Contains("tournament")).ToString()}, Draw: {keys.Count(c => c.Contains("draw")).ToString()}, Lottery: {keys.Count(c => c.Contains("lottery")).ToString()}]";
            return Task.FromResult(HealthCheckResult.Healthy($"Cache has {Cache.Count().ToString()} items{details}"));
        }
    }
}
