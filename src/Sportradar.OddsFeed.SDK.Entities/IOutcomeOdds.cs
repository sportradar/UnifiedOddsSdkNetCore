/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents an odds for an outcome(selection)
    /// </summary>
    public interface IOutcomeOdds : IOutcomeProbabilities
    {
        /// <summary>
        /// Gets the odds for the current <see cref="IOutcomeOdds"/> instance
        /// </summary>
        [Obsolete("This value returns Decimal odds value. Use IOutcomeOddsV1.GetOdds() to retrieve odds by desired format.")]
        double Odds { get; }
    }
}
