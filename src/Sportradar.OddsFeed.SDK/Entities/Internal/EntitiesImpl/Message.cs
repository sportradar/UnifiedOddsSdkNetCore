// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using Sportradar.OddsFeed.SDK.Api;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// A base class for all messages dispatched by the SDK
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security",
                                                     "S5766",
                                                     Justification = "Message instances are not deserialized from untrusted data. The attribute is kept for backward compatibility.")]
    internal abstract class Message : IMessageV2
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyHeaders = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="messageHeaders">The AMQP message headers, or null for empty</param>
        protected Message(IMessageTimestamp timestamp, IProducer producer, IReadOnlyDictionary<string, string> messageHeaders = null)
        {
            Guard.Argument(timestamp, nameof(timestamp)).NotNull();
            Guard.Argument(producer, nameof(producer)).NotNull();

            Timestamps = timestamp;
            Producer = producer;
            MessageHeaders = messageHeaders ?? EmptyHeaders;
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

        /// <summary>
        /// Gets all AMQP headers delivered with the message as a read-only string dictionary.
        /// The returned dictionary is never null.
        /// </summary>
        public IReadOnlyDictionary<string, string> MessageHeaders { get; }
    }
}
