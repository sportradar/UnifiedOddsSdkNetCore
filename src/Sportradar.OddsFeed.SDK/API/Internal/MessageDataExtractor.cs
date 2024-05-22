// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Text;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// Class used to extract most basic information from raw feed message
    /// </summary>
    /// <seealso cref="IMessageDataExtractor" />
    internal class MessageDataExtractor : IMessageDataExtractor
    {
        private static readonly ILogger Log = SdkLoggerFactory.GetLogger(typeof(MessageDataExtractor));

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
            new Tuple<string, MessageType>(odds_change.MessageName, MessageType.OddsChange),
            new Tuple<string, MessageType>(bet_settlement.MessageName, MessageType.BetSettlement),
            new Tuple<string, MessageType>(rollback_bet_settlement.MessageName, MessageType.RollbackBetSettlement),
            new Tuple<string, MessageType>(bet_cancel.MessageName, MessageType.BetCancel),
            new Tuple<string, MessageType>(rollback_bet_cancel.MessageName, MessageType.RollbackBetCancel),
            new Tuple<string, MessageType>(alive.MessageName, MessageType.Alive),
            new Tuple<string, MessageType>(snapshot_complete.MessageName, MessageType.SnapshotComplete),
            new Tuple<string, MessageType>(fixture_change.MessageName, MessageType.FixtureChange),
            new Tuple<string, MessageType>(bet_stop.MessageName, MessageType.BetStop)
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
            Guard.Argument(message, nameof(message)).NotNull().NotEmpty();

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
            return MessageType.Unknown;
        }

        /// <summary>
        /// Extracts and returns a value of the specified attribute from the provided message
        /// </summary>
        /// <param name="message">A <see cref="string"/> representation of the message.</param>
        /// <param name="attributeName">The name of the attribute whose value is to be extracted.</param>
        /// <returns>The value of the specified attribute or a null reference if value could not be determined</returns>
        private static string ExtractAttributeValue(string message, string attributeName)
        {
            Guard.Argument(message, nameof(message)).NotNull().NotEmpty();
            Guard.Argument(attributeName, nameof(attributeName)).NotNull().NotEmpty();

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
                return new BasicMessageData(MessageType.Unknown, null, null);
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
        /// <exception cref="NotImplementedException"></exception>
        public MessageType GetMessageTypeFromMessage(FeedMessage message)
        {
            var messageTypeName = message.GetType().Name;
            if (messageTypeName.Equals("product_down", StringComparison.InvariantCultureIgnoreCase))
            {
                messageTypeName = MessageType.ProducerDown.ToString();
            }
            var tuple = MessageTypes.FirstOrDefault(t => t.Item1 == messageTypeName);
            if (tuple == null)
            {
                Log.LogWarning($"Message of type={messageTypeName} is not supported.");
                return MessageType.Unknown;
            }
            return tuple.Item2;
        }
    }
}
