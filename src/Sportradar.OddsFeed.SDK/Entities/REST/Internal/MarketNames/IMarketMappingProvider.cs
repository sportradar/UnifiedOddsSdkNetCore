/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide mapping ids of markets and outcomes
    /// </summary>
    public interface IMarketMappingProvider
    {
        /// <summary>
        /// Asynchronously gets the market mapping Id of the specified market
        /// </summary>
        /// <param name="cultures">The list of <see cref="CultureInfo"/> to fetch <see cref="IMarketMapping"/></param>
        /// <returns>A <see cref="Task{IMarketMappingId}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<IMarketMapping>> GetMappedMarketIdAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets the mapping Id of the specified outcome
        /// </summary>
        /// <param name="outcomeId">The outcome identifier used to get mapped outcomeId</param>
        /// <param name="cultures">The list of <see cref="CultureInfo"/> to fetch <see cref="IOutcomeMapping"/></param>
        /// <returns>A <see cref="Task{IOutcomeMappingId}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<IOutcomeMapping>> GetMappedOutcomeIdAsync(string outcomeId, IEnumerable<CultureInfo> cultures);
    }
}
