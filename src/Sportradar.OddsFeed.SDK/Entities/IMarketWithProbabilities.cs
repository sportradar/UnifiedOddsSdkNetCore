/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by classes providing probability information for betting markets
    /// </summary>
    public interface IMarketWithProbabilities : IMarket
    {
        /// <summary>
        /// Gets a <see cref="MarketStatus"/> enum member specifying the status of the market associated with the current market
        /// </summary>
        MarketStatus Status { get; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{IOutcomeProbabilities}"/> where each <see cref="IOutcomeProbabilities"/> instance provides
        /// probabilities information for one outcome(selection)
        /// </summary>
        IEnumerable<IOutcomeProbabilities> OutcomeProbabilities { get; }
    }
}