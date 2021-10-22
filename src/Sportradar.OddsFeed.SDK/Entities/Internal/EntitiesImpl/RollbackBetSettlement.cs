/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed indicating settlements notified by <see cref="IBetSettlement{T}" /> message have to be reverted
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RollbackBetSettlement<T> : MarketMessage<IMarketCancel, T>, IRollbackBetSettlement<T> where T : ISportEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RollbackBetSettlement{T}" /> message
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="competition">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="markets">An <see cref="IMarketMessage{T,T1}" /> describing markets associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="rawMessage">The raw message</param>
        public RollbackBetSettlement(IMessageTimestamp timestamp, IProducer producer, T competition, long? requestId, IEnumerable<IMarketCancel> markets, byte[] rawMessage)
            : base(timestamp, producer, competition, requestId, markets, rawMessage)
        {

        }
    }
}