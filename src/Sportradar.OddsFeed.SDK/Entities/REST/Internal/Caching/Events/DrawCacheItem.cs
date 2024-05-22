// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Class DrawCacheItem
    /// </summary>
    /// <seealso cref="SportEventCacheItem" />
    /// <seealso cref="ICompetitionCacheItem" />
    internal class DrawCacheItem : SportEventCacheItem, IDrawCacheItem
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> id of the lottery
        /// </summary>
        private Urn _lotteryId;

        /// <summary>
        /// Gets the status of the draw
        /// </summary>
        private DrawStatus _drawStatus;

        /// <summary>
        /// Gets a value indicating whether results are in chronological order
        /// </summary>
        /// <value><c>true</c> if [results chronological]; otherwise, <c>false</c></value>
        private bool _resultsChronological;

        /// <summary>
        /// Gets the results of the draws
        /// </summary>
        /// <value>The results of the draws</value>
        private IEnumerable<DrawResultCacheItem> _results;

        /// <summary>
        /// The display identifier
        /// </summary>
        private int? _displayId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCacheItem"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public DrawCacheItem(Urn id,
                      IDataRouterManager dataRouterManager,
                      ISemaphorePool semaphorePool,
                      CultureInfo defaultCulture,
                      ICacheStore<string> fixtureTimestampCacheStore)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCacheItem"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public DrawCacheItem(DrawDto eventSummary,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo currentCulture,
                             CultureInfo defaultCulture,
                             ICacheStore<string> fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Guard.Argument(eventSummary, nameof(eventSummary)).NotNull();
            Guard.Argument(currentCulture, nameof(currentCulture)).NotNull();

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableDraw" /> specifying the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public DrawCacheItem(ExportableDraw exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ICacheStore<string> fixtureTimestampCacheStore)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
            _lotteryId = Urn.Parse(exportable.LotteryId);
            _drawStatus = exportable.DrawStatus;
            _resultsChronological = exportable.ResultsChronological;
            _results = exportable.Results?.Select(r => new DrawResultCacheItem(r)).ToList();
            _displayId = exportable.DisplayId;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> representing id of the associated <see cref="ILotteryCacheItem"/>
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        public async Task<Urn> GetLotteryIdAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _lotteryId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _lotteryId;
        }

        /// <summary>
        /// Asynchronously gets <see cref="DrawStatus"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<DrawStatus> GetStatusAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _drawStatus;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _drawStatus;
        }

        /// <summary>
        /// Asynchronously gets a boolean value indicating if the results are in chronological order
        /// </summary>
        /// <returns>The value indicating if the results are in chronological order</returns>
        public async Task<bool> AreResultsChronologicalAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _resultsChronological;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _resultsChronological;
        }

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="DrawResultCacheItem"/>
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IEnumerable<DrawResultCacheItem>> GetResultsAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_results != null && !LanguageHelper.GetMissingCultures(cultureInfos, LoadedSummaries).Any())
            {
                return _results;
            }
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);
            return _results;
        }

        /// <summary>
        /// Gets the display identifier
        /// </summary>
        /// <value>The display identifier</value>
        public async Task<int?> GetDisplayIdAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _displayId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _displayId;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(DrawDto eventSummary, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMerge(eventSummary, culture);
                }
            }
            else
            {
                ActualMerge(eventSummary, culture);
            }
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        private void ActualMerge(DrawDto eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Lottery != null)
            {
                _lotteryId = eventSummary.Lottery.Id;
            }
            _drawStatus = eventSummary.Status;
            _resultsChronological = eventSummary.ResultsChronological;

            if (eventSummary.Results != null && eventSummary.Results.Any())
            {
                if (_results == null)
                {
                    _results = new List<DrawResultCacheItem>(eventSummary.Results.Select(s => new DrawResultCacheItem(s, culture)));
                }
                else
                {
                    MergeDrawResults(eventSummary.Results, culture);
                }
            }

            if (eventSummary.DisplayId != null)
            {
                _displayId = eventSummary.DisplayId;
            }
        }

        /// <summary>
        /// Merges the draw results
        /// </summary>
        /// <param name="results">The draw results</param>
        /// <param name="culture">The culture</param>
        private void MergeDrawResults(IEnumerable<DrawResultDto> results, CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (results == null)
            {
                return;
            }

            var tempResults = _results == null
                ? new List<DrawResultCacheItem>()
                : new List<DrawResultCacheItem>(_results);

            foreach (var result in results)
            {
                var tempResult = tempResults.FirstOrDefault(c => c.Value.Equals(result.Value));
                if (tempResult == null)
                {
                    tempResults.Add(new DrawResultCacheItem(result, culture));
                }
                else
                {
                    tempResult.Merge(result, culture);
                }
            }
            _results = new ReadOnlyCollection<DrawResultCacheItem>(tempResults);
        }

        protected override async Task<T> CreateExportableBaseAsync<T>()
        {
            var exportable = await base.CreateExportableBaseAsync<T>();
            if (exportable is ExportableDraw draw)
            {
                draw.LotteryId = _lotteryId?.ToString();
                draw.DrawStatus = _drawStatus;
                draw.ResultsChronological = _resultsChronological;
                var resultTasks = _results?.Select(async r => await r.ExportAsync().ConfigureAwait(false));
                draw.Results = resultTasks != null ? await Task.WhenAll(resultTasks) : null;
                draw.DisplayId = _displayId;
            }
            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public override async Task<ExportableBase> ExportAsync()
        {
            return await CreateExportableBaseAsync<ExportableDraw>().ConfigureAwait(false);
        }
    }
}
