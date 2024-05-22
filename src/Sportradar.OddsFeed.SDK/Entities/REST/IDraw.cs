// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent information about a lottery draw
    /// </summary>
    public interface IDraw : ISportEvent
    {
        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> representing id of the associated <see cref="ILottery"/>
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        Task<Urn> GetLotteryIdAsync();

        /// <summary>
        /// Asynchronously gets <see cref="DrawStatus"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DrawStatus> GetStatusAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="IDrawResult"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<IDrawResult>> GetResultsAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="int"/> representing display id
        /// </summary>
        /// <returns>The display id</returns>
        Task<int?> GetDisplayIdAsync();
    }
}
