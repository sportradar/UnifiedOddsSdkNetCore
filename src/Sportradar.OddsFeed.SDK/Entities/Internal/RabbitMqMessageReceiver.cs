/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A <see cref="IMessageReceiver"/> implementation using RabbitMQ broker to deliver feed messages
    /// </summary>
    public sealed class RabbitMqMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// A <see cref="ILogger"/> used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(RabbitMqMessageReceiver));

        /// <summary>
        /// A <see cref="ILogger"/> used for feed traffic logging
        /// </summary>
        private static readonly ILogger FeedLog = SdkLoggerFactory.GetLoggerForFeedTraffic(typeof(RabbitMqMessageReceiver));

        /// <summary>
        /// The message interest associated by the session using this instance
        /// </summary>
        private MessageInterest _interest;

        /// <summary>
        /// A <see cref="IRabbitMqChannel"/> representing a channel to the RabbitMQ broker
        /// </summary>
        private readonly IRabbitMqChannel _channel;

        /// <summary>
        /// A <see cref="IDeserializer{T}"/> instance used for de-serialization of messages received from the feed
        /// </summary>
        private readonly IDeserializer<FeedMessage> _deserializer;

        /// <summary>
        /// A <see cref="IRoutingKeyParser"/> used to parse the rabbit's routing key
        /// </summary>
        private readonly IRoutingKeyParser _keyParser;

        /// <summary>
        /// A <see cref="IProducerManager"/> used to get <see cref="IProducer"/>
        /// </summary>
        private readonly IProducerManager _producerManager;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="RabbitMqMessageReceiver"/> is currently opened;
        /// </summary>
        public bool IsOpened => _channel.IsOpened;

        /// <summary>
        /// Event raised when the <see cref="IMessageReceiver"/> receives the message
        /// </summary>
        public event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;

        /// <summary>
        /// Event raised when the <see cref="IMessageReceiver"/> could not deserialize the received message
        /// </summary>
        public event EventHandler<MessageDeserializationFailedEventArgs> FeedMessageDeserializationFailed;

        /// <summary>
        /// Event raised when the <see cref="IMessageReceiver" /> receives the message
        /// </summary>
        public event EventHandler<RawFeedMessageEventArgs> RawFeedMessageReceived;

        /// <summary>
        /// Is connected to the replay server
        /// </summary>
        private readonly bool _useReplay;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqMessageReceiver"/> class
        /// </summary>
        /// <param name="channel">A <see cref="IRabbitMqChannel"/> representing a channel to the RabbitMQ broker</param>
        /// <param name="deserializer">A <see cref="IDeserializer{T}"/> instance used for de-serialization of messages received from the feed</param>
        /// <param name="keyParser">A <see cref="IRoutingKeyParser"/> used to parse the rabbit's routing key</param>
        /// <param name="producerManager">An <see cref="IProducerManager"/> used to get <see cref="IProducer"/></param>
        /// <param name="usedReplay">Is connected to the replay server</param>
        public RabbitMqMessageReceiver(IRabbitMqChannel channel, IDeserializer<FeedMessage> deserializer, IRoutingKeyParser keyParser, IProducerManager producerManager, bool usedReplay)
        {
            Guard.Argument(channel, nameof(channel)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(keyParser, nameof(keyParser)).NotNull();
            Guard.Argument(producerManager, nameof(producerManager)).NotNull();

            _channel = channel;
            _deserializer = deserializer;
            _keyParser = keyParser;
            _producerManager = producerManager;
            _useReplay = usedReplay;
        }

        /// <summary>
        /// Handles the message received event
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the event sender</param>
        /// <param name="eventArgs">A <see cref="BasicDeliverEventArgs"/> containing event information</param>
        private void consumer_OnReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            if (eventArgs.Body == null || !eventArgs.Body.Any())
            {
                var body = eventArgs.Body == null ? "null" : "empty";
                ExecutionLog.LogWarning($"A message with {body} body received. Aborting message processing");
                return;
            }

            var receivedAt =  SdkInfo.ToEpochTime(DateTime.Now);

            // NOT used for GetRawMessage()
            var sessionName = _interest == null ? "system" : _interest.Name;
            string messageBody = null;
            FeedMessage feedMessage;
            try
            {
                if (FeedLog.IsEnabled(LogLevel.Debug))
                {
                    messageBody = Encoding.UTF8.GetString(eventArgs.Body);
                    FeedLog.LogDebug($"<~> {sessionName} <~> {eventArgs.RoutingKey} <~> {messageBody}");
                }
                else
                {
                    FeedLog.LogInformation(eventArgs.RoutingKey);
                }
                feedMessage = _deserializer.Deserialize(new MemoryStream(eventArgs.Body));
                if (eventArgs.BasicProperties?.Headers != null)
                {
                    feedMessage.SentAt = eventArgs.BasicProperties.Headers.ContainsKey("timestamp_in_ms")
                                             ? long.Parse(eventArgs.BasicProperties.Headers["timestamp_in_ms"].ToString())
                                             : feedMessage.GeneratedAt;
                }
                feedMessage.ReceivedAt = receivedAt;
            }
            catch (DeserializationException ex)
            {
                ExecutionLog.LogError($"Failed to parse message. RoutingKey={eventArgs.RoutingKey} Message: {messageBody}", ex);
                //Metric.Context("RABBIT").Meter("RabbitMqMessageReceiver->DeserializationException", Unit.Calls).Mark();
                RaiseDeserializationFailed(eventArgs.Body);
                return;
            }

            // send RawFeedMessage if needed
            try
            {
                //ExecutionLog.LogDebug($"Raw msg [{_interest}]: {feedMessage.GetType().Name} for event {feedMessage.EventId}.");
                var args = new RawFeedMessageEventArgs(eventArgs.RoutingKey, feedMessage, sessionName);
                RawFeedMessageReceived?.Invoke(this, args);
            }
            catch (Exception e)
            {
                ExecutionLog.LogError($"Error dispatching raw message for {feedMessage.EventId}", e);
            }
            // continue normal processing

            if (!_producerManager.Exists(feedMessage.ProducerId))
            {
                ExecutionLog.LogWarning($"A message for producer which is not defined was received. Producer={feedMessage.ProducerId}");
                return;
            }

            var producer = _producerManager.Get(feedMessage.ProducerId);
            var messageName = feedMessage.GetType().Name;

            if (!_useReplay && (!producer.IsAvailable || producer.IsDisabled))
            {
                ExecutionLog.LogDebug($"A message for producer which is disabled was received. Producer={producer}, MessageType={messageName}");
                return;
            }

            ExecutionLog.LogInformation($"Message received. Message=[{feedMessage}].");
            if (feedMessage.IsEventRelated)
            {
                URN sportId;
                if (!string.IsNullOrEmpty(eventArgs.RoutingKey) && _keyParser.TryGetSportId(eventArgs.RoutingKey, messageName, out sportId))
                {
                    feedMessage.SportId = sportId;
                }
                else
                {
                    ExecutionLog.LogWarning($"Failed to parse the SportId from the routing key. RoutingKey={eventArgs.RoutingKey}, message=[{feedMessage}]. SportId will not be set.");
                }
            }

            //Metric.Context("RABBIT").Meter("RabbitMqMessageReceiver", Unit.Items).Mark(messageName);
            RaiseMessageReceived(feedMessage, eventArgs.Body);
        }

        /// <summary>
        /// Raises the <see cref="FeedMessageDeserializationFailed"/> event
        /// </summary>
        /// <param name="data">A <see cref="IEnumerable{Byte}"/> containing raw data of the message that could not be deserialized</param>
        private void RaiseDeserializationFailed(byte[] data)
        {
            FeedMessageDeserializationFailed?.Invoke(this, new MessageDeserializationFailedEventArgs(data));
        }

        /// <summary>
        /// Raises the <see cref="FeedMessageReceived"/> event
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage"/> instance used to construct the event args</param>
        /// <param name="rawMessage">A raw message received from the broker</param>
        private void RaiseMessageReceived(FeedMessage message, byte[] rawMessage)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            FeedMessageReceived?.Invoke(this, new FeedMessageReceivedEventArgs(message, null, rawMessage));
        }

        /// <summary>
        /// Opens the current <see cref="RabbitMqMessageReceiver"/> instance so it starts receiving messages
        /// </summary>
        /// <param name="routingKeys">A list of routing keys specifying which messages should the <see cref="RabbitMqMessageReceiver"/> deliver</param>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session using this instance</param>
        public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
        {
            _interest = interest;
            _channel.Open(routingKeys);
            _channel.Received += consumer_OnReceived;
        }

        /// <summary>
        /// Closes the current <see cref="RabbitMqMessageReceiver"/> so it will no longer receive messages
        /// </summary>
        public void Close()
        {
            _channel.Received -= consumer_OnReceived;
            _channel.Close();
        }
    }
}
