/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.CustomBet
{
    /// <summary>
    /// Provides an available selections for a particular market
    /// </summary>
    public interface IMarket
    {
        /// <summary>
        /// Gets the id of the market
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the specifiers for this market
        /// </summary>
        string Specifiers { get; }

        /// <summary>
        /// Gets the outcomes for this market
        /// </summary>
        IEnumerable<string> Outcomes { get; }
    }
}
