/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract followed by all top-level messages produced by the feed
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets a <see cref="Producer"/> specifying the producer / service which dispatched the current <see cref="IMessage"/> message
        /// </summary>
        IProducer Producer { get; }

        /// <summary>
        /// Gets the timestamps when the message was generated, sent, received and dispatched by the sdk
        /// </summary>
        /// <value>The timestamps</value>
        IMessageTimestamp Timestamps { get; }
    }
}
