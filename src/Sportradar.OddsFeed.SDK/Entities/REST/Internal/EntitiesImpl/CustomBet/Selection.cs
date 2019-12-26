/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to provides an requested selection
    /// </summary>
    public class Selection : ISelection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Selection"/> class
        /// </summary>
        /// <param name="eventId">a <see cref="URN"/> representing the event id</param>
        /// <param name="marketId">a value representing the market id</param>
        /// <param name="specifiers">a value representing the specifiers</param>
        /// <param name="outcomeId">a value representing the outcome id</param>
        internal Selection(URN eventId, int marketId, string specifiers, string outcomeId)
        {
            if (eventId == null)
                throw new ArgumentNullException(nameof(eventId));
            if (specifiers == null)
                throw new ArgumentNullException(nameof(specifiers));
            if (outcomeId == null)
                throw new ArgumentNullException(nameof(outcomeId));

            EventId = eventId;
            MarketId = marketId;
            Specifiers = specifiers;
            OutcomeId = outcomeId;
        }

        public URN EventId { get; }
        public int MarketId { get; }
        public string Specifiers { get; }
        public string OutcomeId { get; }
    }
}
