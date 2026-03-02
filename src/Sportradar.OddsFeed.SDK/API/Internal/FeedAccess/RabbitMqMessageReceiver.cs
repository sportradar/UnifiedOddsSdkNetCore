// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Dawn;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    /// <summary>
    /// A <see cref="IMessageReceiver"/> implementation using RabbitMQ broker to deliver feed messages
    /// </summary>
    internal sealed class RabbitMqMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// A <see cref="ILogger"/> used for execution logging
        /// </summary>
        private readonly ILogger _executionLog;

        /// <summary>
        /// A <see cref="ILogger"/> used for feed traffic logging
        /// </summary>
        private readonly ILogger _feedLog;

        /// <summary>
        /// The message interest associated with the session using this instance
        /// </summary>
        private MessageInterest _interest;

        /// <summary>
        /// A <see cref="IRabbitMqChannel"/> representing a channel to the RabbitMQ broker
        /// </summary>
        private readonly IRabbitMqChannel _channel;

        /// <summary>
        /// A <see cref="IDeserializer{T}"/> instance used for deserialization of messages received from the feed
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
        /// Gets a value indicating whether the current <see cref="IRabbitMqChannel"/> is currently opened;
        /// </summary>
        public bool IsChannelOpened => _channel.IsOpened;

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
        /// <param name="deserializer">A <see cref="IDeserializer{T}"/> instance used for deserialization of messages received from the feed</param>
        /// <param name="keyParser">A <see cref="IRoutingKeyParser"/> used to parse the rabbit's routing key</param>
        /// <param name="producerManager">An <see cref="IProducerManager"/> used to get <see cref="IProducer"/></param>
        /// <param name="configuration">Used to check if it should connect to the replay server</param>
        /// <param name="loggerFactory">The logger factory</param>
        public RabbitMqMessageReceiver(IRabbitMqChannel channel,
                                       IDeserializer<FeedMessage> deserializer,
                                       IRoutingKeyParser keyParser,
                                       IProducerManager producerManager,
                                       IUofConfiguration configuration,
                                       ILoggerFactory loggerFactory)
        {
            _ = Guard.Argument(channel, nameof(channel)).NotNull();
            _ = Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            _ = Guard.Argument(keyParser, nameof(keyParser)).NotNull();
            _ = Guard.Argument(producerManager, nameof(producerManager)).NotNull();
            _ = Guard.Argument(configuration, nameof(configuration)).NotNull();
            _ = Guard.Argument(loggerFactory, nameof(loggerFactory)).NotNull();

            _channel = channel;
            _deserializer = deserializer;
            _keyParser = keyParser;
            _producerManager = producerManager;
            _useReplay = configuration.Environment == SdkEnvironment.Replay;
            _executionLog = loggerFactory.CreateLogger<RabbitMqMessageReceiver>();
            _feedLog = loggerFactory.CreateLogger<FeedTraffic>();
        }

        /// <summary>
        /// Opens the current <see cref="RabbitMqMessageReceiver"/> instance so it starts receiving messages
        /// </summary>
        /// <param name="routingKeys">A list of routing keys specifying which messages should the <see cref="RabbitMqMessageReceiver"/> deliver</param>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session using this instance</param>
        public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
        {
            _interest = interest;
            _channel.Open(interest, routingKeys);
            _channel.Received += OnChannelMessageReceived;
        }

        /// <summary>
        /// Closes the current <see cref="RabbitMqMessageReceiver"/> so it will no longer receive messages
        /// </summary>
        public void Close()
        {
            _channel.Received -= OnChannelMessageReceived;
            _channel.Close();
        }

        /// <summary>
        /// Handles the message received event
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the event sender - not used in the method</param>
        /// <param name="eventArgs">A <see cref="BasicDeliverEventArgs"/> containing event information</param>
        private void OnChannelMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            if (eventArgs.Body.IsEmpty)
            {
                _executionLog.LogWarning("A message with empty body received. Aborting message processing");
                return;
            }

            var receivedAt = SdkInfo.ToEpochTime(DateTime.Now);
            var sessionName = _interest == null ? "system" : _interest.Name;

            var deserializationResult = TryDeserializeMessage(eventArgs, receivedAt);
            if (deserializationResult == null)
            {
                return;
            }

            var (feedMessage, producer, messageName, messageBody) = deserializationResult.Value;

            LogFeedTraffic(sessionName, eventArgs.RoutingKey, messageBody, producer);

            DispatchRawFeedMessageIfSubscribed(eventArgs, producer, feedMessage, sessionName);

            bool isMessageValidForProducer;

            if (_useReplay)
            {
                isMessageValidForProducer = ValidateProducerExists(feedMessage, producer, messageName);
            }
            else
            {
                isMessageValidForProducer = ValidateProducerExists(feedMessage, producer, messageName) && ValidateProducerIsActive(producer, messageName);
            }

            if (!isMessageValidForProducer)
            {
                return;
            }

            _executionLog.LogInformation("Message received. Message=[{FeedMessage}]", feedMessage);

            RaiseMessageReceived(feedMessage, eventArgs.Body.ToArray());
        }

        /// <summary>
        /// Attempts to deserialize the message from the event args
        /// </summary>
        /// <param name="eventArgs">The event arguments containing the message data</param>
        /// <param name="receivedAt">The timestamp when the message was received</param>
        /// <returns>A tuple containing the deserialized message, producer, message name, and body, or null if deserialization failed</returns>
        private (FeedMessage feedMessage, IProducer producer, string messageName, string messageBody)? TryDeserializeMessage(BasicDeliverEventArgs eventArgs, long receivedAt)
        {
            string messageBody = null;
            try
            {
                using (var tracker = new TelemetryTracker(UofSdkTelemetry.RabbitMessageReceiverMessageDeserializationTime))
                {
                    var bodyBytes = eventArgs.Body.ToArray();
                    messageBody = Encoding.UTF8.GetString(bodyBytes);
                    var feedMessage = _deserializer.Deserialize(new MemoryStream(bodyBytes));
                    feedMessage.ReceivedAt = receivedAt;

                    var producer = _producerManager.GetProducer(feedMessage.ProducerId);
                    var messageName = feedMessage.GetType().Name;

                    TryEnrichFeedMessageWithEventUrnAndSportId(feedMessage, eventArgs.RoutingKey, messageName);
                    LogLongFeedMessageDeserialization(tracker, feedMessage, feedMessage.SportId);

                    SetFeedMessageSentAtFromBasicPropertiesHeader(eventArgs, feedMessage);
                    UofSdkTelemetry.RabbitMessageReceiverMessageReceived.Add(1, new KeyValuePair<string, object>("FeedMessageType", messageName));

                    return (feedMessage, producer, messageName, messageBody);
                }
            }
            catch (DeserializationException ex)
            {
                _executionLog.LogError(ex, "Failed to parse message. RoutingKey={RoutingKey} Message: {MessageBody}", eventArgs.RoutingKey, messageBody);
                UofSdkTelemetry.RabbitMessageReceiverDeserializationException.Add(1);
                RaiseDeserializationFailed(eventArgs.Body.ToArray());
                return null;
            }
            catch (Exception ex)
            {
                _executionLog.LogError(ex, "Error consuming feed message. RoutingKey={RoutingKey} Message: {MessageBody}", eventArgs.RoutingKey, messageBody);
                UofSdkTelemetry.RabbitMessageReceiverConsumingException.Add(1);
                RaiseDeserializationFailed(eventArgs.Body.ToArray());
                return null;
            }
        }

        /// <summary>
        /// Enriches the feed message with EventUrn and SportId during deserialization
        /// </summary>
        /// <param name="feedMessage">The feed message to enrich</param>
        /// <param name="routingKey">The routing key from the message</param>
        /// <param name="messageName">The name of the message type</param>
        private void TryEnrichFeedMessageWithEventUrnAndSportId(FeedMessage feedMessage, string routingKey, string messageName)
        {
            if (!feedMessage.IsEventRelated)
            {
                return;
            }

            if (!string.IsNullOrEmpty(feedMessage.EventId) && Urn.TryParse(feedMessage.EventId, out var eventUrn))
            {
                feedMessage.EventUrn = eventUrn;
            }

            if (!string.IsNullOrEmpty(routingKey) && _keyParser.TryGetSportId(routingKey, messageName, out var sportId))
            {
                feedMessage.SportId = sportId;
            }
            else
            {
                _executionLog.LogWarning("Failed to parse the SportId from the routing key. SportId will not be set. RoutingKey={RoutingKey}, message=[{FeedMessage}]", routingKey, feedMessage);
            }
        }

        /// <summary>
        /// Logs the feed traffic based on producer status
        /// </summary>
        /// <param name="sessionName">The name of the session</param>
        /// <param name="routingKey">The routing key of the message</param>
        /// <param name="messageBody">The message body</param>
        /// <param name="producer">The producer</param>
        private void LogFeedTraffic(string sessionName, string routingKey, string messageBody, IProducer producer)
        {
            if (producer.IsAvailable && !producer.IsDisabled)
            {
                _feedLog.LogInformation("<~> {SessionName} <~> {RoutingKey} <~> {MessageBody}", sessionName, routingKey, messageBody);
            }
            else if (_feedLog.IsEnabled(LogLevel.Debug))
            {
                _feedLog.LogDebug("<~> {SessionName} <~> {RoutingKey} <~> {ProducerId}", sessionName, routingKey, producer.Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        private bool ValidateProducerExists(FeedMessage feedMessage, IProducer producer, string messageName)
        {
            if (_producerManager.Exists(feedMessage.ProducerId))
            {
                return true;
            }
            _executionLog.LogWarning("A message for producer which is not defined was received. Producer={Producer}, MessageType={FeedMessageType}",
                                     producer,
                                     messageName);
            return false;
        }

        private bool ValidateProducerIsActive(IProducer producer, string messageName)
        {
            if (producer.IsAvailable && !producer.IsDisabled)
            {
                return true;
            }
            _executionLog.LogDebug("A message for producer which is disabled was received. Producer={Producer}, MessageType={FeedMessageType}", producer, messageName);
            return false;

        }

        private void DispatchRawFeedMessageIfSubscribed(BasicDeliverEventArgs eventArgs, IProducer producer, FeedMessage feedMessage, string sessionName)
        {
            try
            {
                if (!producer.IsAvailable || producer.IsDisabled || RawFeedMessageReceived == null)
                {
                    return;
                }
                using (new TelemetryTracker(UofSdkTelemetry.RabbitMessageReceiverRawMessageDispatched))
                {
                    var args = new RawFeedMessageEventArgs(eventArgs.RoutingKey, feedMessage, sessionName);
                    RawFeedMessageReceived.Invoke(this, args);
                }
            }
            catch (Exception e)
            {
                _executionLog.LogError(e, "Error dispatching raw message for {EventId}", feedMessage.EventId);
            }
        }

        private void LogLongFeedMessageDeserialization(TelemetryTracker tracker, FeedMessage feedMessage, Urn sportId)
        {
            if (tracker.Elapsed.TotalMilliseconds < 300)
            {
                return;
            }
            var marketCounts = 0;
            var outcomeCounts = 0;
            switch (feedMessage)
            {
                case odds_change oddsChange:
                    marketCounts = oddsChange.odds?.market?.Length ?? 0;
                    outcomeCounts = oddsChange.odds?.market?.Where(w => w.outcome != null).SelectMany(list => list.outcome).Count() ?? 0;
                    break;
                case bet_settlement betSettlement:
                    marketCounts = betSettlement.outcomes?.Length ?? 0;
                    outcomeCounts = betSettlement.outcomes?.Where(w => w.Items != null).SelectMany(list => list.Items).Count() ?? 0;
                    break;
            }
            _executionLog.LogDebug("Deserialization of {FeedMessageType} for {EventId} ({GeneratedAt}) and sport {SportId} took {Elapsed}ms. Markets={MarketCount}, Outcomes={OutcomeCount}",
                                   feedMessage.GetType().Name,
                                   feedMessage.EventId,
                                   feedMessage.GeneratedAt.ToString(CultureInfo.InvariantCulture),
                                   sportId,
                                   tracker.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                                   marketCounts.ToString(CultureInfo.InvariantCulture),
                                   outcomeCounts.ToString(CultureInfo.InvariantCulture));
        }

        private void SetFeedMessageSentAtFromBasicPropertiesHeader(BasicDeliverEventArgs eventArgs, FeedMessage feedMessage)
        {
            if (eventArgs.BasicProperties != null
                && eventArgs.BasicProperties.IsHeadersPresent()
                && eventArgs.BasicProperties.Headers.TryGetValue("timestamp_in_ms", out var headerTimestamp)
                && long.TryParse(headerTimestamp.ToString(), out var timestamp))
            {
                feedMessage.SentAt = timestamp;
            }
            else
            {
                _executionLog.LogDebug("{FeedMessageType} for {EventId} ({GeneratedAt}) is missing sent message header", feedMessage.GetType().Name, feedMessage.EventId, feedMessage.GeneratedAt.ToString(CultureInfo.InvariantCulture));
                feedMessage.SentAt = feedMessage.GeneratedAt + 1;
            }
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            //no need to add interest, it is later used from session interest
            FeedMessageReceived?.Invoke(this, new FeedMessageReceivedEventArgs(message, null, rawMessage));
        }
    }
}
