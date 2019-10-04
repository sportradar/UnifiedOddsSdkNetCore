/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by classes providing odds information for betting markets
    /// </summary>
    public interface IMarketWithOdds : IMarket
    {
        /// <summary>
        /// Gets a <see cref="MarketStatus"/> enum member specifying the status of the market associated with the current <see cref="IMarketWithOdds"/> instance
        /// </summary>
        MarketStatus Status { get; }

        /// <summary>
        /// Gets a <see cref="CashoutStatus"/> enum member specifying the availability of cashout, or a null reference
        /// </summary>
        CashoutStatus? CashoutStatus { get; }

        /// <summary>
        /// Gets a value indicating whether the market associated with the current instance is the favorite market (i.e. the one with most balanced odds)
        /// </summary>
        bool IsFavorite { get; }

        /// <summary>
        /// Gets an <see cref="IEnumerable{IOutcomeOdds}"/> where each <see cref="IOutcomeOdds"/> instance provides
        /// odds information for one outcome(selection)
        /// </summary>
        IEnumerable<IOutcomeOdds> OutcomeOdds { get; }

        /// <summary>
        /// Gets the market metadata which contains the additional market information
        /// </summary>
        /// <value>The market metadata which contains the additional market information</value>
        IMarketMetadata MarketMetadata { get; }
    }
}
