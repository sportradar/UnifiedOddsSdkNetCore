/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent information about a lottery
    /// </summary>
    public interface ILottery : ILongTermEvent
    {
        /// <summary>
        /// Asynchronously gets the associated category
        /// </summary>
        /// <returns>The associated category</returns>
        Task<ICategorySummary> GetCategoryAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IBonusInfo"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IBonusInfo> GetBonusInfoAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IDrawInfo"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IDrawInfo> GetDrawInfoAsync();

        /// <summary>
        /// Asynchronously gets the list of ids of associated <see cref="IDraw"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<URN>> GetScheduledDrawsAsync();
    }
}
