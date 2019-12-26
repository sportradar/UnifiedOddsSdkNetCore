/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents settlement information for a market
    /// </summary>
    public interface IMarketWithSettlement : IMarketCancel
    {
        /// <summary>
        /// Gets an <see cref="IEnumerable{IOutcomeSettlement}"/> where each <see cref="IOutcomeSettlement"/> instance provides
        /// settlement information for one outcome(selection)
        /// </summary>
        IEnumerable<IOutcomeSettlement> OutcomeSettlements { get; }
    }
}