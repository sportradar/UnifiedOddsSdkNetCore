/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Implementation of <see cref="IOddsGeneration"/>
    /// </summary>
    /// <remarks>Provided by the prematch odds producer only, and contains a few key-parameters that can be used in a client’s own special odds model, or even offer spread betting bets based on it.</remarks>
    /// <seealso cref="Sportradar.OddsFeed.SDK.Entities.IOddsGeneration" />
    internal class OddsGeneration : IOddsGeneration
    {
        /// <summary>
        /// Gets the expected totals (how many goals are expected in total?)
        /// </summary>
        /// <value>The expected totals</value>
        public double? ExpectedTotals { get; }

        /// <summary>
        /// Gets the expected supremacy (how big is the expected goal supremacy)
        /// </summary>
        /// <value>The expected supremacy</value>
        public double? ExpectedSupremacy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OddsGeneration"/> class
        /// </summary>
        /// <param name="oddsGenerationProperties">The odds generation properties</param>
        public OddsGeneration(oddsGenerationProperties oddsGenerationProperties)
        {
            Guard.Argument(oddsGenerationProperties).NotNull();

            ExpectedTotals = oddsGenerationProperties.expected_totalsSpecified
                                 ? oddsGenerationProperties.expected_totals
                                 : (double?) null;
            ExpectedSupremacy = oddsGenerationProperties.expected_supremacySpecified
                                    ? oddsGenerationProperties.expected_supremacy
                                    : (double?) null;
        }
    }
}
