/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class LotteryCI
    /// </summary>
    /// <seealso cref="SportEventCI" />
    /// <seealso cref="ILotteryCI" />
    internal class LotteryCI : SportEventCI, ILotteryCI
    {
        /// <summary>
        /// Gets the <see cref="URN"/> id of the category
        /// </summary>
        private URN _categoryId;

        /// <summary>
        /// Gets the bonus info
        /// </summary>
        private BonusInfoCI _bonusInfo;

        /// <summary>
        /// Gets the draw info
        /// </summary>
        private DrawInfoCI _drawInfo;

        /// <summary>
        /// Gets the scheduled draws
        /// </summary>
        /// <value>The the scheduled draws</value>
        private IEnumerable<URN> _scheduledDraws;

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteryCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public LotteryCI(URN id,
                         IDataRouterManager dataRouterManager,
                         ISemaphorePool semaphorePool,
                         CultureInfo defaultCulture,
                         ObjectCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteryCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public LotteryCI(LotteryDTO eventSummary,
                         IDataRouterManager dataRouterManager,
                         ISemaphorePool semaphorePool,
                         CultureInfo currentCulture,
                         CultureInfo defaultCulture,
                         ObjectCache fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Contract.Requires(eventSummary != null);
            Contract.Requires(currentCulture != null);

            Merge(eventSummary, currentCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteryCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableLotteryCI" /> specifying the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public LotteryCI(ExportableLotteryCI exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ObjectCache fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _categoryId = URN.Parse(exportable.CategoryId);
            _bonusInfo = exportable.BonusInfo != null ? new BonusInfoCI(exportable.BonusInfo) : null;
            _drawInfo = exportable.DrawInfo != null ? new DrawInfoCI(exportable.DrawInfo) : null;
            _scheduledDraws = exportable.ScheduledDraws?.Select(URN.Parse).ToList();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> representing  the associated category id
        /// </summary>
        /// <returns>The id of the associated category</returns>
        public async Task<URN> GetCategoryIdAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _categoryId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _categoryId;
        }

        /// <summary>
        /// Asynchronously gets <see cref="BonusInfoCI"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<BonusInfoCI> GetBonusInfoAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _bonusInfo;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _bonusInfo;
        }

        /// <summary>
        /// Asynchronously gets <see cref="DrawInfoCI"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<DrawInfoCI> GetDrawInfoAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _drawInfo;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _drawInfo;
        }

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="IDrawCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IEnumerable<URN>> GetScheduledDrawsAsync()
        {
            if (LoadedSummaries.Any())
            {
                return _scheduledDraws;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _scheduledDraws;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(LotteryDTO eventSummary, CultureInfo culture, bool useLock)
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

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        private void Merge(LotteryDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (_categoryId == null && eventSummary.Category != null)
            {
                _categoryId = eventSummary.Category.Id;
            }
            if (_bonusInfo == null && eventSummary.BonusInfo != null)
            {
                _bonusInfo = new BonusInfoCI(eventSummary.BonusInfo);
            }
            if (_drawInfo == null && eventSummary.DrawInfo != null)
            {
                _drawInfo = new DrawInfoCI(eventSummary.DrawInfo);
            }
            if (eventSummary.DrawEvents != null && eventSummary.DrawEvents.Any())
            {
                _scheduledDraws = eventSummary.DrawEvents.Select(s => s.Id);
            }
        }

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>();
            var lottery = exportable as ExportableLotteryCI;

            lottery.CategoryId = _categoryId.ToString();
            lottery.BonusInfo = _bonusInfo != null ? await _bonusInfo.ExportAsync().ConfigureAwait(false) : null;
            lottery.DrawInfo = _drawInfo != null ? await _drawInfo.ExportAsync().ConfigureAwait(false) : null;
            lottery.ScheduledDraws = _scheduledDraws?.Select(s => s.ToString()).ToList();

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableLotteryCI>().ConfigureAwait(false);
    }
}
