/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Linq;
using System.Text;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Class used to extract most basic information from raw feed message
    /// </summary>
    /// <seealso cref="IMessageDataExtractor" />
    internal class MessageDataExtractor : IMessageDataExtractor
    {
        private static readonly ILog Log = SdkLoggerFactory.GetLogger(typeof(MessageDataExtractor));

        /// <summary>
        /// A formats describing search targets
        /// </summary>
        private static readonly string[] TargetFormats =
        {
            "<{0} ", //'<odds_change '
            "<{0}>", //'<odds_change>'
            "</{0}>"  //'</odds_change>
        };

        /// <summary>
        /// Names of supported xml messages
        /// </summary>
        private static readonly Tuple<string, MessageType>[] MessageTypes =
        {
            new Tuple<string, MessageType>(odds_change.MessageName, MessageType.ODDS_CHANGE),
            new Tuple<string, MessageType>(bet_settlement.MessageName, MessageType.BET_SETTLEMENT),
            new Tuple<string, MessageType>(rollback_bet_settlement.MessageName, MessageType.ROLLBACK_BET_SETTLEMENT),
            new Tuple<string, MessageType>(bet_cancel.MessageName, MessageType.BET_CANCEL),
            new Tuple<string, MessageType>(rollback_bet_cancel.MessageName, MessageType.ROLLBACK_BET_CANCEL),
            new Tuple<string, MessageType>(alive.MessageName, MessageType.ALIVE),
            new Tuple<string, MessageType>(snapshot_complete.MessageName, MessageType.SNAPSHOT_COMPLETE),
            new Tuple<string, MessageType>(fixture_change.MessageName, MessageType.FIXTURE_CHANGE),
            new Tuple<string, MessageType>(bet_stop.MessageName, MessageType.BET_STOP)
        };

        /// <summary>
        /// A string containing a single double quote
        /// </summary>
        private const string Quote = "\"";

        /// <summary>
        /// The name of the 'product' attribute
        /// </summary>
        private const string ProductIdAttributeName = "product=\"";

        /// <summary>
        /// The name of the 'event_id' attribute
        /// </summary>
        private const string EventIdAttributeName = "event_id=\"";

        /// <summary>
        /// Extracts and returns <see cref="MessageType"/> member specifying the type of the provided xml message
        /// </summary>
        /// <param name="message">A <see cref="string"/> representation of the message.</param>
        /// <returns>The <see cref="MessageType"/> member specifying the type of the provided xml message.</returns>
        private static MessageType ExtractMessageName(string message)
        {
            Guard.Argument(!string.IsNullOrEmpty(message));

            foreach (var messageName in MessageTypes)
            {
                foreach (var target in TargetFormats.Select(format => string.Format(format, messageName.Item1)))
                {
                    if (message.Contains(target))
                    {
                        return messageName.Item2;
                    }
                }
            }
            return MessageType.UNKNOWN;
        }

        /// <summary>
        /// Extracts and returns a value of the specified attribute from the provided message
        /// </summary>
        /// <param name="message">A <see cref="string"/> representation of the message.</param>
        /// <param name="attributeName">The name of the attribute whose value is to be extracted.</param>
        /// <returns>The value of the specified attribute or a null reference if value could not be determined</returns>
        private static string ExtractAttributeValue(string message, string attributeName)
        {
            Guard.Argument(!string.IsNullOrEmpty(message));
            Guard.Argument(!string.IsNullOrEmpty(attributeName));

            var startIndex = message.IndexOf(attributeName, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return null;
            }

            var searchStartIndex = startIndex + attributeName.Length;
            var quoteIndex = message.IndexOf(Quote, searchStartIndex, StringComparison.Ordinal);

            return quoteIndex <= searchStartIndex
                ? null
                : message.Substring(searchStartIndex, quoteIndex - searchStartIndex);
        }


        /// <summary>
        /// Constructs and returns a <see cref="BasicMessageData" /> specifying the basic data of the message
        /// </summary>
        /// <param name="messageData">The raw message data.</param>
        /// <returns>a <see cref="BasicMessageData" /> specifying the basic data of the message</returns>
        public BasicMessageData GetBasicMessageData(byte[] messageData)
        {
            if (messageData == null || !messageData.Any())
            {
                return new BasicMessageData(MessageType.UNKNOWN, null, null);
            }

            var message = Encoding.UTF8.GetString(messageData);

            var messageName = ExtractMessageName(message);
            var product = ExtractAttributeValue(message, ProductIdAttributeName);
            var eventId = ExtractAttributeValue(message, EventIdAttributeName);

            return new BasicMessageData(messageName, product, eventId);
        }


        /// <summary>
        /// Gets the <see cref="MessageType" /> member from the provided <see cref="FeedMessage" /> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage" /> instance whose type is to be determined.</param>
        /// <returns>A <see cref="MessageType" /> enum member specifying the type of the provided <see cref="FeedMessage" /></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public MessageType GetMessageTypeFromMessage(FeedMessage message)
        {
            var messageTypeName = message.GetType().Name;
            if (messageTypeName.Equals("product_down", StringComparison.InvariantCultureIgnoreCase))
            {
                messageTypeName = MessageType.PRODUCER_DOWN.ToString();
            }
            var tuple = MessageTypes.FirstOrDefault(t => t.Item1 == messageTypeName);
            if (tuple == null)
            {
                Log.Warn($"Message of type={messageTypeName} is not supported.");
                return MessageType.UNKNOWN;
            }
            return tuple.Item2;
        }
    }
}