/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents an odds for an outcome (selection)
    /// </summary>
    /// <remarks>Interface will be merged into base <see cref="IOutcomeOdds"/> in next major version scheduled for January 2019</remarks>
    public interface IOutcomeOddsV1 : IOutcomeOdds
    {
        /// <summary>
        /// Gets the odds in specified format
        /// </summary>
        /// <param name="oddsDisplayType">Display type of the odds (default: <see cref="OddsDisplayType.Decimal"/>)</param>
        /// <returns>The value of the outcome odds in specified format</returns>
        double? GetOdds(OddsDisplayType oddsDisplayType = OddsDisplayType.Decimal);
    }
}