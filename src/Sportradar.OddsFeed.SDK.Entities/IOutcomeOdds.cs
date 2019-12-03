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
        [Obsolete("This value returns Decimal odds value. Use IOutcomeOdds.GetOdds() to retrieve odds by desired format.")]
        double Odds { get; }

        /// <summary>
        /// Gets the odds in specified format
        /// </summary>
        /// <param name="oddsDisplayType">Display type of the odds (default: <see cref="OddsDisplayType.Decimal"/>)</param>
        /// <returns>The value of the outcome odds in specified format</returns>
        double? GetOdds(OddsDisplayType oddsDisplayType = OddsDisplayType.Decimal);
    }
}
