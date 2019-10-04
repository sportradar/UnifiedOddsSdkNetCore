/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Class MessageTimestamp
    /// </summary>
    /// <seealso cref="IMessageTimestamp" />
    public class MessageTimestamp : IMessageTimestamp
    {
        /// <summary>
        /// Gets the value specifying when the message was generated and put in rabbit queue (the milliseconds since EPOCH UTC)
        /// </summary>
        public long Created { get; }

        /// <summary>
        /// Gets the value specifying when the message was sent from the rabbit server (the milliseconds since EPOCH UTC)
        /// </summary>
        public long Sent { get; }

        /// <summary>
        /// Gets the value specifying when the message was received for processing by the sdk (the milliseconds since EPOCH UTC)
        /// </summary>
        public long Received { get; }

        /// <summary>
        /// Gets the value specifying when the message was dispatched to the user (the milliseconds since EPOCH UTC)
        /// </summary>
        public long Dispatched { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageTimestamp"/> class
        /// </summary>
        /// <param name="timestamp">The timestamp</param>
        public MessageTimestamp(long timestamp)
        {
            Contract.Requires(timestamp > 0);

            Created = timestamp;
            Sent = timestamp;
            Received = timestamp;
            Dispatched = timestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageTimestamp"/> class
        /// </summary>
        /// <param name="generated">The generated</param>
        /// <param name="sent">The sent</param>
        /// <param name="received">The received</param>
        /// <param name="dispatched">The dispatched</param>
        public MessageTimestamp(long generated, long sent, long received, long dispatched)
        {
            Contract.Requires(generated > 0);
            //Contract.Requires(sent > 0);
            Contract.Requires(received > 0);
            //Contract.Requires(dispatched > 0);

            Created = generated;
            Sent = sent;
            Received = received;
            Dispatched = dispatched;
        }
    }
}
