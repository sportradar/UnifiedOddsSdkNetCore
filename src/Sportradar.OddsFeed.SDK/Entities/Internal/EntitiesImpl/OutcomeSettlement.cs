// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents the result of a market outcome (selection)
    /// </summary>
    internal class OutcomeSettlement : Outcome, IOutcomeSettlement
    {
        /// <summary>Initializes a new instance of the <see cref="OutcomeSettlement" /> class</summary>
        /// <param name="deadHeatFactor">a dead-head factor for the current <see cref="IOutcomeSettlement" /> instance.</param>
        /// <param name="eachWayResult">an each-way result for the current <see cref="IOutcomeSettlement" /> instance.</param>
        /// <param name="eachWayPlaceFactor">an each-way place factor for the current <see cref="IOutcomeSettlement" /> instance.</param>
        /// <param name="deadHeatFactorPlace">a dead-heat factor for the place part of an each-way bet.</param>
        /// <param name="id">the value uniquely identifying the current <see cref="IOutcomeSettlement" /></param>
        /// <param name="result">a value indicating whether the outcome associated with current <see cref="IOutcomeSettlement" /> is winning</param>
        /// <param name="voidFactor">the <see cref="VoidFactor" /> associated with a current <see cref="IOutcomeSettlement" /> or a null reference</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> used to generate the outcome name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapping ids of markets and outcomes</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition">The associated <see cref="IOutcomeDefinition"/></param>
        internal OutcomeSettlement(double? deadHeatFactor,
                                   EachWayResult? eachWayResult,
                                   double? eachWayPlaceFactor,
                                   double? deadHeatFactorPlace,
                                   string id,
                                   int result,
                                   VoidFactor? voidFactor,
                                   INameProvider nameProvider,
                                   IMarketMappingProvider mappingProvider,
                                   IReadOnlyCollection<CultureInfo> cultures,
                                   IOutcomeDefinition outcomeDefinition)
            : base(id, nameProvider, mappingProvider, cultures, outcomeDefinition)
        {
            DeadHeatFactor = deadHeatFactor;
            EachWayResult = eachWayResult;
            EachWayPlaceFactor = eachWayPlaceFactor;
            DeadHeatFactorPlace = deadHeatFactorPlace;
            VoidFactor = voidFactor;
            switch (result)
            {
                case 0:
                    OutcomeResult = OutcomeResult.Lost;
                    break;
                case 1:
                    OutcomeResult = OutcomeResult.Won;
                    break;
                case -1:
                    OutcomeResult = OutcomeResult.UndecidedYet;
                    break;
                default:
                    OutcomeResult = OutcomeResult.UnsupportedBySdk;
                    break;
            }
        }

        /// <summary>
        /// Gets a dead-head factor for the current <see cref="IOutcomeSettlement" /> instance.
        /// </summary>
        /// <remarks>
        /// A dead heat is defined as an event in which there are two or more joint winning contracts.
        /// Dead heat rules state that the stake should be divided by the number of competitors involved in the dead heat and then settled at the normal odds
        /// </remarks>
        public double? DeadHeatFactor { get; }

        /// <summary>
        /// Gets whether the each-way outcome is settled as a win or a place
        /// </summary>
        public EachWayResult? EachWayResult { get; }

        /// <summary>
        /// Gets the each-way place factor (fraction of win odds used to settle the place part)
        /// </summary>
        public double? EachWayPlaceFactor { get; }

        /// <summary>
        /// Gets the dead-heat factor for the place part of an each-way bet
        /// </summary>
        public double? DeadHeatFactorPlace { get; }

        /// <summary>
        /// Gets the <see cref="VoidFactor" /> associated with a current <see cref="IOutcomeSettlement" /> or a null reference.
        /// The value indicates the percentage of the stake that should be voided(returned to the punter).
        /// </summary>
        public VoidFactor? VoidFactor { get; }

        /// <summary>
        /// Gets a value indicating whether the outcome associated with current <see cref="IOutcomeSettlement"/> is winning - i.e. have the bets placed on this outcome winning or losing
        /// </summary>
        public OutcomeResult OutcomeResult { get; }
    }
}
