/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a lottery draw
    /// </summary>
    internal interface IDrawCI : ISportEventCI
    {
        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> representing id of the associated <see cref="ILotteryCI"/>
        /// </summary>
        /// <returns>The id of the associated lottery</returns>
        Task<URN> GetLotteryIdAsync();

        /// <summary>
        /// Asynchronously gets <see cref="DrawStatus"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DrawStatus> GetStatusAsync();

        /// <summary>
        /// Asynchronously gets a bool value indicating if the results are in chronological order
        /// </summary>
        /// <returns>The value indicating if the results are in chronological order</returns>
        Task<bool> AreResultsChronologicalAsync();

        /// <summary>
        /// Asynchronously gets <see cref="IEnumerable{T}"/> list of associated <see cref="DrawResultCI"/>
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<DrawResultCI>> GetResultsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Gets the display identifier
        /// </summary>
        /// <value>The display identifier</value>
        Task<int?> GetDisplayIdAsync();
    }
}