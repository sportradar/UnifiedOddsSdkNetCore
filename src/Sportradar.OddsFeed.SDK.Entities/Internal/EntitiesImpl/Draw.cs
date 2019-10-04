/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Class Draw
    /// </summary>
    /// <seealso cref="SportEvent" />
    /// <seealso cref="IDraw" />
    internal class Draw : SportEvent, IDrawV1
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Draw));

        /// <summary>
        /// Initializes a new instance of the <see cref="Draw"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN"/> uniquely identifying the sport event associated with the current instance</param>
        /// <param name="sportId">A <see cref="URN"/> uniquely identifying the sport associated with the current instance</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCI"/></param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the initialized instance will handle potential exceptions</param>
        public Draw(URN id,
                    URN sportId,
                    ISportEventCache sportEventCache,
                    IEnumerable<CultureInfo> cultures,
                    ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
        }
        /// <summary>
        /// Asynchronously gets a <see cref="T:Sportradar.OddsFeed.SDK.Messages.URN" /> representing id of the associated <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.ILottery" />
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<URN> GetLotteryIdAsync()
        {
            var drawCI = (DrawCI)SportEventCache.GetEventCacheItem(Id);
            if (drawCI == null)
            {
                ExecutionLog.Debug($"Missing data. No draw cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await drawCI.GetLotteryIdAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(drawCI.GetLotteryIdAsync).SafeInvokeAsync(ExecutionLog, "LotteryId").ConfigureAwait(false);

            return item;
        }

        /// <summary>
        /// Asynchronously gets <see cref="DrawStatus" /> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing an async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<DrawStatus> GetStatusAsync()
        {
            var drawCI = (DrawCI)SportEventCache.GetEventCacheItem(Id);
            if (drawCI == null)
            {
                ExecutionLog.Debug($"Missing data. No draw cache item for id={Id}.");
                return DrawStatus.Unknown;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await drawCI.GetStatusAsync().ConfigureAwait(false)
                : await new Func<Task<DrawStatus>>(drawCI.GetStatusAsync).SafeInvokeAsync(ExecutionLog, "DrawStatus").ConfigureAwait(false);

            return item;
        }

        /// <summary>
        /// Asynchronously gets <see cref="T:System.Collections.Generic.IEnumerable`1" /> list of associated <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.IDrawResult" />
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing an async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<IEnumerable<IDrawResult>> GetResultsAsync()
        {
            var drawCI = (DrawCI) SportEventCache.GetEventCacheItem(Id);
            if (drawCI == null)
            {
                ExecutionLog.Debug($"Missing data. No draw cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await drawCI.GetResultsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<DrawResultCI>>>(drawCI.GetResultsAsync).SafeInvokeAsync(Cultures, ExecutionLog, "DrawResults").ConfigureAwait(false);

            return item?.Select(s => new DrawResult(s));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="int"/> representing display id
        /// </summary>
        /// <returns>The display id</returns>
        public async Task<int?> GetDisplayIdAsync()
        {
            var drawCI = (DrawCI)SportEventCache.GetEventCacheItem(Id);
            if (drawCI == null)
            {
                ExecutionLog.Debug($"Missing data. No draw cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                           ? await drawCI.GetDisplayIdAsync().ConfigureAwait(false)
                           : await new Func<Task<int?>>(drawCI.GetDisplayIdAsync).SafeInvokeAsync(ExecutionLog, "DisplayId").ConfigureAwait(false);

            return item;
        }
    }
}
