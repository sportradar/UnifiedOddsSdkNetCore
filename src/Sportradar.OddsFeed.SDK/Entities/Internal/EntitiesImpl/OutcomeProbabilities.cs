/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents market selection with probabilities
    /// </summary>
    internal class OutcomeProbabilities : Outcome, IOutcomeProbabilities
    {
        /// <summary>
        /// Gets a value indicating whether the current <see cref="IOutcomeOdds" /> is active - i.e. should bets on it be accepted
        /// </summary>
        public bool? Active { get; }

        /// <summary>
        /// Gets the probabilities for the current <see cref="IOutcomeOdds" /> instance or a null reference if probabilities are not known.
        /// </summary>
        public double? Probabilities { get; }

        /// <summary>Initializes a new instance of the <see cref="OutcomeProbabilities" /> class</summary>
        /// <param name="id">The value uniquely identifying the current <see cref="OutcomeProbabilities" /> instance</param>
        /// <param name="active">A value indicating whether the current <see cref="OutcomeProbabilities" /> is active - i.e. should bets on it be accepted
        /// </param>
        /// <param name="probabilities">The probabilities for the current <see cref="OutcomeProbabilities" /> instance</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> used to generate the outcome name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapping ids of markets and outcomes</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition">The associated <see cref="IOutcomeDefinition"/></param>
        internal OutcomeProbabilities(string id,
                                      bool? active,
                                      double? probabilities,
                                      INameProvider nameProvider,
                                      IMarketMappingProvider mappingProvider,
                                      IEnumerable<CultureInfo> cultures,
                                      IOutcomeDefinition outcomeDefinition)
            : base(id, nameProvider, mappingProvider, cultures, outcomeDefinition)
        {
            Active = active;
            Probabilities = probabilities;
        }
    }
}