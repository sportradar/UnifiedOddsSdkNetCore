// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Class LotteryCacheItem
    /// </summary>
    /// <seealso cref="SportEventCacheItem" />
    /// <seealso cref="ILotteryCacheItem" />
    internal class LotteryCacheItem : SportEventCacheItem, ILotteryCacheItem
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> id of the category
        /// </summary>
        private Urn _categoryId;

        /// <summary>
        /// Gets the bonus info
        /// </summary>
        private BonusInfoCacheItem _bonusInfo;

        /// <summary>
        /// Gets the draw info
        /// </summary>
        private DrawInfoCacheItem _drawInfo;

        /// <summary>
        /// Gets the scheduled draws
        /// </summary>
        /// <value>The scheduled draws</value>
        private IEnumerable<Urn> _scheduledDraws;

        private bool _scheduleFetched;
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteryCacheItem"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public LotteryCacheItem(Urn id,
                         IDataRouterManager dataRouterManager,
                         ISemaphorePool semaphorePool,
                         CultureInfo defaultCulture,
                         ICacheStore<string> fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _scheduleFetched = false;
            _dataRouterManager = dataRouterManager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteryCacheItem"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public LotteryCacheItem(LotteryDto eventSummary,
                         IDataRouterManager dataRouterManager,
                         ISemaphorePool semaphorePool,
                         CultureInfo currentCulture,
                         CultureInfo defaultCulture,
                         ICacheStore<string> fixtureTimestampCacheStore)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCacheStore)
        {
            Guard.Argument(eventSummary, nameof(eventSummary)).NotNull();
            Guard.Argument(currentCulture, nameof(currentCulture)).NotNull();

            _scheduleFetched = false;
            _dataRouterManager = dataRouterManager;

            Merge(eventSummary, currentCulture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteryCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableLottery" /> specifying the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public LotteryCacheItem(ExportableLottery exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ICacheStore<string> fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _scheduleFetched = false;
            _dataRouterManager = dataRouterManager;

            _categoryId = Urn.Parse(exportable.CategoryId);
            _bonusInfo = exportable.BonusInfo != null ? new BonusInfoCacheItem(exportable.BonusInfo) : null;
            _drawInfo = exportable.DrawInfo != null ? new DrawInfoCacheItem(exportable.DrawInfo) : null;
            _scheduledDraws = exportable.ScheduledDraws?.Select(Urn.Parse).ToList();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> representing  the associated category id
        /// </summary>
        /// <returns>The id of the associated category</returns>
        public async Task<Urn> GetCategoryIdAsync()
        {
            if (_categoryId != null)
            {
                return _categoryId;
            }

            if (LoadedSummaries.Any())
            {
                return _categoryId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _categoryId;
        }

        /// <summary>
        /// Asynchronously gets <see cref="BonusInfoCacheItem"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<BonusInfoCacheItem> GetBonusInfoAsync()
        {
            if (_bonusInfo != null)
            {
                return _bonusInfo;
            }

            if (LoadedSummaries.Any())
            {
                return _bonusInfo;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _bonusInfo;
        }

        /// <summary>
        /// Asynchronously gets <see cref="DrawInfoCacheItem"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<DrawInfoCacheItem> GetDrawInfoAsync()
        {
            if (_drawInfo != null)
            {
                return _drawInfo;
            }

            if (LoadedSummaries.Any())
            {
                return _drawInfo;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _drawInfo;
        }

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="IDrawCacheItem"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IEnumerable<Urn>> GetScheduledDrawsAsync()
        {
            if (_scheduleFetched || (_scheduledDraws != null && _scheduledDraws.Any()))
            {
                return _scheduledDraws;
            }
            await _dataRouterManager.GetLotteryScheduleAsync(Id, DefaultCulture, null).ConfigureAwait(false);
            _scheduleFetched = true;
            return _scheduledDraws;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(LotteryDto eventSummary, CultureInfo culture, bool useLock)
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
        private void Merge(LotteryDto eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (_categoryId == null && eventSummary.Category != null)
            {
                _categoryId = eventSummary.Category.Id;
            }
            if (_bonusInfo == null && eventSummary.BonusInfo != null)
            {
                _bonusInfo = new BonusInfoCacheItem(eventSummary.BonusInfo);
            }
            if (_drawInfo == null && eventSummary.DrawInfo != null)
            {
                _drawInfo = new DrawInfoCacheItem(eventSummary.DrawInfo);
            }
            if (eventSummary.DrawEvents != null && eventSummary.DrawEvents.Any())
            {
                _scheduledDraws = eventSummary.DrawEvents.Select(s => s.Id);
            }
        }

        protected override async Task<T> CreateExportableBaseAsync<T>()
        {
            var exportable = await base.CreateExportableBaseAsync<T>();
            if (exportable is ExportableLottery lottery)
            {
                lottery.CategoryId = _categoryId.ToString();
                lottery.BonusInfo = _bonusInfo != null ? await _bonusInfo.ExportAsync().ConfigureAwait(false) : null;
                lottery.DrawInfo = _drawInfo != null ? await _drawInfo.ExportAsync().ConfigureAwait(false) : null;
                lottery.ScheduledDraws = _scheduledDraws?.Select(s => s.ToString()).ToList();
            }
            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public override async Task<ExportableBase> ExportAsync()
        {
            return await CreateExportableBaseAsync<ExportableLottery>().ConfigureAwait(false);
        }
    }
}
