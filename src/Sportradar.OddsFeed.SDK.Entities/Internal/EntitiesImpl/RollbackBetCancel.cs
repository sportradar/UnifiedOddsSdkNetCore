/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed indicating that cancellation specified by <see cref="BetCancel{T}" /> message must be roll-backed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RollbackBetCancel<T> : MarketMessage<IMarketCancel, T>, IRollbackBetCancel<T> where T : ISportEvent
    {
        /// <summary>
        /// Get the number of milliseconds from UTC epoch representing the start of rollback cancellation period.
        /// A null value indicates the period started with market activation
        /// </summary>
        public long? StartTime { get; }

        /// <summary>
        /// Gets the number of milliseconds from UTC epoch representing the end of rollback cancellation period.
        /// A null value indicates the period ended when the market was closed
        /// </summary>
        public long? EndTime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbackBetCancel{T}" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="startTime">The number of milliseconds from UTC epoch representing the start of rollback cancellation period</param>
        /// <param name="endTime">The number of milliseconds from UTC epoch representing the end of rollback cancellation period</param>
        /// <param name="markets">An <see cref="IEnumerable{T}" /> describing markets associated with the current <see cref="IMarketMessage{T, R}" /></param>
        /// <param name="rawMessage">The raw message</param>
        public RollbackBetCancel(IMessageTimestamp timestamp, IProducer producer, T @event, long? requestId, long? startTime, long? endTime, IEnumerable<IMarketCancel> markets, byte[] rawMessage)
            : base(timestamp, producer, @event, requestId, markets, rawMessage)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}