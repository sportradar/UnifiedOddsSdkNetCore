﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A class used to connect to the Rabbit mq broker
    /// </summary>
    /// <seealso cref="IRabbitMqChannel" />
    internal class RabbitMqChannel : IRabbitMqChannel
    {
        /// <summary>
        /// The <see cref="ILogger"/> used execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(RabbitMqChannel));

        /// <summary>
        /// A <see cref="IChannelFactory"/> used to construct the <see cref="IModel"/> representing Rabbit MQ channel
        /// </summary>
        private readonly IChannelFactory _channelFactory;

        /// <summary>
        /// The <see cref="IModel"/> representing the channel to the broker
        /// </summary>
        private IModel _channel;

        /// <summary>
        /// The <see cref="EventingBasicConsumer"/> used to consume the broker data
        /// </summary>
        private EventingBasicConsumer _consumer;

        /// <summary>
        /// Value indicating whether the current instance is opened. 1 means opened, 0 means closed
        /// </summary>
        private long _isOpened;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="RabbitMqMessageReceiver"/> is currently opened;
        /// </summary>
        public bool IsOpened => Interlocked.Read(ref _isOpened) == 1;

        /// <summary>
        /// Occurs when the current channel received the data
        /// </summary>
        public event EventHandler<BasicDeliverEventArgs> Received;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqChannel"/> class.
        /// </summary>
        /// <param name="channelFactory">A <see cref="IChannelFactory"/> used to construct the <see cref="IModel"/> representing Rabbit MQ channel.</param>
        public RabbitMqChannel(IChannelFactory channelFactory)
        {
            Guard.Argument(channelFactory, nameof(channelFactory)).NotNull();

            _channelFactory = channelFactory;
        }

        /// <summary>
        /// Handles the <see cref="EventingBasicConsumer.Received"/> event
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the instance raising the event.</param>
        /// <param name="basicDeliverEventArgs">The <see cref="BasicDeliverEventArgs"/> instance containing the event data.</param>
        private void OnDataReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            Received?.Invoke(this, basicDeliverEventArgs);
        }

        /// <summary>
        /// Opens the current channel and binds the created queue to provided routing keys
        /// </summary>
        /// <param name="routingKeys">A <see cref="IEnumerable{String}"/> specifying the routing keys of the constructed queue.</param>
        public void Open(IEnumerable<string> routingKeys)
        {
            if (Interlocked.CompareExchange(ref _isOpened, 1, 0) != 0)
            {
                ExecutionLog.LogError("Opening an already opened channel is not allowed");
                throw new InvalidOperationException("The instance is already opened");
            }

            Guard.Argument(routingKeys, nameof(routingKeys)).NotNull().NotEmpty();

            _channel = _channelFactory.CreateChannel();
            ExecutionLog.LogInformation($"Opening the channel with channelNumber: {_channel.ChannelNumber}.");
            var declareResult = _channel.QueueDeclare();

            foreach (var routingKey in routingKeys)
            {
                ExecutionLog.LogInformation($"Binding queue={declareResult.QueueName} with routingKey={routingKey}");
                _channel.QueueBind(declareResult.QueueName, "unifiedfeed", routingKey);
            }

            _channel.ModelShutdown += ChannelOnModelShutdown;

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += OnDataReceived;
            _consumer.Shutdown += ConsumerOnShutdown;
            _channel.BasicConsume(declareResult.QueueName, true, _consumer);
        }

        /// <summary>
        /// Closes the current channel
        /// </summary>
        /// <exception cref="InvalidOperationException">The instance is already closed</exception>
        public void Close()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 0, 1) != 1)
            {
                ExecutionLog.LogError($"Cannot close the channel on channelNumber: {_channel?.ChannelNumber}, because this channel is already closed");
                throw new InvalidOperationException("The instance is already closed");
            }

            ExecutionLog.LogInformation($"Closing the channel with channelNumber: {_channel?.ChannelNumber}.");
            if (_consumer != null)
            {
                _consumer.Received -= OnDataReceived;
                _consumer.Shutdown -= ConsumerOnShutdown;
                _consumer = null;
            }
            if (_channel != null)
            {
                _channel.ModelShutdown -= ChannelOnModelShutdown;
            }
            _channel?.Dispose();
        }

        private void ConsumerOnShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ExecutionLog.LogInformation($"The consumer: {string.Join(",", _consumer.ConsumerTags)} is shutdown.");
        }

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ExecutionLog.LogInformation($"The channel with channelNumber: {_channel.ChannelNumber} is shutdown.");
        }
    }
}