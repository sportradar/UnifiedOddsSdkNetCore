/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents the result of a market outcome (selection)
    /// </summary>
    internal class OutcomeSettlement : Outcome, IOutcomeSettlement
    {
        /// <summary>Initializes a new instance of the <see cref="OutcomeSettlement" /> class</summary>
        /// <param name="deadHeatFactor">a dead-head factor for the current <see cref="IOutcomeSettlement" /> instance.</param>
        /// <param name="id">the value uniquely identifying the current <see cref="IOutcomeSettlement" /></param>
        /// <param name="result">a value indicating whether the outcome associated with current <see cref="IOutcomeSettlement" /> is winning</param>
        /// <param name="voidFactor">the <see cref="VoidFactor" /> associated with a current <see cref="IOutcomeSettlement" /> or a null reference</param>
        /// <param name="nameProvider">A <see cref="INameProvider"/> used to generate the outcome name(s)</param>
        /// <param name="mappingProvider">A <see cref="IMarketMappingProvider"/> instance used for providing mapping ids of markets and outcomes</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition">The associated <see cref="IOutcomeDefinition"/></param>
        internal OutcomeSettlement(double? deadHeatFactor,
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
#pragma warning disable CS0618
            Result = result == 1;
#pragma warning restore CS0618
            VoidFactor = voidFactor;
            OutcomeResult = result switch
            {
                0 => OutcomeResult.Lost,
                1 => OutcomeResult.Won,
                _ => OutcomeResult.UndecidedYet
            };
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
        /// Gets a value indicating whether the outcome associated with current <see cref="IOutcomeSettlement" /> is winning - i.e. have the bets placed on this outcome winning or losing.
        /// </summary>
        [Obsolete("Results may also include other values. Use OutcomeResult instead.")]
        public bool Result { get; }

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
