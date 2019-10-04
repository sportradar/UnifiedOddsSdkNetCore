/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class DrawCI
    /// </summary>
    /// <seealso cref="SportEventCI" />
    /// <seealso cref="ICompetitionCI" />
    internal class DrawCI : SportEventCI, IDrawCI
    {
        /// <summary>
        /// Gets the <see cref="URN"/> id of the lottery
        /// </summary>
        private URN _lotteryId;

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
        private IEnumerable<DrawResultCI> _results;

        /// <summary>
        /// The display identifier
        /// </summary>
        private int? _displayId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public DrawCI(URN id,
                      IDataRouterManager dataRouterManager,
                      ISemaphorePool semaphorePool,
                      CultureInfo defaultCulture,
                      ObjectCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public DrawCI(DrawDTO eventSummary,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo currentCulture,
                             CultureInfo defaultCulture,
                             ObjectCache fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Contract.Requires(eventSummary != null);
            Contract.Requires(currentCulture != null);

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableDrawCI" /> specifying the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public DrawCI(ExportableDrawCI exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ObjectCache fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _lotteryId = URN.Parse(exportable.LotteryId);
            _drawStatus = exportable.DrawStatus;
            _resultsChronological = exportable.ResultsChronological;
            _results = exportable.Results?.Select(r => new DrawResultCI(r)).ToList();
            _displayId = exportable.DisplayId;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> representing id of the associated <see cref="ILotteryCI"/>
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        public async Task<URN> GetLotteryIdAsync()
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
        /// Asynchronously gets a bool value indicating if the results are in chronological order
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
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="DrawResultCI"/>
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IEnumerable<DrawResultCI>> GetResultsAsync(IEnumerable<CultureInfo> cultures)
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
            await FetchMissingSummary(new[] {DefaultCulture}, false).ConfigureAwait(false);
            return _displayId;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(DrawDTO eventSummary, CultureInfo culture, bool useLock)
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
        private void ActualMerge(DrawDTO eventSummary, CultureInfo culture)
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
                    _results = new List<DrawResultCI>(eventSummary.Results.Select(s => new DrawResultCI(s, culture)));
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
        private void MergeDrawResults(IEnumerable<DrawResultDTO> results, CultureInfo culture)
        {
            Contract.Requires(culture != null);

            if (results == null)
            {
                return;
            }

            var tempResults = _results == null
                ? new List<DrawResultCI>()
                : new List<DrawResultCI>(_results);

            foreach (var result in results)
            {
                var tempResult = tempResults.FirstOrDefault(c => c.Value.Equals(result.Value));
                if (tempResult == null)
                {
                    tempResults.Add(new DrawResultCI(result, culture));
                }
                else
                {
                    tempResult.Merge(result, culture);
                }
            }
            _results = new ReadOnlyCollection<DrawResultCI>(tempResults);
        }

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>();
            var draw = exportable as ExportableDrawCI;

            draw.LotteryId = _lotteryId?.ToString();
            draw.DrawStatus = _drawStatus;
            draw.ResultsChronological = _resultsChronological;
            var resultTasks = _results?.Select(async r => await r.ExportAsync().ConfigureAwait(false));
            draw.Results = resultTasks != null ? await Task.WhenAll(resultTasks) : null;
            draw.DisplayId = _displayId;

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableDrawCI>().ConfigureAwait(false);
    }
}
