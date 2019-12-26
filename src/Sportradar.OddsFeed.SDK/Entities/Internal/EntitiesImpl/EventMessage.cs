/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a base class for all messages dispatched by the feed associated with the sport event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class EventMessage<T> : Message, IEventMessage<T> where T : ISportEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventMessage{T}" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="rawMessage">The raw message </param>
        protected EventMessage(IMessageTimestamp timestamp, IProducer producer, T @event, long? requestId, byte[] rawMessage)
            : base(timestamp, producer)
        {
            Guard.Argument(@event, nameof(@event)).Require(@event != null);

            Event = @event;
            RequestId = requestId;
            RawMessage = rawMessage;
        }

        /// <summary>
        /// Gets a <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" />
        /// </summary>
        public T Event { get; }

        /// <summary>
        /// Get the id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference if no requestId was provided to the request
        /// </summary>
        public long? RequestId { get; }

        /// <summary>
        /// Gets the raw message received from the broker
        /// </summary>
        /// <value>The raw message received from the broker</value>
        public byte[] RawMessage { get; }
    }
}