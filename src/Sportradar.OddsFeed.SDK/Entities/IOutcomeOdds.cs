/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents an odds for an outcome(selection)
    /// </summary>
    public interface IOutcomeOdds : IOutcomeProbabilities
    {
        /// <summary>
        /// Gets the odds in specified format
        /// </summary>
        /// <param name="oddsDisplayType">Display type of the odds (default: <see cref="OddsDisplayType.Decimal"/>)</param>
        /// <returns>The value of the outcome odds in specified format</returns>
        double? GetOdds(OddsDisplayType oddsDisplayType = OddsDisplayType.Decimal);
    }
}
