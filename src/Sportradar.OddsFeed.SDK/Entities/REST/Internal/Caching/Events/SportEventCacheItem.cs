// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Base class of the cache item for sport event
    /// </summary>
    /// <seealso cref="SportEventCacheItem" />
    internal class SportEventCacheItem : ISportEventCacheItem, IExportableBase
    {
        /// <summary>
        /// The <see cref="ILogger" /> instance used for execution logging
        /// </summary>
        internal readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(SportEventCacheItem));

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain sport event summary and fixture
        /// </summary>
        protected readonly IDataRouterManager DataRouterManager;

        /// <summary>
        /// A <see cref="ISemaphorePool" /> instance used to obtain <see cref="SemaphoreSlim" /> instances used to sync async calls
        /// </summary>
        protected readonly ISemaphorePool SemaphorePool;

        /// <summary>
        /// A <see cref="object" /> instance used for thread synchronization
        /// </summary>
        protected readonly object MergeLock = new object();

        /// <summary>
        /// A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)
        /// </summary>
        protected readonly CultureInfo DefaultCulture;

        /// <summary>
        /// The names
        /// </summary>
        protected readonly Dictionary<CultureInfo, string> Names = new Dictionary<CultureInfo, string>();

        /// <summary>
        /// The sport identifier
        /// </summary>
        private Urn _sportId;

        /// <summary>
        /// The scheduled
        /// </summary>
        internal DateTime? Scheduled;

        /// <summary>
        /// The scheduled end
        /// </summary>
        internal DateTime? ScheduledEnd;

        /// <summary>
        /// The start time to be determined
        /// </summary>
        private bool? _startTimeTbd;

        /// <summary>
        /// The replaced by
        /// </summary>
        private Urn _replacedBy;

        /// <summary>
        /// The loaded fixtures
        /// </summary>
        internal readonly List<CultureInfo> LoadedFixtures = new List<CultureInfo>();

        /// <summary>
        /// The loaded summaries
        /// </summary>
        internal readonly List<CultureInfo> LoadedSummaries = new List<CultureInfo>();

        /// <summary>
        /// A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps
        /// </summary>
        protected readonly ICacheStore<string> FixtureTimestampCacheStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCacheItem" /> class
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public SportEventCacheItem(Urn id,
                            IDataRouterManager dataRouterManager,
                            ISemaphorePool semaphorePool,
                            CultureInfo defaultCulture,
                            ICacheStore<string> fixtureTimestampCacheStore)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(defaultCulture, nameof(defaultCulture)).NotNull();
            Guard.Argument(semaphorePool, nameof(semaphorePool)).NotNull();
            Guard.Argument(fixtureTimestampCacheStore, nameof(fixtureTimestampCacheStore)).NotNull();

            Id = id;
            DataRouterManager = dataRouterManager;
            SemaphorePool = semaphorePool;
            DefaultCulture = defaultCulture;
            FixtureTimestampCacheStore = fixtureTimestampCacheStore;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCacheItem" /> class
        /// </summary>
        /// <param name="eventSummary">The sport event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="currentCulture">A <see cref="CultureInfo" /> of the <see cref="SportEventSummaryDto" /> instance</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public SportEventCacheItem(SportEventSummaryDto eventSummary,
                            IDataRouterManager dataRouterManager,
                            ISemaphorePool semaphorePool,
                            CultureInfo currentCulture,
                            CultureInfo defaultCulture,
                            ICacheStore<string> fixtureTimestampCacheStore)
            : this(eventSummary.Id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
            Guard.Argument(eventSummary, nameof(eventSummary)).NotNull();
            Guard.Argument(currentCulture, nameof(currentCulture)).NotNull();

            Merge(eventSummary, currentCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCacheItem" /> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSportEvent" /> representing the sport event</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public SportEventCacheItem(ExportableSportEvent exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ICacheStore<string> fixtureTimestampCacheStore)
            : this(Urn.Parse(exportable.Id), dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
            Names = new Dictionary<CultureInfo, string>(exportable.Names);
            _sportId = _sportId = exportable.SportId == null ? null : Urn.Parse(exportable.SportId);
            Scheduled = exportable.Scheduled;
            ScheduledEnd = exportable.ScheduledEnd;
            _startTimeTbd = exportable.StartTimeTbd;
            _replacedBy = exportable.ReplacedBy != null ? Urn.Parse(exportable.ReplacedBy) : null;
            LoadedFixtures = new List<CultureInfo>(exportable.LoadedFixtures ?? new List<CultureInfo>());
            LoadedSummaries = new List<CultureInfo>(exportable.LoadedSummaries ?? new List<CultureInfo>());
        }

        /// <summary>
        /// Gets a <see cref="Urn" /> specifying the id of the sport event associated with the current instance
        /// </summary>
        /// <value>The identifier</value>
        public Urn Id { get; internal set; }

        /// <summary>
        /// Get names as an asynchronous operation
        /// </summary>
        /// <param name="cultures">The cultures</param>
        /// <returns>Return a name of the race, or match</returns>
        public async Task<IReadOnlyDictionary<CultureInfo, string>> GetNamesAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
            if (HasTranslationsFor(cultureInfos))
            {
                return Names;
            }
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);
            return new ReadOnlyDictionary<CultureInfo, string>(Names);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> representing sport id
        /// </summary>
        /// <returns>The <see cref="Urn"/> of the sport this sport event belongs to</returns>
        public async Task<Urn> GetSportIdAsync()
        {
            if (_sportId != null)
            {
                return _sportId;
            }
            if (LoadedSummaries.Any())
            {
                return _sportId;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return _sportId;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="DateTime"/> instance specifying when the sport event associated with the current
        /// instance was scheduled, or a null reference if the time is not known.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> instance specifying when the sport event associated with the current
        /// instance was scheduled, or a null reference if the time is not known</returns>
        public async Task<DateTime?> GetScheduledAsync()
        {
            if (Scheduled != null && Scheduled > DateTime.MinValue)
            {
                return Scheduled;
            }
            if (LoadedSummaries.Any())
            {
                return Scheduled;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return Scheduled;
        }

        /// <summary>
        /// get scheduled end as an asynchronous operation.
        /// </summary>
        /// <returns>A <see cref="Task{DateTime}" /> representing the retrieval operation</returns>
        public async Task<DateTime?> GetScheduledEndAsync()
        {
            if (ScheduledEnd != null && ScheduledEnd > DateTime.MinValue)
            {
                return ScheduledEnd;
            }
            if (Scheduled != null && Scheduled > DateTime.MinValue) // if _schedule is loaded, then something was prefetched
            {
                return ScheduledEnd;
            }
            if (LoadedSummaries.Any())
            {
                return ScheduledEnd;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return ScheduledEnd;
        }

        /// <summary>
        /// Asynchronously gets a value specifying if the start time to be determined is set for the associated sport event
        /// </summary>
        /// <returns>A value specifying if the start time to be determined is set for the associated sport event</returns>
        public async Task<bool?> GetStartTimeTbdAsync()
        {
            if (_startTimeTbd != null)
            {
                return _startTimeTbd;
            }
            if (LoadedSummaries.Any())
            {
                return _startTimeTbd;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return _startTimeTbd;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> specifying the replacement sport event for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="Urn"/> specifying the replacement sport event for the associated sport event.</returns>
        public async Task<Urn> GetReplacedByAsync()
        {
            if (_replacedBy != null)
            {
                return _replacedBy;
            }
            if (LoadedSummaries.Any())
            {
                return _replacedBy;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return _replacedBy;
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false</returns>
        public bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => Names.ContainsKey(c));
        }

        /// <summary>
        /// Fetches sport event detail info for those of the specified languages which are not yet fetched
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <param name="requestOptions">Request options for fetching summaries</param>
        /// <param name="forceFetch">Should the cached data be ignored</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        public async Task FetchMissingSummary(IEnumerable<CultureInfo> cultures, RequestOptions requestOptions, bool forceFetch)
        {
            // to improve performance check if anything is missing without acquiring a lock
            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
            if (cultureInfos.IsNullOrEmpty())
            {
                throw new ArgumentException("Missing cultures");
            }
            var missingCultures = LanguageHelper.GetMissingCultures(cultureInfos, LoadedSummaries);
            if (!missingCultures.Any() && !forceFetch)
            {
                return;
            }

            using (new TelemetryTracker(UofSdkTelemetry.SportEventFetchMissingSummary))
            {
                var id = $"{Id}_Summary";
                SemaphoreSlim semaphore = null;
                Exception initialException = null;
                try
                {
                    // acquire the lock
                    semaphore = await SemaphorePool.AcquireAsync(id).ConfigureAwait(false);
                    await semaphore.WaitAsync().ConfigureAwait(false);

                    // make sure there is still some data missing and was not fetched while waiting to acquire the lock
                    missingCultures = forceFetch
                        ? cultureInfos.ToList()
                        : LanguageHelper.GetMissingCultures(cultureInfos, LoadedSummaries);
                    if (!missingCultures.Any())
                    {
                        return;
                    }

                    // fetch data for missing cultures
                    Dictionary<CultureInfo, Task> fetchTasks;
                    if (Id.TypeGroup == ResourceTypeGroup.Draw)
                    {
                        fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture,
                            missingCulture => DataRouterManager.GetDrawSummaryAsync(Id, missingCulture, this));
                    }
                    else if (Id.TypeGroup == ResourceTypeGroup.Lottery)
                    {
                        fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture,
                            missingCulture => DataRouterManager.GetLotteryScheduleAsync(Id, missingCulture, this));
                    }
                    else
                    {
                        fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture,
                            missingCulture => DataRouterManager.GetSportEventSummaryAsync(Id, missingCulture, this, requestOptions));
                    }

                    await Task.WhenAll(fetchTasks.Values).ConfigureAwait(false);

                    foreach (var culture in missingCultures)
                    {
                        if (!LoadedSummaries.Contains(culture))
                        {
                            LoadedSummaries.Add(culture);
                        }
                    }
                }
                catch (CommunicationException ce)
                {
                    if (this is TournamentInfoCacheItem &&
                        ce.Message.Contains("NotFound")) // especially for tournaments that do not have summary
                    {
                        LoadedSummaries.AddRange(missingCultures);
                        ExecutionLog.LogWarning("Fetching summary for eventId={EventId} for languages [{Languages}] COMPLETED WITH NOT_FOUND", Id, string.Join(",", missingCultures));
                    }
                    else
                    {
                        ExecutionLog.LogWarning("Fetching summary for eventId={EventId} for languages [{Languages}] COMPLETED WITH ERROR", Id, string.Join(",", missingCultures));
                        initialException = ce;
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Fetching summary for eventId={EventId} for languages [{Languages}] COMPLETED WITH EX", Id, string.Join(",", missingCultures));
                    initialException = ex;
                }
                finally
                {
                    // release semaphore
                    // ReSharper disable once PossibleNullReferenceException
                    semaphore.ReleaseSafe();
                    SemaphorePool.Release(id);
                }

                if (initialException != null && DataRouterManager.ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw initialException;
                }
            }
        }

        /// <summary>
        /// Fetches sport event detail info for those of the specified languages which are not yet fetched
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <param name="forceFetch">Should the cached data be ignored</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        protected async Task FetchMissingSummary(IEnumerable<CultureInfo> cultures, bool forceFetch)
        {
            await FetchMissingSummary(cultures, GetTimeCriticalRequestOptions(), forceFetch).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches fixture info for those of the specified languages which are not yet fetched
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        protected async Task FetchMissingFixtures(IEnumerable<CultureInfo> cultures)
        {
            // to improve performance, check if anything is missing without acquiring a lock
            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
            if (cultureInfos.IsNullOrEmpty())
            {
                throw new ArgumentException("Missing cultures");
            }
            var missingCultures = LanguageHelper.GetMissingCultures(cultureInfos, LoadedFixtures);
            if (!missingCultures.Any())
            {
                return;
            }

            using (new TelemetryTracker(UofSdkTelemetry.SportEventFetchMissingFixtures))
            {
                var id = $"{Id}_Fixture";
                SemaphoreSlim semaphore = null;
                Exception potentialException = null;
                try
                {
                    // acquire the lock
                    semaphore = await SemaphorePool.AcquireAsync(id).ConfigureAwait(false);
                    await semaphore.WaitAsync().ConfigureAwait(false);

                    // make sure there is still some data missing and was not fetched while waiting to acquire the lock
                    missingCultures = LanguageHelper.GetMissingCultures(cultureInfos, LoadedFixtures);
                    if (!missingCultures.Any())
                    {
                        return;
                    }

                    // fetch data for missing cultures
                    Dictionary<CultureInfo, Task> fetchTasks;
                    if (Id.TypeGroup == ResourceTypeGroup.Draw)
                    {
                        fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture,
                            missingCulture => DataRouterManager.GetDrawFixtureAsync(Id, missingCulture, this));
                    }
                    else
                    {
                        fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture, missingCulture =>
                            DataRouterManager.GetSportEventFixtureAsync(
                                Id,
                                missingCulture,
                                !FixtureTimestampCacheStore.GetKeys().Contains(Id.ToString()),
                                this));
                    }

                    await Task.WhenAll(fetchTasks.Values).ConfigureAwait(false);
                    LoadedFixtures.AddRange(missingCultures);
                }
                catch (CommunicationException ce)
                {
                    if (this is TournamentInfoCacheItem && ce.Message.Contains("NotFound"))
                    {
                        LoadedFixtures.AddRange(missingCultures);
                        ExecutionLog.LogWarning("Fetching fixtures for eventId={EventId} for languages [{Languages}] COMPLETED WITH NOT_FOUND", Id, string.Join(",", missingCultures));
                    }
                    else
                    {
                        ExecutionLog.LogWarning("Fetching fixtures for eventId={EventId} for languages [{Languages}] COMPLETED WITH ERROR", Id, string.Join(",", missingCultures));
                        potentialException = ce;
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "Fetching fixtures for eventId={EventId} for languages [{Languages}] COMPLETED WITH EX", Id, string.Join(",", missingCultures));
                    potentialException = ex;
                }
                finally
                {
                    // release semaphore
                    // ReSharper disable once PossibleNullReferenceException
                    semaphore.ReleaseSafe();
                    SemaphorePool.Release(id);
                }

                if (potentialException != null && ((DataRouterManager)DataRouterManager).ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw potentialException;
                }
            }
        }

        /// <summary>
        /// Merge current instance with the data obtained via provider
        /// </summary>
        /// <param name="dto">A <see cref="SportEventSummaryDto" /> used to merge properties with current instance</param>
        /// <param name="culture">A language code of the <see cref="SportEventSummaryDto" /> data</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(SportEventSummaryDto dto, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    Merge(dto, culture);
                }
            }
            else
            {
                Merge(dto, culture);
            }
        }

        private void Merge(SportEventSummaryDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            lock (MergeLock)
            {
                Names[culture] = dto.Name;
                if (dto.SportId != null)
                {
                    _sportId = dto.SportId;
                }
                if (dto.Scheduled != null)
                {
                    Scheduled = dto.Scheduled;
                }
                if (dto.ScheduledEnd != null)
                {
                    ScheduledEnd = dto.ScheduledEnd;
                }
                if (dto.StartTimeTbd != null)
                {
                    _startTimeTbd = dto.StartTimeTbd;
                }
                if (dto.ReplacedBy != null)
                {
                    _replacedBy = dto.ReplacedBy;
                }
            }
        }

        /// <summary>
        /// Create exportable cache items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Exportable cache items</returns>
        protected virtual Task<T> CreateExportableBaseAsync<T>() where T : ExportableSportEvent, new()
        {
            var exportable = new T
            {
                Id = Id.ToString(),
                Names = new Dictionary<CultureInfo, string>(Names),
                SportId = _sportId?.ToString(),
                Scheduled = Scheduled,
                ScheduledEnd = ScheduledEnd,
                StartTimeTbd = _startTimeTbd,
                ReplacedBy = _replacedBy?.ToString(),
                LoadedFixtures = new List<CultureInfo>(LoadedFixtures ?? new List<CultureInfo>()),
                LoadedSummaries = new List<CultureInfo>(LoadedSummaries ?? new List<CultureInfo>())
            };

            return Task.FromResult(exportable);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public virtual async Task<ExportableBase> ExportAsync()
        {
            return await CreateExportableBaseAsync<ExportableSportEvent>().ConfigureAwait(false);
        }

        private static RequestOptions GetTimeCriticalRequestOptions()
        {
            return new RequestOptions(ExecutionPath.TimeCritical);
        }
    }
}
