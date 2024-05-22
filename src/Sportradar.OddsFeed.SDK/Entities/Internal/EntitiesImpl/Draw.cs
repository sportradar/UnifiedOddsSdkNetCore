// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Class Draw
    /// </summary>
    /// <seealso cref="SportEvent" />
    /// <seealso cref="IDraw" />
    internal class Draw : SportEvent, IDraw
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Draw));

        /// <summary>
        /// Initializes a new instance of the <see cref="Draw"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> uniquely identifying the sport event associated with the current instance</param>
        /// <param name="sportId">A <see cref="Urn"/> uniquely identifying the sport associated with the current instance</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance containing <see cref="SportEventCacheItem"/></param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the initialized instance will handle potential exceptions</param>
        public Draw(Urn id,
                    Urn sportId,
                    ISportEventCache sportEventCache,
                    IReadOnlyCollection<CultureInfo> cultures,
                    ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
        }
        /// <summary>
        /// Asynchronously gets a <see cref="Urn" /> representing id of the associated <see cref="ILottery" />
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Urn> GetLotteryIdAsync()
        {
            var drawCacheItem = (DrawCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (drawCacheItem == null)
            {
                ExecutionLog.LogDebug("Missing data. No draw cache item for id={EventId}", Id);
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await drawCacheItem.GetLotteryIdAsync().ConfigureAwait(false)
                : await new Func<Task<Urn>>(drawCacheItem.GetLotteryIdAsync).SafeInvokeAsync(ExecutionLog, "LotteryId").ConfigureAwait(false);

            return item;
        }

        /// <summary>
        /// Asynchronously gets <see cref="DrawStatus" /> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="DrawStatus" /> associated with the current instance</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<DrawStatus> GetStatusAsync()
        {
            var drawCacheItem = (DrawCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (drawCacheItem == null)
            {
                ExecutionLog.LogDebug("Missing data. No draw cache item for id={EventId}", Id);
                return DrawStatus.Unknown;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await drawCacheItem.GetStatusAsync().ConfigureAwait(false)
                : await new Func<Task<DrawStatus>>(drawCacheItem.GetStatusAsync).SafeInvokeAsync(ExecutionLog, "DrawStatus").ConfigureAwait(false);

            return item;
        }

        /// <summary>
        /// Asynchronously gets list of associated <see cref="IDrawResult" />
        /// </summary>
        /// <returns>A list of associated <see cref="IDrawResult" /></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<IDrawResult>> GetResultsAsync()
        {
            var drawCacheItem = (DrawCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (drawCacheItem == null)
            {
                ExecutionLog.LogDebug("Missing data. No draw cache item for id={EventId}", Id);
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await drawCacheItem.GetResultsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<DrawResultCacheItem>>>(drawCacheItem.GetResultsAsync).SafeInvokeAsync(Cultures, ExecutionLog, "DrawResults").ConfigureAwait(false);

            return item?.Select(s => new DrawResult(s));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="int"/> representing display id
        /// </summary>
        /// <returns>The display id</returns>
        public async Task<int?> GetDisplayIdAsync()
        {
            var drawCacheItem = (DrawCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (drawCacheItem == null)
            {
                ExecutionLog.LogDebug("Missing data. No draw cache item for id={EventId}", Id);
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                           ? await drawCacheItem.GetDisplayIdAsync().ConfigureAwait(false)
                           : await new Func<Task<int?>>(drawCacheItem.GetDisplayIdAsync).SafeInvokeAsync(ExecutionLog, "DisplayId").ConfigureAwait(false);

            return item;
        }
    }
}
