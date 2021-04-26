/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract used to extract most crucial information from the message received from the feed.
    /// </summary>
    internal interface IMessageDataExtractor
    {
        /// <summary>
        /// Constructs and returns a <see cref="BasicMessageData"/> specifying the basic data of the message
        /// </summary>
        /// <param name="messageData">The raw message data.</param>
        /// <returns>a <see cref="BasicMessageData"/> specifying the basic data of the message</returns>
        BasicMessageData GetBasicMessageData(byte[] messageData);

        /// <summary>
        /// Gets the <see cref="MessageType"/> member from the provided <see cref="FeedMessage"/> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage"/> instance whose type is to be determined.</param>
        /// <returns>A <see cref="MessageType"/> enum member specifying the type of the provided <see cref="FeedMessage"/></returns>
        MessageType GetMessageTypeFromMessage(FeedMessage message);
    }
}
