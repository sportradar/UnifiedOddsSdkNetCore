/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a lottery draw
    /// </summary>
    internal interface IDrawCacheItem : ISportEventCacheItem
    {
        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> representing id of the associated <see cref="ILotteryCacheItem"/>
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        Task<Urn> GetLotteryIdAsync();

        /// <summary>
        /// Asynchronously gets <see cref="DrawStatus"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DrawStatus> GetStatusAsync();

        /// <summary>
        /// Asynchronously gets a bool value indicating if the results are in chronological order
        /// </summary>
        /// <returns>The value indicating if the results are in chronological order</returns>
        // TODO: public interface does not expose this
        Task<bool> AreResultsChronologicalAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="DrawResultCacheItem"/>
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<DrawResultCacheItem>> GetResultsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Gets the display identifier
        /// </summary>
        /// <value>The display identifier</value>
        Task<int?> GetDisplayIdAsync();
    }
}
