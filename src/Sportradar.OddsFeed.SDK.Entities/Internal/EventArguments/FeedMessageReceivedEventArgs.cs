/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments
{
    /// <summary>
    /// An event argument used by events raised when a message from the feed is received
    /// </summary>
    public class FeedMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a <see cref="string"/> representing deserialized message
        /// </summary>
        public FeedMessage Message { get; }

        /// <summary>
        /// The <see cref="MessageInterest"/> specifying the interest of the associated session
        /// </summary>
        public MessageInterest Interest { get; }

        /// <summary>
        /// The raw message
        /// </summary>
        public byte[] RawMessage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedMessageReceivedEventArgs"/> class
        /// </summary>
        /// <param name="message">a <see cref="FeedMessage"/> representing the received message</param>
        /// <param name="interest">a <see cref="MessageInterest"/> specifying the interest of the associated session or a null reference
        /// if no session is associated with the received message</param>
        /// <param name="rawMessage">The raw message</param>
        public FeedMessageReceivedEventArgs(FeedMessage message, MessageInterest interest, byte[] rawMessage)
        {
            Contract.Requires(message != null);

            Message = message;
            Interest = interest;
            RawMessage = rawMessage;
        }
    }
}
