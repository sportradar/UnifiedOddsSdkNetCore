/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

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
        /// A <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Lottery));

        /// <summary>
        /// A <see cref="ISportDataCache"/> instance used to retrieve basic information about the tournament(sport, category, names)
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Draw"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN"/> uniquely identifying the sport event associated with the current instance</param>
        /// <param name="sportId">A <see cref="URN"/> uniquely identifying the sport associated with the current instance</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCI"/></param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve basic tournament information</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the initialized instance will handle potential exceptions</param>
        public Lottery(URN id,
                        URN sportId,
                        ISportEventCache sportEventCache,
                        ISportDataCache sportDataCache,
                        IEnumerable<CultureInfo> cultures,
                        ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Contract.Requires(sportDataCache != null);

            _sportDataCache = sportDataCache;
        }

        /// <summary>
        /// Asynchronously get the <see cref="ISportSummary"/> instance representing the sport associated with the current instance
        /// </summary>
        /// <value>The <see cref="ISportSummary"/> instance representing the sport associated with the current instance</value>
        public async Task<ISportSummary> GetSportAsync()
        {
            var lotteryCI = (LotteryCI) SportEventCache.GetEventCacheItem(Id);
            if (lotteryCI == null)
            {
                ExecutionLog.Debug($"Missing data. No lottery cache item for id={Id}.");
                return null;
            }
            var sportId = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await lotteryCI.GetSportIdAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(lotteryCI.GetSportIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SportId")).ConfigureAwait(false);

            if (sportId == null)
            {
                ExecutionLog.Debug($"Missing data. No sportId for lottery cache item with id={Id}.");
                return null;
            }
            var sportData = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await _sportDataCache.GetSportAsync(sportId, Cultures).ConfigureAwait(false)
                : await new Func<URN, IEnumerable<CultureInfo>, Task<SportData>>(_sportDataCache.GetSportAsync).SafeInvokeAsync(sportId, Cultures, ExecutionLog, GetFetchErrorMessage("SportData")).ConfigureAwait(false);

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
            var lotteryCI = (LotteryCI)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCI == null)
            {
                ExecutionLog.Debug($"Missing data. No lottery cache item for id={Id}.");
                return null;
            }

            var categoryId = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await lotteryCI.GetCategoryIdAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(lotteryCI.GetCategoryIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("CategoryId")).ConfigureAwait(false);
            if (categoryId == null)
            {
                ExecutionLog.Debug($"Missing data. No categoryId for lottery cache item with id={Id}.");
                return null;
            }
            var categoryData = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false)
                : await new Func<URN, IEnumerable<CultureInfo>, Task<CategoryData>>(_sportDataCache.GetCategoryAsync).SafeInvokeAsync(categoryId, Cultures, ExecutionLog, GetFetchErrorMessage("CategoryData")).ConfigureAwait(false);

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
            var lotteryCI = (LotteryCI)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCI == null)
            {
                ExecutionLog.Debug($"Missing data. No lottery cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await lotteryCI.GetBonusInfoAsync().ConfigureAwait(false)
                : await new Func<Task<BonusInfoCI>>(lotteryCI.GetBonusInfoAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("BonusInfo")).ConfigureAwait(false);

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
            var lotteryCI = (LotteryCI)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCI == null)
            {
                ExecutionLog.Debug($"Missing data. No lottery cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await lotteryCI.GetDrawInfoAsync().ConfigureAwait(false)
                : await new Func<Task<DrawInfoCI>>(lotteryCI.GetDrawInfoAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("DrawInfo")).ConfigureAwait(false);

            return item == null
                ? null
                : new DrawInfo(item);
        }

        /// <summary>
        /// Asynchronously gets the list of ids of associated <see cref="IDraw"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IEnumerable<URN>> GetScheduledDrawsAsync()
        {
            var lotteryCI = (LotteryCI)SportEventCache.GetEventCacheItem(Id);
            if (lotteryCI == null)
            {
                ExecutionLog.Debug($"Missing data. No lottery cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await lotteryCI.GetScheduledDrawsAsync().ConfigureAwait(false)
                : await new Func<Task<IEnumerable<URN>>>(lotteryCI.GetScheduledDrawsAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ScheduledDraws")).ConfigureAwait(false);

            return item;
        }
    }
}
