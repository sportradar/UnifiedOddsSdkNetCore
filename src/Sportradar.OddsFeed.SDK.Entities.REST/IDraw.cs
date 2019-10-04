/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent information about a lottery draw
    /// </summary>
    public interface IDraw : ISportEvent
    {
        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> representing id of the associated <see cref="ILottery"/>
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        Task<URN> GetLotteryIdAsync();

        /// <summary>
        /// Asynchronously gets <see cref="DrawStatus"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DrawStatus> GetStatusAsync();

        ///// <summary>
        ///// Asynchronously gets a bool value indicating if the results are in chronological order
        ///// </summary>
        ///// <returns>The value indicating if the results are in chronological order</returns>
        //Task<bool> AreResultsChronologicalAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="IDrawResult"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<IDrawResult>> GetResultsAsync();
    }
}
