/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    ///     Represents the odds for an outcome
    /// </summary>
    internal class OutcomeOdds : OutcomeProbabilities, IOutcomeOddsV1
    {
        /// <summary>
        /// Gets the odds for the current <see cref="IOutcomeOdds" /> instance
        /// </summary>
        public double Odds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutcomeOdds" /> class
        /// </summary>
        /// <param name="id">the value uniquely identifying the current <see cref="OutcomeOdds" /> instance</param>
        /// <param name="active">A value indicating whether the current <see cref="OutcomeOdds" /> is active - i.e. should bets on it be accepted </param>
        /// <param name="odds">the odds for the current <see cref="OutcomeOdds" /> instance</param>
        /// <param name="probabilities">the probabilities for the current <see cref="OutcomeOdds" /> instance</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> used to generate the outcome name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapping ids of markets and outcomes</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition">The associated <see cref="IOutcomeDefinition"/></param>
        internal OutcomeOdds(string id,
                             bool? active,
                             double odds,
                             double? probabilities,
                             INameProvider nameProvider,
                             IMarketMappingProvider mappingProvider,
                             IEnumerable<CultureInfo> cultures,
                             IOutcomeDefinition outcomeDefinition)
            : base(id, active, probabilities, nameProvider, mappingProvider, cultures, outcomeDefinition)
        {
            Odds = odds;
        }

        /// <summary>
        /// Gets the odds in specified format
        /// </summary>
        /// <param name="oddsDisplayType">Display type of the odds (default: <see cref="OddsDisplayType.Decimal"/>)</param>
        /// <returns>The value of the outcome odds in specified format</returns>
        public double? GetOdds(OddsDisplayType oddsDisplayType = OddsDisplayType.Decimal)
        {
            if (oddsDisplayType == OddsDisplayType.Decimal)
            {
                return Odds;
            }

            return (double?) ConvertEuOddsToUs((decimal)Odds);
        }

        private static decimal? ConvertEuOddsToUs(decimal oddsEu)
        {
            decimal? oddUs;
            if (oddsEu == 1)
            {
                oddUs = null;
            }
            else if (oddsEu >= 2)
            {
                oddUs = (oddsEu - 1) * 100;
            }
            else
            {
                oddUs = (-100) / (oddsEu - 1);
            }

            return oddUs;
        }
    }
}