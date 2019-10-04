/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes capable of delivering messages from the feed
    /// </summary>
    public interface IMessageReceiver
    {
        /// <summary>
        /// Gets a value indicating whether the current <see cref="IMessageReceiver"/> is currently opened;
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Event raised when the <see cref="IMessageReceiver"/> receives the message
        /// </summary>
        event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;

        /// <summary>
        /// Event raised when the <see cref="IMessageReceiver"/> could not deserialize the received message
        /// </summary>
        event EventHandler<MessageDeserializationFailedEventArgs> FeedMessageDeserializationFailed;

        /// <summary>
        /// Event raised when the <see cref="IMessageReceiver"/> receives the message
        /// </summary>
        event EventHandler<RawFeedMessageEventArgs> RawFeedMessageReceived;

        /// <summary>
        /// Opens the current <see cref="IMessageReceiver"/> instance so it starts receiving messages
        /// </summary>
        /// <param name="routingKeys">A list of strings representing routing key</param>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session using this instance</param>
        void Open(MessageInterest interest, IEnumerable<string> routingKeys);

        /// <summary>
        /// Closes the current <see cref="IMessageReceiver"/> instance so it will no longer receive messages
        /// </summary>
        void Close();
    }
}
