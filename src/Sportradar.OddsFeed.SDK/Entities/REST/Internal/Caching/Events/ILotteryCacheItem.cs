// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a lottery
    /// </summary>
    internal interface ILotteryCacheItem : ISportEventCacheItem
    {
        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> representing  the associated category id
        /// </summary>
        /// <returns>The id of the associated category</returns>
        Task<Urn> GetCategoryIdAsync();

        /// <summary>
        /// Asynchronously gets <see cref="BonusInfoCacheItem"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<BonusInfoCacheItem> GetBonusInfoAsync();

        /// <summary>
        /// Asynchronously gets <see cref="DrawInfoCacheItem"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DrawInfoCacheItem> GetDrawInfoAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="IDrawCacheItem"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<Urn>> GetScheduledDrawsAsync();
    }
}
