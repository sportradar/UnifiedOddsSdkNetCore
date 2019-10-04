/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a lottery
    /// </summary>
    internal interface ILotteryCI : ISportEventCI
    {
        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> representing  the associated category id
        /// </summary>
        /// <returns>The id of the associated category</returns>
        Task<URN> GetCategoryIdAsync();

        /// <summary>
        /// Asynchronously gets <see cref="BonusInfoCI"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<BonusInfoCI> GetBonusInfoAsync();

        /// <summary>
        /// Asynchronously gets <see cref="DrawInfoCI"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DrawInfoCI> GetDrawInfoAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="IDrawCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<URN>> GetScheduledDrawsAsync();
    }
}