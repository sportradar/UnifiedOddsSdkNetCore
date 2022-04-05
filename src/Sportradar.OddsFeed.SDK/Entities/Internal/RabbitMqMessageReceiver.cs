/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics;
using App.Metrics.Meter;
using App.Metrics.Timer;
using Dawn;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A <see cref="IMessageReceiver"/> implementation using RabbitMQ broker to deliver feed messages
    /// </summary>
    internal sealed class RabbitMqMessageReceiver : IMessageReceiver
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
        /// <param name="deserializer">A <see cref="IDeserializer{T}"/> instance used for deserialization of messages received from the feed</param>
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

            var receivedAt = SdkInfo.ToEpochTime(DateTime.Now);

            // NOT used for GetRawMessage()
            var sessionName = _interest == null ? "system" : _interest.Name;
            string messageBody = null;
            FeedMessage feedMessage;
            IProducer producer;
            string messageName;
            try
            {
                using (var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(new TimerOptions { Context = "FEED", Name = "Message deserialization time", MeasurementUnit = Unit.Items }))
                {
                    t.TrackUserValue(eventArgs.RoutingKey);
                    messageBody = Encoding.UTF8.GetString(eventArgs.Body);
                    feedMessage = _deserializer.Deserialize(new MemoryStream(eventArgs.Body));
                    producer = _producerManager.Get(feedMessage.ProducerId);
                    messageName = feedMessage.GetType().Name;
                    if (!string.IsNullOrEmpty(feedMessage.EventId) && URN.TryParse(feedMessage.EventId, out URN eventUrn))
                    {
                        feedMessage.EventURN = eventUrn;
                    }
                    if (_keyParser.TryGetSportId(eventArgs.RoutingKey, messageName, out var sportId))
                    {
                        feedMessage.SportId = sportId;
                    }

                    if (t.Elapsed.TotalMilliseconds > 300)
                    {
                        var marketCounts = 0;
                        var outcomeCounts = 0;
                        if (feedMessage is odds_change oddsChange)
                        {
                            marketCounts = oddsChange.odds?.market?.Length ?? 0;
                            outcomeCounts = oddsChange.odds?.market?.Where(w => w.outcome != null).SelectMany(list => list.outcome).Count() ?? 0;
                        }
                        if (feedMessage is bet_settlement betSettlement)
                        {
                            marketCounts = betSettlement.outcomes?.Length ?? 0;
                            outcomeCounts = betSettlement.outcomes?.Where(w => w.Items != null).SelectMany(list => list.Items).Count() ?? 0;
                        }
                        ExecutionLog.LogDebug($"Deserialization of {feedMessage.GetType().Name} for {feedMessage.EventId} ({feedMessage.GeneratedAt}) and sport {sportId} took {t.Elapsed.TotalMilliseconds}ms. Markets={marketCounts}, Outcomes={outcomeCounts}");
                    }
                }

                if (producer.IsAvailable && !producer.IsDisabled)
                {
                    FeedLog.LogInformation($"<~> {sessionName} <~> {eventArgs.RoutingKey} <~> {messageBody}");
                }
                else
                {
                    if (FeedLog.IsEnabled(LogLevel.Debug))
                    {
                        FeedLog.LogDebug($"<~> {sessionName} <~> {eventArgs.RoutingKey} <~> {producer.Id}");
                    }
                }

                if (eventArgs.BasicProperties?.Headers != null)
                {
                    feedMessage.SentAt = eventArgs.BasicProperties.Headers.ContainsKey("timestamp_in_ms")
                                             ? long.Parse(eventArgs.BasicProperties.Headers["timestamp_in_ms"].ToString())
                                             : feedMessage.GeneratedAt;
                }
                feedMessage.ReceivedAt = receivedAt;
                SdkMetricsFactory.MetricsRoot.Measure.Meter.Mark(new MeterOptions { Context = "FEED", Name = "Message received" }, MetricTags.Empty, messageName);
            }
            catch (DeserializationException ex)
            {
                ExecutionLog.LogError(ex, $"Failed to parse message. RoutingKey={eventArgs.RoutingKey} Message: {messageBody}");
                SdkMetricsFactory.MetricsRoot.Measure.Meter.Mark(new MeterOptions { Context = "FEED", Name = "Message deserialization exception" }, MetricTags.Empty, eventArgs.RoutingKey);
                RaiseDeserializationFailed(eventArgs.Body);
                return;
            }
            catch (Exception ex)
            {
                ExecutionLog.LogError(ex, $"Error consuming feed message. RoutingKey={eventArgs.RoutingKey} Message: {messageBody}");
                SdkMetricsFactory.MetricsRoot.Measure.Meter.Mark(new MeterOptions { Context = "FEED", Name = "Exception consuming feed message" }, MetricTags.Empty, eventArgs.RoutingKey);
                RaiseDeserializationFailed(eventArgs.Body);
                return;
            }

            // send RawFeedMessage if needed
            try
            {
                if (producer.IsAvailable && !producer.IsDisabled)
                {
                    //ExecutionLog.LogDebug($"Raw msg [{_interest}]: {feedMessage.GetType().Name} for event {feedMessage.EventId}.");
                    var args = new RawFeedMessageEventArgs(eventArgs.RoutingKey, feedMessage, sessionName);
                    RawFeedMessageReceived?.Invoke(this, args);
                }
            }
            catch (Exception e)
            {
                ExecutionLog.LogError(e, $"Error dispatching raw message for {feedMessage.EventId}");
            }
            // continue normal processing

            if (!_producerManager.Exists(feedMessage.ProducerId))
            {
                ExecutionLog.LogWarning($"A message for producer which is not defined was received. Producer={feedMessage.ProducerId}");
                return;
            }

            if (!_useReplay && (!producer.IsAvailable || producer.IsDisabled))
            {
                ExecutionLog.LogDebug($"A message for producer which is disabled was received. Producer={producer}, MessageType={messageName}");
                return;
            }

            ExecutionLog.LogInformation($"Message received. Message=[{feedMessage}].");
            if (feedMessage.IsEventRelated)
            {
                if (!string.IsNullOrEmpty(eventArgs.RoutingKey) && _keyParser.TryGetSportId(eventArgs.RoutingKey, messageName, out var sportId))
                {
                    feedMessage.SportId = sportId;
                }
                else
                {
                    ExecutionLog.LogWarning($"Failed to parse the SportId from the routing key. RoutingKey={eventArgs.RoutingKey}, message=[{feedMessage}]. SportId will not be set.");
                }
            }

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
            _channel.Open(interest, routingKeys);
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
