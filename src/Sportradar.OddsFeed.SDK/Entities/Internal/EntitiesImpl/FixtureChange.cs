/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed when a fixture changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FixtureChange<T> : EventMessage<T>, IFixtureChange<T> where T : ISportEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixtureChange{T}" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="changeType">A <see cref="FixtureChangeType"/> indicating how the fixture was changed (added, re-scheduled, ...)</param>
        /// <param name="nextLiveTime">The next live time</param>
        /// <param name="startTime">A value specifying the start time of the fixture in milliseconds since EPOCH UTC </param>
        /// <param name="rawMessage">The raw message</param>
        public FixtureChange(IMessageTimestamp timestamp, IProducer producer, T @event, long? requestId, FixtureChangeType changeType, long? nextLiveTime, long startTime, byte[] rawMessage)
            : base(timestamp, producer, @event, requestId, rawMessage)
        {
            ChangeType = changeType;
            NextLiveTime = nextLiveTime;
            StartTime = startTime;
        }

        /// <summary>
        /// Gets a <see cref="FixtureChangeType"/> indicating how the fixture was changed (added, re-scheduled, ...)
        /// </summary>
        /// <remarks>ToDo: ideally this should be nullable</remarks>
        public FixtureChangeType ChangeType { get; }

        /// <summary>
        /// Gets a value specifying the start time of the fixture in milliseconds since EPOCH UTC after the fixture was re-scheduled
        /// </summary>
        /// <value>The next live time</value>
        public long? NextLiveTime { get; }

        /// <summary>
        /// Gets a value specifying the start time of the fixture in milliseconds since EPOCH UTC
        /// </summary>
        public long StartTime { get; }
    }
}