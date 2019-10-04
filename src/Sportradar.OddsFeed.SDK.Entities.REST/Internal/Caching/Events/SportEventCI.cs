/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Base class of the cache item for sport event
    /// </summary>
    /// <seealso cref="SportEventCI" />
    public class SportEventCI : ISportEventCI, IExportableCI
    {
        /// <summary>
        /// The <see cref="ILog" /> instance used for execution logging
        /// </summary>
        internal readonly ILog ExecutionLog = SdkLoggerFactory.GetLogger(typeof(SportEventCI));

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
        private URN _sportId;

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
        private URN _replacedBy;

        /// <summary>
        /// The loaded fixtures
        /// </summary>
        internal readonly List<CultureInfo> LoadedFixtures = new List<CultureInfo>();

        /// <summary>
        /// The loaded summaries
        /// </summary>
        internal readonly List<CultureInfo> LoadedSummaries = new List<CultureInfo>();

        /// <summary>
        /// A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps
        /// </summary>
        protected readonly ObjectCache FixtureTimestampCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCI" /> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public SportEventCI(URN id,
                            IDataRouterManager dataRouterManager,
                            ISemaphorePool semaphorePool,
                            CultureInfo defaultCulture,
                            ObjectCache fixtureTimestampCache)
        {
            Contract.Requires(id != null);
            Contract.Requires(dataRouterManager != null);
            Contract.Requires(defaultCulture != null);
            Contract.Requires(semaphorePool != null);
            Contract.Requires(fixtureTimestampCache != null);

            Id = id;
            DataRouterManager = dataRouterManager;
            SemaphorePool = semaphorePool;
            DefaultCulture = defaultCulture;
            FixtureTimestampCache = fixtureTimestampCache;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCI" /> class
        /// </summary>
        /// <param name="eventSummary">The sport event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="currentCulture">A <see cref="CultureInfo" /> of the <see cref="SportEventSummaryDTO" /> instance</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public SportEventCI(SportEventSummaryDTO eventSummary,
                            IDataRouterManager dataRouterManager,
                            ISemaphorePool semaphorePool,
                            CultureInfo currentCulture,
                            CultureInfo defaultCulture,
                            ObjectCache fixtureTimestampCache)
            : this(eventSummary.Id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            Contract.Requires(eventSummary != null);
            Contract.Requires(currentCulture != null);

            Merge(eventSummary, currentCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCI" /> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSportEventCI" /> representing the the sport event</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public SportEventCI(ExportableSportEventCI exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ObjectCache fixtureTimestampCache)
            : this(URN.Parse(exportable.Id), dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            Names = new Dictionary<CultureInfo, string>(exportable.Name);
            _sportId = URN.Parse(exportable.SportId);
            Scheduled = exportable.Scheduled;
            ScheduledEnd = exportable.ScheduledEnd;
            _startTimeTbd = exportable.StartTimeTbd;
            _replacedBy = exportable.ReplacedBy != null ? URN.Parse(exportable.ReplacedBy) : null;
            LoadedFixtures = new List<CultureInfo>(exportable.LoadedFixtures ?? new List<CultureInfo>());
            LoadedSummaries = new List<CultureInfo>(exportable.LoadedSummaries ?? new List<CultureInfo>());
        }

        /// <summary>
        /// Gets a <see cref="URN" /> specifying the id of the sport event associated with the current instance
        /// </summary>
        /// <value>The identifier</value>
        public URN Id { get; internal set; }

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
        /// Asynchronously gets a <see cref="URN"/> representing sport id
        /// </summary>
        /// <returns>The <see cref="URN"/> of the sport this sport event belongs to</returns>
        public async Task<URN> GetSportIdAsync()
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
        /// Asynchronously gets a <see cref="URN"/> specifying the replacement sport event for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="URN"/> specifying the replacement sport event for the associated sport event.</returns>
        public async Task<URN> GetReplacedByAsync()
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
        /// <param name="forceFetch">Should the cached data be ignored</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        protected async Task FetchMissingSummary(IEnumerable<CultureInfo> cultures, bool forceFetch)
        {
            Contract.Requires(cultures != null);
            Contract.Requires(cultures.Any());

            // to improve performance check if anything is missing without acquiring a lock
            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
            var missingCultures = LanguageHelper.GetMissingCultures(cultureInfos, LoadedSummaries).ToList();
            if (!missingCultures.Any() && !forceFetch)
            {
                return;
            }

            var id = $"{Id}_Summary";
            SemaphoreSlim semaphore = null;
            try
            {
                // acquire the lock
                semaphore = await SemaphorePool.Acquire(id).ConfigureAwait(false);
                await semaphore.WaitAsync().ConfigureAwait(false);

                // make sure there is still some data missing and was not fetched while waiting to acquire the lock
                missingCultures = forceFetch ? cultureInfos.ToList() : LanguageHelper.GetMissingCultures(cultureInfos, LoadedSummaries).ToList();
                if (!missingCultures.Any())
                {
                    return;
                }

                // fetch data for missing cultures
                Dictionary<CultureInfo, Task> fetchTasks;
                if (Id.TypeGroup == ResourceTypeGroup.DRAW)
                {
                    fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture, missingCulture => DataRouterManager.GetDrawSummaryAsync(Id, missingCulture, this));
                }
                else if (Id.TypeGroup == ResourceTypeGroup.LOTTERY)
                {
                    fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture, missingCulture => DataRouterManager.GetLotteryScheduleAsync(Id, missingCulture, this));
                }
                else
                {
                    fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture, missingCulture => DataRouterManager.GetSportEventSummaryAsync(Id, missingCulture, this));
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
                if (ce.Message.Contains("NotFound")) // especially for tournaments that dont have summary
                {
                    LoadedSummaries.AddRange(missingCultures);
                    ExecutionLog.Warn($"Fetching summary for eventId={Id} for languages [{string.Join(",", missingCultures)}] COMPLETED WITH NOT_FOUND.");
                }
                else
                {
                    ExecutionLog.Warn($"Fetching summary for eventId={Id} for languages [{string.Join(",", missingCultures)}] COMPLETED WITH ERROR.");
                    if (((DataRouterManager)DataRouterManager).ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.Error($"Fetching summary for eventId={Id} for languages [{string.Join(",", missingCultures)}] COMPLETED WITH EX.", ex);
                var drm = DataRouterManager as DataRouterManager;
                if (drm != null)
                {
                    if (drm.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    {
                        throw;
                    }
                }
            }
            finally
            {
                // release semaphore
                // ReSharper disable once PossibleNullReferenceException
                semaphore.Release();
                SemaphorePool.Release(id);
            }
        }

        /// <summary>
        /// Fetches fixture info for those of the specified languages which are not yet fetched
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        protected async Task FetchMissingFixtures(IEnumerable<CultureInfo> cultures)
        {
            Contract.Requires(cultures != null);
            Contract.Requires(cultures.Any());

            // to improve performance check if anything is missing without acquiring a lock
            var cultureInfos = cultures as IList<CultureInfo> ?? cultures.ToList();
            var missingCultures = LanguageHelper.GetMissingCultures(cultureInfos, LoadedFixtures).ToList();
            if (!missingCultures.Any())
            {
                return;
            }

            var id = $"{Id}_Fixture";
            SemaphoreSlim semaphore = null;
            try
            {
                // acquire the lock
                semaphore = await SemaphorePool.Acquire(id).ConfigureAwait(false);
                await semaphore.WaitAsync().ConfigureAwait(false);

                // make sure there is still some data missing and was not fetched while waiting to acquire the lock
                missingCultures = LanguageHelper.GetMissingCultures(cultureInfos, LoadedFixtures).ToList();
                if (!missingCultures.Any())
                {
                    return;
                }

                // fetch data for missing cultures
                //ExecutionLog.Debug($"Fetching fixtures for eventId={Id} for languages [{string.Join(",", missingCultures)}].");
                Dictionary<CultureInfo, Task> fetchTasks;
                if (Id.TypeGroup == ResourceTypeGroup.DRAW)
                {
                    fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture, missingCulture => DataRouterManager.GetDrawFixtureAsync(Id, missingCulture, this));
                }
                else
                {
                    fetchTasks = missingCultures.ToDictionary(missingCulture => missingCulture, missingCulture => DataRouterManager.GetSportEventFixtureAsync(
                                                                                                                          Id,
                                                                                                                          missingCulture,
                                                                                                                          !FixtureTimestampCache.Contains(Id.ToString()),
                                                                                                                          this));
                }
                await Task.WhenAll(fetchTasks.Values).ConfigureAwait(false);
                LoadedFixtures.AddRange(missingCultures);
                //ExecutionLog.Debug($"Fetching fixtures for eventId={Id} for languages [{string.Join(",", missingCultures)}] COMPLETED.");
            }
            catch (CommunicationException ce)
            {
                if (ce.Message.Contains("NotFound"))
                {
                    LoadedFixtures.AddRange(missingCultures);
                    ExecutionLog.Warn($"Fetching fixtures for eventId={Id} for languages [{string.Join(",", missingCultures)}] COMPLETED WITH NOT_FOUND.");
                }
                else
                {
                    ExecutionLog.Warn($"Fetching fixtures for eventId={Id} for languages [{string.Join(",", missingCultures)}] COMPLETED WITH ERROR.");
                    if (((DataRouterManager)DataRouterManager).ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ExecutionLog.Error($"Fetching fixtures for eventId={Id} for languages [{string.Join(",", missingCultures)}] COMPLETED WITH EX.", ex);
                if (((DataRouterManager)DataRouterManager).ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }
            finally
            {
                // release semaphore
                // ReSharper disable once PossibleNullReferenceException
                semaphore.Release();
                SemaphorePool.Release(id);
            }
        }

        /// <summary>
        /// Merge current instance with the data obtained via provider
        /// </summary>
        /// <param name="eventSummary">A <see cref="SportEventSummaryDTO" /> used to merge properties with current instance</param>
        /// <param name="culture">A language code of the <see cref="SportEventSummaryDTO" /> data</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(SportEventSummaryDTO eventSummary, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    Merge(eventSummary, culture);
                }
            }
            else
            {
                Merge(eventSummary, culture);
            }
        }

        private void Merge(SportEventSummaryDTO eventSummary, CultureInfo culture)
        {
            Contract.Requires(eventSummary != null);
            Contract.Requires(culture != null);

            lock (MergeLock)
            {
                Names[culture] = eventSummary.Name;
                _sportId = eventSummary.SportId;
                Scheduled = eventSummary.Scheduled;
                ScheduledEnd = eventSummary.ScheduledEnd;
                _startTimeTbd = eventSummary.StartTimeTbd;
                _replacedBy = eventSummary.ReplacedBy;
            }
        }

        protected virtual Task<T> CreateExportableCIAsync<T>() where T : ExportableSportEventCI, new()
        {
            var exportable = new T
            {
                Id = Id.ToString(),
                Name = new Dictionary<CultureInfo, string>(Names),
                SportId = _sportId.ToString(),
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
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public virtual async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableSportEventCI>().ConfigureAwait(false);
    }
}
