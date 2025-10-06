// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Api;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// A base class for all messages dispatched by the SDK
    /// </summary>
    [Serializable]
    internal abstract class Message : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        protected Message(IMessageTimestamp timestamp, IProducer producer)
        {
            Guard.Argument(timestamp, nameof(timestamp)).NotNull();

            Timestamps = timestamp;
            Producer = producer;
        }

        /// <summary>
        /// Gets the timestamps when the message was generated, sent, received and dispatched by the sdk
        /// </summary>
        /// <value>The timestamps</value>
        public IMessageTimestamp Timestamps { get; }

        /// <summary>
        /// Gets the <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="IMessage" /> message
        /// </summary>
        public IProducer Producer { get; }
    }
}
