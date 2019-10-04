/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    ///     Represents a message dispatched by the feed indicating that markets have been cleared
    ///     and bets associated with them can be settled
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BetSettlement<T> : MarketMessage<IMarketWithSettlement, T>, IBetSettlement<T> where T : ISportEvent
    {
        public BetSettlementCertainty Certainty { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetSettlement{T}" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="markets">An <see cref="IEnumerable{T}" /> describing markets associated with the current <see cref="IMarketMessage{T, R}" /></param>
        /// <param name="certainty">A <see cref="IBetSettlement{T}"/> certainty</param>
        /// <param name="rawMessage">The raw message</param>
        public BetSettlement(IMessageTimestamp timestamp,
                             IProducer producer,
                             T @event,
                             long? requestId,
                             IEnumerable<IMarketWithSettlement> markets,
                             int certainty,
                             byte[] rawMessage)
            : base(timestamp, producer, @event, requestId, markets, rawMessage)
        {
            if (certainty == 1)
            {
                Certainty = BetSettlementCertainty.LiveScouted;
            }
            else if (certainty == 2)
            {
                Certainty = BetSettlementCertainty.Confirmed;
            }
            else
            {
                Certainty = BetSettlementCertainty.Unknown;
            }
        }
    }
}