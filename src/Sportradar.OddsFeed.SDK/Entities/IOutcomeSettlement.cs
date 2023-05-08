/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represent settlement information for an outcome(market selection)
    /// </summary>
    public interface IOutcomeSettlement : IOutcome
    {
        /// <summary>
        /// Gets a dead-heat factor for the current <see cref="IOutcomeSettlement"/> instance
        /// </summary>
        /// <remarks>
        /// A dead heat is defined as an event in which there are two or more joint winning contracts
        /// Dead heat rules state that the stake should be divided by the number of competitors involved in the dead heat and then settled at the normal odds
        /// </remarks>
        double? DeadHeatFactor { get; }

        /// <summary>
        /// Gets a value indicating whether the outcome associated with current <see cref="IOutcomeSettlement"/> is winning - i.e. have the bets placed on this outcome winning or losing
        /// </summary>
        [Obsolete("Results may also include other values. Use OutcomeResult instead.")]
        bool Result { get; }

        /// <summary>
        /// Gets the <see cref="VoidFactor"/> associated with a current <see cref="IOutcomeSettlement"/> or a null reference. The value indicates
        /// the percentage of the stake that should be voided(returned to the punter).
        /// </summary>
        VoidFactor? VoidFactor { get; }

#pragma warning disable CS0618 // Type or member is obsolete
        /// <summary>
        /// Gets a value indicating whether the outcome associated with current <see cref="IOutcomeSettlement"/> is winning - i.e. have the bets placed on this outcome winning or losing
        /// </summary>
        OutcomeResult OutcomeResult => Result ? OutcomeResult.Won : OutcomeResult.Lost;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
