// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Api;

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

        /// <summary>
        /// Gets all AMQP headers delivered with the message as a read-only string dictionary.
        /// The returned dictionary is never null.
        /// </summary>
        IReadOnlyDictionary<string, string> MessageHeaders { get; }
    }
}
