/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to provides an requested selection
    /// </summary>
    internal class Selection : ISelection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Selection"/> class
        /// </summary>
        /// <param name="eventId">a <see cref="URN"/> representing the event id</param>
        /// <param name="marketId">a value representing the market id</param>
        /// <param name="outcomeId">a value representing the outcome id</param>
        /// <param name="specifiers">a value representing the specifiers</param>
        internal Selection(URN eventId, int marketId, string outcomeId, string specifiers)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            MarketId = marketId > 0 ? marketId : throw new ArgumentException("Missing marketId", nameof(marketId));
            OutcomeId = outcomeId ?? throw new ArgumentNullException(nameof(outcomeId));
            Specifiers = specifiers;
        }

        /// <summary>
        /// Gets the event id.
        /// </summary>
        public URN EventId { get; }

        /// <summary>
        /// Gets the market id.
        /// </summary>
        public int MarketId { get; }

        /// <summary>
        /// Gets the outcome id.
        /// </summary>
        public string OutcomeId { get; }

        /// <summary>
        /// Gets the specifiers.
        /// </summary>
        public string Specifiers { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var specifiers = Specifiers.IsNullOrEmpty() ? string.Empty : $",Specifiers={Specifiers}";
            return $"Event={EventId},Market={MarketId},Outcome={OutcomeId}{specifiers}";
        }
    }
}
