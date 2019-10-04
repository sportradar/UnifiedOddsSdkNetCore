/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments of <see cref="IOddsFeedSession.OnUnparsableMessageReceived"/> event
    /// </summary>
    public class UnparsableMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="MessageType"/> member specifying the type of the unparsable message
        /// </summary>
        public MessageType MessageType { get; }

        /// <summary>
        /// Gets the <see cref="string"/> representation of the producer associated with the unparsable message
        /// </summary>
        public string Producer { get; }

        /// <summary>
        /// Gets the <see cref="string"/> representation of the sport event id associated with the unparsable message
        /// </summary>
        public string EventId { get; }

        /// <summary>
        /// The raw message
        /// </summary>
        private readonly byte[] _rawMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnparsableMessageEventArgs"/> class
        /// </summary>
        /// <param name="messageType">The <see cref="MessageType"/> member specifying the type of the unparsable message.</param>
        /// <param name="producer">The <see cref="string"/> representation of the producer associated with the unparsable message.</param>
        /// <param name="eventId">The <see cref="string"/> representation of the sport event id associated with the unparsable message.</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        public UnparsableMessageEventArgs(MessageType messageType, string producer, string eventId, byte[] rawMessage)
        {
            MessageType = messageType;
            Producer = producer;
            EventId = eventId;
            _rawMessage = rawMessage;
        }

        /// <summary>
        /// Gets the raw xml message received from the feed
        /// </summary>
        /// <returns>Returns the raw xml message received from the feed</returns>
        [Obsolete("The message was moved to event")]
        public string GetRawMessage()
        {
            return _rawMessage == null
                       ? null
                       : Encoding.UTF8.GetString(_rawMessage);
        }
    }
}
