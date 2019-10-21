/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a pitcher
    /// </summary>
    /// <seealso cref="SportEntityDTO" />
    public class PitcherDTO : SportEntityDTO
    {
        /// <summary>
        /// Gets the hand with which player pitches
        /// </summary>
        /// <value>The hand with which player pitches</value>
        public PlayerHand Hand { get; }

        /// <summary>
        /// Gets the indicator if the competitor is Home or Away
        /// </summary>
        /// <value>The indicator if the competitor is Home or Away</value>
        public HomeAway Competitor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitcherDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="pitcher"/> containing information about a pitcher</param>
        internal PitcherDTO(pitcher record)
            :base(record.id, record.name)
        {
            Guard.Argument(record).NotNull();
            Hand = record.hand.Equals("l", StringComparison.InvariantCultureIgnoreCase)
                ? PlayerHand.Left
                : PlayerHand.Right;
            Competitor = record.competitor.Equals("home", StringComparison.InvariantCultureIgnoreCase)
                ? HomeAway.Home
                : HomeAway.Away;
        }
    }
}
