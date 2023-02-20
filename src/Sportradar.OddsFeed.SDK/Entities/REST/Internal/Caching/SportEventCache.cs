/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Health;
using App.Metrics.Timer;
using Castle.Core.Internal;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using ITimer = Sportradar.OddsFeed.SDK.Common.Internal.ITimer;

// ReSharper disable InconsistentlySynchronizedField

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// A implementation of the interface <see cref="ISportEventCache"/>
    /// </summary>
    /// <seealso cref="ISportEventCache" />
    internal class SportEventCache : SdkCache, ISportEventCache
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        private static new readonly ILogger CacheLog = SdkLoggerFactory.GetLoggerForCache(typeof(SportEventCache));

        /// <summary>
        /// The list of dates already automatically loaded by the timer
        /// </summary>
        private readonly List<DateTime> _fetchedDates;

        /// <summary>
        /// A <see cref="MemoryCache"/> which will be used to cache the data
        /// </summary>
        internal readonly MemoryCache Cache;

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain data via REST request
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// >A instance of <see cref="ISportEventCacheItemFactory"/> used to create new <see cref="SportEventCI"/>
        /// </summary>
        private readonly ISportEventCacheItemFactory _sportEventCacheItemFactory;

        /// <summary>
        /// The list of all supported <see cref="CultureInfo"/>
        /// </summary>
        private readonly IEnumerable<CultureInfo> _cultures;

        /// <summary>
        /// A <see cref="ITimer"/> instance used to trigger periodic cache refresh-es
        /// </summary>
        private readonly ITimer _timer;

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
        internal readonly ConcurrentBag<URN> SpecialTournaments;

        internal LockManager LockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCache"/> class
        /// </summary>
        /// <param name="cache">The in-memory cache of sport events</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain data</param>
        /// <param name="sportEventCacheItemFactory">A instance of <see cref="ISportEventCacheItemFactory"/> used to create new <see cref="SportEventCI"/></param>
        /// <param name="timer">The timer</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        public SportEventCache(MemoryCache cache,
                               IDataRouterManager dataRouterManager,
                               ISportEventCacheItemFactory sportEventCacheItemFactory,
                               ITimer timer,
                               IEnumerable<CultureInfo> cultures,
                               ICacheManager cacheManager)
            : base(cacheManager)
        {
            Guard.Argument(cache, nameof(cache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(sportEventCacheItemFactory, nameof(sportEventCacheItemFactory)).NotNull();
            Guard.Argument(timer, nameof(timer)).NotNull();
            if (cultures == null || !cultures.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(cultures));
            }

            Cache = cache;
            _dataRouterManager = dataRouterManager;
            _sportEventCacheItemFactory = sportEventCacheItemFactory;
            _cultures = cultures as IReadOnlyList<CultureInfo>;

            _fetchedDates = new List<DateTime>();

            _isDisposed = false;

            SpecialTournaments = new ConcurrentBag<URN>();

            LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));

            _timer = timer;
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                await OnTimerElapsedAsync().ConfigureAwait(false);
                LockManager.Clean();
            });
        }

        /// <summary>
        /// Invoked when the internally used timer elapses
        /// </summary>
        private async Task OnTimerElapsedAsync()
        {
            //check what needs to be fetched, then go fetched by culture, (not by date)
            var datesToFetch = new List<DateTime>();

            await _timerSemaphoreSlim.WaitAsync().ConfigureAwait(false);

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
                return;
            }

            var culturesToFetch = _cultures.ToDictionary(ci => ci, ci => datesToFetch);

            var timerOptions = new TimerOptions { Context = "SportEventCache", Name = "GetAll", MeasurementUnit = Unit.Requests };
            using (SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(timerOptions))
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
                        ExecutionLog.LogWarning($"Periodic events schedule retrieval failed because the instance {ex.ObjectName} is being disposed.");
                    }
                    catch (TaskCanceledException)
                    {
                        ExecutionLog.LogWarning("Periodic events schedule retrieval failed because the instance is being disposed.");
                    }
                    catch (FeedSdkException ex)
                    {
                        ExecutionLog.LogWarning(ex, "An exception occurred while attempting to retrieve schedule.");
                    }
                    catch (AggregateException ex)
                    {
                        var baseException = ex.GetBaseException();
                        if (baseException.GetType() == typeof(ObjectDisposedException))
                        {
                            ExecutionLog.LogWarning($"Error happened during fetching schedule, because the instance {((ObjectDisposedException)baseException).ObjectName} is being disposed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ExecutionLog.LogWarning(ex, "An exception occurred while attempting to retrieve schedule.");
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

            CacheLog.LogInformation($"{fetchedItem.Count()} sport events retrieved for {date.ToShortDateString()} and locale '{culture.TwoLetterISOLanguageName}'.");
        }

        /// <summary>
        /// Gets a <see cref="SportEventCI"/> instance representing cached sport event data
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of the sport event which cached representation to return</param>
        /// <returns>a <see cref="SportEventCI"/> instance representing cached sport event data</returns>
        public SportEventCI GetEventCacheItem(URN id)
        {
            LockManager.Wait(id.ToString());
            try
            {
                var item = (SportEventCI)Cache.Get(id.ToString());
                if (item != null)
                {
                    return item;
                }

                item = _sportEventCacheItemFactory.Build(id);

                AddNewCacheItem(item);

                // if there are events for non-standard tournaments (tournaments not on All tournaments for all sports)
                if (item is TournamentInfoCI && !SpecialTournaments.Contains(item.Id))
                {
                    SpecialTournaments.Add(item.Id);
                }

                return item;
            }
            catch (Exception ex)
            {
                ExecutionLog.LogError(ex, $"Error getting cache item for id={id}");
            }
            finally
            {
                LockManager.Release(id.ToString());
            }
            return null;
        }

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{URN}"/> containing id's of sport events, which belong to the specified tournament
        /// </summary>
        /// <param name="tournamentId">A <see cref="URN"/> representing the tournament identifier</param>
        /// <param name="culture">The culture to fetch the data</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task<IEnumerable<Tuple<URN, URN>>> GetEventIdsAsync(URN tournamentId, CultureInfo culture)
        {
            var ci = culture ?? _cultures.FirstOrDefault();

            var schedule = await _dataRouterManager.GetSportEventsForTournamentAsync(tournamentId, ci, null).ConfigureAwait(false);

            return schedule;
        }

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{URN}"/> containing id's of sport events, which belong to the specified tournament
        /// </summary>
        /// <param name="tournamentId">A <see cref="URN"/> representing the tournament identifier</param>
        /// <param name="cultures">A list of <see cref="CultureInfo"/> used to fetch schedules</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task GetEventIdsAsync(URN tournamentId, IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures?.ToList() ?? _cultures.ToList();

            var tasks = wantedCultures.Select(s => _dataRouterManager.GetSportEventsForTournamentAsync(tournamentId, s, null));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{URN}"/> containing id's of sport events, which are scheduled for specified date
        /// </summary>
        /// <param name="date">The date for which to retrieve the schedule, or a null reference to get currently live events</param>
        /// <param name="culture">The culture to fetch the data</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task<IEnumerable<Tuple<URN, URN>>> GetEventIdsAsync(DateTime? date, CultureInfo culture)
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
        /// <param name="id">A <see cref="URN"/> representing the event</param>
        public void AddFixtureTimestamp(URN id)
        {
            var cache = _sportEventCacheItemFactory.GetFixtureTimestampCache();
            cache.Set(id.ToString(), id, DateTimeOffset.Now.AddMinutes(2));
        }

        /// <summary>
        /// Asynchronously gets a list of active <see cref="IEnumerable{TournamentInfoCI}"/>
        /// </summary>
        /// <remarks>Lists all <see cref="TournamentInfoCI"/> that are cached (once schedule is loaded)</remarks>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<TournamentInfoCI>> GetActiveTournamentsAsync(CultureInfo culture = null)
        {
            await OnTimerElapsedAsync().ConfigureAwait(false); // this is almost not needed anymore, since we call all tournaments for all sports :)
            var tourKeys = Cache.Select(s => s.Key).Where(w => w.Contains("tournament") || w.Contains("stage") || w.Contains("outright") || w.Contains("simple_tournament"));

            var tours = new List<TournamentInfoCI>();
            var errorBuilder = new StringBuilder();
            foreach (var key in tourKeys)
            {
                try
                {
                    tours.Add((TournamentInfoCI)_sportEventCacheItemFactory.Get(Cache.Get(key)));
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
            ExecutionLog.LogDebug($"Found {tours.Count} tournaments. Errors: {error}");

            return tours;
        }

        /// <summary>
        /// Deletes the sport events from cache which are scheduled before specific DateTime
        /// </summary>
        /// <param name="before">The scheduled DateTime used to delete sport events from cache</param>
        /// <returns>Number of deleted items</returns>
        public int DeleteSportEventsFromCache(DateTime before)
        {
            LockManager.Wait();
            try
            {
                var startCount = Cache.Count();
                foreach (var keyValuePair in Cache)
                {
                    try
                    {
                        var ci = (SportEventCI)keyValuePair.Value;
                        if (ci.Scheduled != null && ci.Scheduled < before)
                        {
                            Cache.Remove(keyValuePair.Key);
                        }
                        else if (ci.ScheduledEnd != null && ci.ScheduledEnd < before)
                        {
                            Cache.Remove(keyValuePair.Key);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                var endCount = Cache.Count();
                ExecutionLog.LogInformation($"Deleted {startCount - endCount} items from cache (before={before}).");
                return startCount - endCount;
            }
            catch (Exception e)
            {
                ExecutionLog.LogError(e, "Error during DeleteSportEventsFromCache.");
            }
            finally
            {
                LockManager.Release();
            }
            return 0;
        }

        /// <summary>
        /// Asynchronous gets a <see cref="URN"/> of event's sport id
        /// </summary>
        /// <param name="id">A <see cref="URN"/> representing the event identifier</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        public async Task<URN> GetEventSportIdAsync(URN id)
        {
            try
            {
                if (!Cache.Contains(id.ToString()))
                {
                    await _dataRouterManager.GetSportEventSummaryAsync(id, _cultures.First(), null).ConfigureAwait(false);
                }

                var item = (SportEventCI)Cache.Get(id.ToString());
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
                ExecutionLog.LogError(ex, $"Error getting cache item sportId for id={id}");
            }

            return null;
        }

        private void CacheItemRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            if (arguments?.CacheItem == null)
            {
                return;
            }

            if (arguments.RemovedReason != CacheEntryRemovedReason.CacheSpecificEviction && arguments.RemovedReason != CacheEntryRemovedReason.Removed)
            {
                CacheLog.LogDebug($"SportEventCI {arguments.CacheItem.Key} removed from cache. Reason={arguments.RemovedReason}.");
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> CacheAddDtoItemAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
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
                    var fixture = item as FixtureDTO;
                    if (fixture != null)
                    {
                        AddSportEvent(id, fixture, culture, requester, dtoType);
                        saved = true;
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
                    if (match != null)
                    {
                        AddSportEvent(id, match, culture, requester, dtoType);
                        if (match.Tournament != null)
                        {
                            var ti = new TournamentInfoDTO(match.Tournament);
                            AddSportEvent(ti.Id, ti, culture, requester, dtoType);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDTO), item.GetType());
                    }
                    break;
                case DtoType.MatchTimeline:
                    var timeline = item as MatchTimelineDTO;
                    if (timeline != null)
                    {
                        AddMatchTimeLine(timeline, culture, requester, dtoType);
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
                    var stage = item as StageDTO;
                    if (stage != null)
                    {
                        AddSportEvent(id, stage, culture, requester, dtoType);
                        if (stage.Tournament != null)
                        {
                            var ti = new TournamentInfoDTO(stage.Tournament);
                            AddSportEvent(ti.Id, ti, culture, requester, dtoType);
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
                        await SaveTournamentDataFromSportAsync(sport, culture).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportDTO), item.GetType());
                    }
                    break;
                case DtoType.SportList:
                    var sportEntityList = item as EntityList<SportDTO>;
                    if (sportEntityList != null)
                    {
                        var tasks = sportEntityList.Items.Select(s => SaveTournamentDataFromSportAsync(s, culture));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
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
                    var tourInfo = item as TournamentInfoDTO;
                    if (tourInfo != null)
                    {
                        await SaveTournamentDataToSportEventCacheAsync(tourInfo, tourInfo.CurrentSeason?.Id, culture).ConfigureAwait(false);
                        if (tourInfo.Season != null)
                        {
                            await SaveTournamentDataToSportEventCacheAsync(tourInfo, tourInfo.Season?.Id, culture).ConfigureAwait(false);
                        }
                        break;
                    }
                    var summary = item as SportEventSummaryDTO;
                    if (summary != null)
                    {
                        AddSportEvent(id, summary, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventSummaryDTO), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummaryList:
                    var summaryList = item as EntityList<SportEventSummaryDTO>;
                    if (summaryList != null && !summaryList.Items.IsNullOrEmpty())
                    {
                        var tasks = summaryList.Items.Select(s => SaveSportEventSummaryDTOAsync(s, culture, dtoType, requester));
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportEventSummaryDTO>), item.GetType());
                    }
                    break;
                case DtoType.Tournament:
                    var t = item as TournamentDTO;
                    if (t != null)
                    {
                        var ti = new TournamentInfoDTO(t);
                        AddSportEvent(id, ti, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentInfo:
                    var tour = item as TournamentInfoDTO;
                    if (tour != null)
                    {
                        AddSportEvent(id, tour, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentInfoDTO), item.GetType());
                    }
                    break;
                case DtoType.TournamentSeasons:
                    var tourSeasons = item as TournamentSeasonsDTO;
                    if (tourSeasons?.Tournament != null)
                    {
                        AddSportEvent(id, tourSeasons.Tournament, culture, requester, dtoType);
                        var cacheItem = (TournamentInfoCI)_sportEventCacheItemFactory.Get(Cache.Get(id.ToString()));
                        cacheItem?.Merge(tourSeasons, culture, true);

                        if (tourSeasons.Seasons != null && tourSeasons.Seasons.Any())
                        {
                            foreach (var season in tourSeasons.Seasons)
                            {
                                AddSportEvent(season.Id, new TournamentInfoDTO(season), culture, null, dtoType);
                            }
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(TournamentSeasonsDTO), item.GetType());
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
                        AddSportEvent(id, lottery, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(LotteryDTO), item.GetType());
                    }
                    break;
                case DtoType.LotteryDraw:
                    var draw = item as DrawDTO;
                    if (draw != null)
                    {
                        AddSportEvent(id, draw, culture, requester, dtoType);
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(DrawDTO), item.GetType());
                    }
                    break;
                case DtoType.LotteryList:
                    var lotteryList = item as EntityList<LotteryDTO>;
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
                        LogSavingDtoConflict(id, typeof(EntityList<LotteryDTO>), item.GetType());
                    }
                    break;
                case DtoType.BookingStatus:
                    if (Cache.Contains(id.ToString()))
                    {
                        var e = Cache.Get(id.ToString());
                        var comp = e as CompetitionCI;
                        comp?.Book();
                    }
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
                            AddSportEvent(id, t1, culture, requester, dtoType);
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<TournamentInfoDTO>), item.GetType());
                    }
                    break;
                case DtoType.PeriodSummary:
                    break;
                case DtoType.Calculation:
                    break;
                default:
                    ExecutionLog.LogWarning($"Trying to add unchecked dto type:{dtoType} for id: {id}.");
                    break;
            }
            return saved;
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
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
        /// <param name="id">A <see cref="URN"/> representing the id of the sport event which to be deleted</param>
        /// <param name="cacheItemType">The cache item type to be deleted</param>
        public override void CacheDeleteItem(URN id, CacheItemType cacheItemType)
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
        /// <param name="id">A <see cref="URN" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(URN id, CacheItemType cacheItemType)
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
        /// Registers the health check which will be periodically triggered
        /// </summary>
        public void RegisterHealthCheck()
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult"/>
        /// </summary>
        public HealthCheckResult StartHealthCheck()
        {
            var keys = Cache.Select(w => w.Key).ToList();
            var details = $" [Match: {keys.Count(c => c.Contains("match"))}, Stage: {keys.Count(c => c.Contains("race") || c.Contains("stage"))}, Season: {keys.Count(c => c.Contains("season"))}, Tournament: {keys.Count(c => c.Contains("tournament"))}, Draw: {keys.Count(c => c.Contains("draw"))}, Lottery: {keys.Count(c => c.Contains("lottery"))}]";
            return Cache.Any() ? HealthCheckResult.Healthy($"Cache has {Cache.Count()} items{details}.") : HealthCheckResult.Unhealthy("Cache is empty.");
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
                _timerSemaphoreSlim.ReleaseSafe();
                _timerSemaphoreSlim.Dispose();
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

        private async Task SaveSportEventSummaryDTOAsync(SportEventSummaryDTO sportEventSummaryDTO, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            var tourInfosDTO = sportEventSummaryDTO as TournamentInfoDTO;
            if (tourInfosDTO != null)
            {
                await SaveTournamentDataToSportEventCacheAsync(tourInfosDTO, tourInfosDTO.CurrentSeason?.Id, culture).ConfigureAwait(false);
                if (tourInfosDTO.Season != null)
                {
                    await SaveTournamentDataToSportEventCacheAsync(tourInfosDTO, tourInfosDTO.Season?.Id, culture).ConfigureAwait(false);
                }
                return;
            }
            AddSportEvent(sportEventSummaryDTO.Id, sportEventSummaryDTO, culture, requester, dtoType);
        }

        private async Task SaveTournamentDataFromSportAsync(SportDTO sportDTO, CultureInfo culture)
        {
            if (sportDTO.Categories != null)
            {
                foreach (var categoryData in sportDTO.Categories)
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

        private async Task SaveTournamentDataToSportEventCacheAsync(TournamentDTO tournamentDto, URN secondId, CultureInfo culture)
        {
            await CacheAddDtoItemAsync(tournamentDto.Id, tournamentDto, culture, DtoType.Tournament, null).ConfigureAwait(false);

            if (secondId != null && !Equals(tournamentDto.Id, secondId))
            {
                var tourInfoDto = new TournamentInfoDTO(tournamentDto);
                var newTournamentDto = new TournamentInfoDTO(tourInfoDto, tourInfoDto.Season != null, tourInfoDto.CurrentSeason != null);
                await CacheAddDtoItemAsync(secondId, newTournamentDto, culture, DtoType.TournamentInfo, null).ConfigureAwait(false);
            }
        }

        private async Task SaveTournamentDataToSportEventCacheAsync(TournamentInfoDTO tournamentDto, URN secondId, CultureInfo culture)
        {
            await CacheAddDtoItemAsync(tournamentDto.Id, tournamentDto, culture, DtoType.TournamentInfo, null).ConfigureAwait(false);

            if (secondId != null && !Equals(tournamentDto.Id, secondId))
            {
                var newTournamentDto = new TournamentInfoDTO(tournamentDto, tournamentDto.Season != null, tournamentDto.CurrentSeason != null);
                await CacheAddDtoItemAsync(secondId, newTournamentDto, culture, DtoType.TournamentInfo, null).ConfigureAwait(false);
            }
        }

        private void SaveParentStage(URN parentId, StageDTO parentStage, TournamentDTO tournamentDto, CultureInfo culture)
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
                    var stageDtoFromTournament = new StageDTO(tournamentDto);
                    var ci2 = (StageCI)_sportEventCacheItemFactory.Build(stageDtoFromTournament, culture);
                    ci2.Merge(parentStage, culture, false);
                    AddNewCacheItem(ci2);
                }
                else
                {
                    var ci2 = (StageCI)_sportEventCacheItemFactory.Build(parentStage, culture);
                    AddNewCacheItem(ci2);
                }
            }
            else
            {
                if (cacheItem is StageCI stageCI)
                {
                    stageCI.Merge(parentStage, culture, false);
                }
                else
                {
                    // something wrong
                    var _ = cacheItem.GetType().Name;
                }
            }
        }

        private void SaveLotteryDraw(DrawDTO draw, CultureInfo culture)
        {
            if (draw == null)
            {
                return;
            }
            var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(draw.Id.ToString()));
            if (cacheItem == null)
            {
                var ci2 = (DrawCI)_sportEventCacheItemFactory.Build(draw, culture);
                AddNewCacheItem(ci2);
            }
            else
            {
                if (cacheItem is DrawCI drawCI)
                {
                    drawCI.Merge(draw, culture, false);
                }
                else
                {
                    //ignored
                }
            }
        }

        private void AddSportEvent(URN id, SportEventSummaryDTO item, CultureInfo culture, ISportEventCI requester, DtoType dtoType)
        {
            TournamentInfoDTO tournamentInfoDTO = null;
            LockManager.Wait(id.ToString());
            try
            {
                var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(id.ToString()));

                if (requester != null && !Equals(requester, cacheItem) && id.Equals(requester.Id))
                {
                    try
                    {
                        var requesterMerged = false;
                        var fixture = item as FixtureDTO;
                        if (fixture != null)
                        {
                            if (requester.Id.TypeGroup == ResourceTypeGroup.MATCH)
                            {
                                ((MatchCI)requester).MergeFixture(fixture, culture, true);
                            }
                            else if (requester.Id.TypeGroup == ResourceTypeGroup.STAGE)
                            {
                                var stageCI = (StageCI)requester;
                                stageCI.MergeFixture(fixture, culture, true);
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
                                ((TournamentInfoCI)requester).MergeFixture(fixture, culture, true);
                            }

                            requesterMerged = true;
                        }

                        if (!requesterMerged)
                        {
                            var match = item as MatchDTO;
                            if (match != null)
                            {
                                ((MatchCI)requester).Merge(match, culture, true);
                                requesterMerged = true;
                            }
                        }

                        if (!requesterMerged)
                        {
                            var stage = item as StageDTO;
                            if (stage != null)
                            {
                                ((StageCI)requester).Merge(stage, culture, true);
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
                            var tour = item as TournamentInfoDTO;
                            if (tour != null)
                            {
                                var stageCI = requester as StageCI;
                                if (stageCI != null)
                                {
                                    stageCI.Merge(tour, culture, true);
                                    requesterMerged = true;
                                }
                                else
                                {
                                    var tourCI = requester as TournamentInfoCI;
                                    if (tourCI != null)
                                    {
                                        tourCI.Merge(tour, culture, true);
                                        requesterMerged = true;
                                    }
                                }
                            }
                        }

                        if (!requesterMerged)
                        {
                            var draw = item as DrawDTO;
                            if (draw != null)
                            {
                                ((DrawCI)requester).Merge(draw, culture, true);
                                requesterMerged = true;
                            }
                        }

                        if (!requesterMerged)
                        {
                            var lottery = item as LotteryDTO;
                            if (lottery != null)
                            {
                                ((LotteryCI)requester).Merge(lottery, culture, true);
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
                        ExecutionLog.LogDebug($"Merging failed for {id} and item type: {item.GetType().Name} and dto type: {dtoType} for requester: {requester.Id}.");
                    }
                }

                if (cacheItem != null)
                {
                    //ExecutionLog.LogDebug($"Saving OLD data for {id} and item type: {item.GetType().Name} and dto type: {dtoType}.");
                    var merged = false;
                    //var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(id.ToString()));
                    var fixture = item as FixtureDTO;
                    if (fixture != null)
                    {
                        if (cacheItem.Id.TypeGroup == ResourceTypeGroup.MATCH)
                        {
                            ((MatchCI)cacheItem).MergeFixture(fixture, culture, true);
                        }
                        else if (cacheItem.Id.TypeGroup == ResourceTypeGroup.STAGE)
                        {
                            ((StageCI)cacheItem).MergeFixture(fixture, culture, true);
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
                            ((TournamentInfoCI)cacheItem).MergeFixture(fixture, culture, true);
                        }

                        if (fixture.Tournament != null)
                        {
                            tournamentInfoDTO = new TournamentInfoDTO(fixture.Tournament);
                        }

                        merged = true;
                    }

                    if (!merged)
                    {
                        var stage = item as StageDTO;
                        if (stage != null)
                        {
                            ((StageCI)cacheItem).Merge(stage, culture, true);
                            merged = true;
                            if (stage.Tournament != null)
                            {
                                tournamentInfoDTO = new TournamentInfoDTO(stage.Tournament);
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
                        var tour = item as TournamentInfoDTO;
                        if (tour != null)
                        {
                            var stageCI = cacheItem as StageCI;
                            if (stageCI != null)
                            {
                                stageCI.Merge(tour, culture, true);
                                merged = true;
                            }
                            else
                            {
                                var tourCI = cacheItem as TournamentInfoCI;
                                if (tourCI != null)
                                {
                                    tourCI.Merge(tour, culture, true);
                                    merged = true;
                                }
                            }
                        }
                    }

                    if (!merged)
                    {
                        var match = item as MatchDTO;
                        if (match != null)
                        {
                            ((MatchCI)cacheItem).Merge(match, culture, true);
                            merged = true;
                            if (match.Tournament != null)
                            {
                                tournamentInfoDTO = new TournamentInfoDTO(match.Tournament);
                            }
                        }
                    }

                    if (!merged)
                    {
                        var draw = item as DrawDTO;
                        if (draw != null)
                        {
                            ((DrawCI)cacheItem).Merge(draw, culture, true);
                            merged = true;
                        }
                    }

                    if (!merged)
                    {
                        var lottery = item as LotteryDTO;
                        if (lottery != null)
                        {
                            ((LotteryCI)cacheItem).Merge(lottery, culture, true);
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
                        var tInfo = item as TournamentInfoDTO;
                        if (tInfo != null)
                        {
                            var newTournamentDto = new TournamentInfoDTO(tInfo, tInfo.Season != null, tInfo.CurrentSeason != null);
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
                ExecutionLog.LogError(ex, $"Error adding sport event for id={id}, dto type={item?.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
            }
            finally
            {
                LockManager.Release(id.ToString());
            }

            // if there are events for non-standard tournaments (tournaments not on All tournaments for all sports)
            // TODO: should we save/merge all, or just adding non-existing???
            if (tournamentInfoDTO != null && (SpecialTournaments.Contains(tournamentInfoDTO.Id) || !Cache.Contains(tournamentInfoDTO.Id.ToString())))
            {
                if (SpecialTournaments.Contains(tournamentInfoDTO.Id))
                {
                }
                else
                {
                    SpecialTournaments.Add(tournamentInfoDTO.Id);
                }
                AddSportEvent(tournamentInfoDTO.Id, tournamentInfoDTO, culture, null, dtoType);
            }
        }

        private void AddMatchTimeLine(MatchTimelineDTO item, CultureInfo culture, ISportEventCI requester, DtoType dtoType)
        {
            AddSportEvent(item.SportEvent.Id, item.SportEvent, culture, requester, dtoType);

            LockManager.Wait(item.SportEvent.Id.ToString());
            try
            {
                UpdateMatchWithTimeline(item, culture);
            }
            catch (Exception ex)
            {
                ExecutionLog.LogError(ex, $"Error adding timeline for id={item.SportEvent.Id}, dto type={item.GetType().Name} and lang={culture.TwoLetterISOLanguageName}.");
            }
            finally
            {
                LockManager.Release(item.SportEvent.Id.ToString());
            }
        }

        private void UpdateMatchWithTimeline(MatchTimelineDTO item, CultureInfo culture)
        {
            if (item?.BasicEvents == null)
            {
                return;
            }

            var cacheItem = _sportEventCacheItemFactory.Get(Cache.Get(item.SportEvent.Id.ToString()));

            var matchCI = cacheItem as MatchCI;
            matchCI?.MergeTimeline(item, culture, true);
        }

        private void AddNewCacheItem(SportEventCI item)
        {
            //TODO: maybe check if already present
            Cache.Add(item.Id.ToString(), item, new CacheItemPolicy { RemovedCallback = CacheItemRemovedCallback });
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableCI"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableCI>> ExportAsync()
        {
            IEnumerable<IExportableCI> exportables;
            LockManager.Wait();
            exportables = Cache.Select(i => (IExportableCI)i.Value);
            LockManager.Release();

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
#pragma warning disable 1998
        public async Task ImportAsync(IEnumerable<ExportableCI> items)
#pragma warning restore 1998
        {
            LockManager.Wait();
            try
            {
                foreach (var exportable in items)
                {
                    var sportEventCI = _sportEventCacheItemFactory.Build(exportable);
                    if (sportEventCI != null)
                    {
                        AddNewCacheItem(sportEventCI);
                    }
                }
            }
            catch (Exception e)
            {
                ExecutionLog.LogError(e, "Error importing items.");
            }
            finally
            {
                LockManager.Release();
            }
        }

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{K, V}"/> containing all cache item types in the cache and their counts</returns>
        public IReadOnlyDictionary<string, int> CacheStatus()
        {
            var types = new[] {
                typeof(SportEventCI), typeof(CompetitionCI), typeof(DrawCI), typeof(LotteryCI),
                typeof(TournamentInfoCI), typeof(MatchCI), typeof(StageCI)
            };

            List<KeyValuePair<string, object>> items;
            LockManager.Wait();
            items = Cache.ToList();
            LockManager.Release();

            var status = items.GroupBy(i => i.Value.GetType().Name).ToDictionary(g => g.Key, g => g.Count());

            foreach (var type in types.Select(t => t.Name))
            {
                if (!status.ContainsKey(type))
                {
                    status[type] = 0;
                }
            }
            return status;
        }
    }
}
