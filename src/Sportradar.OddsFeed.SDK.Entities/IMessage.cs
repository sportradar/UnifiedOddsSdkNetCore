/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract followed by all top-level messages produced by the feed
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets the value specifying when the message was generated in the milliseconds since EPOCH UTC
        /// </summary>
        [Obsolete("Check Timestamps for all available timestamps")]
        long Timestamp { get; }

        /// <summary>
        /// Gets a <see cref="Producer"/> specifying the producer / service which dispatched the current <see cref="IMessage"/> message
        /// </summary>
        IProducer Producer { get; }
    }
}
