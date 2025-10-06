// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Class Lottery
    /// </summary>
    /// <seealso cref="SportEvent" />
    /// <seealso cref="ILottery" />
    internal class Lottery : SportEvent, ILottery
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Lottery));

        /// <summary>
        /// A <see cref="ISportDataCache"/> instance used to retrieve basic information about the tournament(sport, category, names)
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Draw"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> uniquely identifying the sport event associated with the current instance</param>
        /// <param name="sportId">A <see cref="Urn"/> uniquely identifying the sport associated with the current instance</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCacheItem"/></param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve basic tournament information</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the initialized instance will handle potential exceptions</param>
        public Lottery(Urn id,
                       Urn sportId,
                       ISportEventCache sportEventCache,
                       ISportDataCache sportDataCache,
                       IReadOnlyCollection<CultureInfo> cultures,
                       ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();

            _sportDataCache = sportDataCache;
        }

        /// <summary>
        /// Asynchronously get the <see cref="ISportSummary"/> instance representing the sport associated with the current instance
        /// </summary>
        /// <value>The <see cref="ISportSummary"/> instance representing the sport associated with the current instance</value>
        public async Task<ISportSummary> GetSportAsync()
        {
            var lotteryCacheItem = (LotteryCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var sportId = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                              ? await lotteryCacheItem.GetSportIdAsync().ConfigureAwait(false)
                              : await new Func<Task<Urn>>(lotteryCacheItem.GetSportIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SportId")).ConfigureAwait(false);

            if (sportId == null)
            {
                ExecutionLog.LogDebug("Missing data. No sportId for lottery cache item with id={SportEventId}", Id);
                return null;
            }
            var sportData = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                                ? await _sportDataCache.GetSportAsync(sportId, Cultures).ConfigureAwait(false)
                                : await new Func<Urn, IReadOnlyCollection<CultureInfo>, Task<SportData>>(_sportDataCache.GetSportAsync).SafeInvokeAsync(sportId, Cultures, ExecutionLog, GetFetchErrorMessage("SportData")).ConfigureAwait(false);

            return sportData == null
                       ? null
                       : new SportSummary(sportData.Id, sportData.Names);
        }

        /// <summary>
        /// Asynchronously gets the associated category
        /// </summary>
        /// <returns>The associated category</returns>
        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var lotteryCacheItem = (LotteryCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }

            var categoryId = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                                 ? await lotteryCacheItem.GetCategoryIdAsync().ConfigureAwait(false)
                                 : await new Func<Task<Urn>>(lotteryCacheItem.GetCategoryIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("CategoryId")).ConfigureAwait(false);
            if (categoryId == null)
            {
                ExecutionLog.LogDebug("Missing data. No categoryId for lottery cache item with id={SportEventId}", Id);
                return null;
            }
            var categoryData = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                                   ? await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false)
                                   : await new Func<Urn, IReadOnlyCollection<CultureInfo>, Task<CategoryData>>(_sportDataCache.GetCategoryAsync).SafeInvokeAsync(categoryId, Cultures, ExecutionLog, GetFetchErrorMessage("CategoryData"))
                                                                                                                                                .ConfigureAwait(false);

            return categoryData == null
                       ? null
                       : new CategorySummary(categoryData.Id, categoryData.Names, categoryData.CountryCode);
        }

        /// <summary>
        /// Asynchronously get the <see cref="ITournamentCoverage"/> instance representing the tournament coverage associated with the current instance
        /// </summary>
        /// <value>The <see cref="ITournamentCoverage"/> instance representing the tournament coverage associated with the current instance</value>
        public Task<ITournamentCoverage> GetTournamentCoverage()
        {
            return Task.FromResult(null as ITournamentCoverage);
        }

        /// <summary>
        /// Asynchronously gets <see cref="IBonusInfo"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IBonusInfo> GetBonusInfoAsync()
        {
            var lotteryCacheItem = (LotteryCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                           ? await lotteryCacheItem.GetBonusInfoAsync().ConfigureAwait(false)
                           : await new Func<Task<BonusInfoCacheItem>>(lotteryCacheItem.GetBonusInfoAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("BonusInfo")).ConfigureAwait(false);

            return item == null
                       ? null
                       : new BonusInfo(item);
        }

        /// <summary>
        /// Asynchronously gets <see cref="IDrawInfo"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDrawInfo> GetDrawInfoAsync()
        {
            var lotteryCacheItem = (LotteryCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                           ? await lotteryCacheItem.GetDrawInfoAsync().ConfigureAwait(false)
                           : await new Func<Task<DrawInfoCacheItem>>(lotteryCacheItem.GetDrawInfoAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("DrawInfo")).ConfigureAwait(false);

            return item == null
                       ? null
                       : new DrawInfo(item);
        }

        /// <summary>
        /// Asynchronously gets the list of ids of associated <see cref="IDraw"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IEnumerable<Urn>> GetScheduledDrawsAsync()
        {
            var lotteryCacheItem = (LotteryCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                           ? await lotteryCacheItem.GetScheduledDrawsAsync().ConfigureAwait(false)
                           : await new Func<Task<IEnumerable<Urn>>>(lotteryCacheItem.GetScheduledDrawsAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ScheduledDraws")).ConfigureAwait(false);

            return item;
        }

        /// <summary>
        /// Asynchronously gets the list of associated <see cref="IDraw"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IEnumerable<IDraw>> GetDrawsAsync()
        {
            var lotteryCacheItem = (LotteryCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                           ? await lotteryCacheItem.GetScheduledDrawsAsync().ConfigureAwait(false)
                           : await new Func<Task<IEnumerable<Urn>>>(lotteryCacheItem.GetScheduledDrawsAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ScheduledDraws")).ConfigureAwait(false);

            return item.Select(selector: s => new Draw(s, SportId, SportEventCache, Cultures.ToList(), ExceptionStrategy));
        }
    }
}
