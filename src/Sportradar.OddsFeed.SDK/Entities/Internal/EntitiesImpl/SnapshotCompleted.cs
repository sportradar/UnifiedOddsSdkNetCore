/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed after all the messages associated with snapshot request were dispatched
    /// </summary>
    internal class SnapshotCompleted : Message, ISnapshotCompleted
    {
        /// <summary>
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="requestId">the id of the request which triggered the current <see cref="SnapshotCompleted" /> message</param>
        public SnapshotCompleted(IMessageTimestamp timestamp, IProducer producer, long requestId)
            : base(timestamp, producer)
        {
            RequestId = requestId;
        }

        /// <summary>
        /// Get the id of the request which triggered the current <see cref="SnapshotCompleted" /> message
        /// </summary>
        public long RequestId { get; }
    }
}