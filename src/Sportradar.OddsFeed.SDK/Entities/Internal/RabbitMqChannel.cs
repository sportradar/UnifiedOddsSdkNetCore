/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A class used to connect to the RabbitMQ broker
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

        private List<string> _routingKeys;

        private DateTime _lastMessageReceived;

        /// <summary>
        /// A <see cref="ITimer"/> instance used to trigger periodic connection test
        /// </summary>
        private readonly ITimer _timer;

        private readonly TimeSpan _maxTimeBetweenMessages;

        /// <summary>
        /// The timer semaphore slim used reduce concurrency within timer calls
        /// </summary>
        private readonly SemaphoreSlim _timerSemaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqChannel"/> class.
        /// </summary>
        /// <param name="channelFactory">A <see cref="IChannelFactory"/> used to construct the <see cref="IModel"/> representing Rabbit MQ channel.</param>
        /// <param name="timer">Timer used to check if there is connection</param>
        /// <param name="maxTimeBetweenMessages">Max timeout between messages to check if connection is ok</param>
        public RabbitMqChannel(IChannelFactory channelFactory, ITimer timer, TimeSpan maxTimeBetweenMessages)
        {
            Guard.Argument(channelFactory, nameof(channelFactory)).NotNull();

            _channelFactory = channelFactory;
            _routingKeys = new List<string>();
            _lastMessageReceived = DateTime.MinValue;

            _timer = timer;
            _maxTimeBetweenMessages = maxTimeBetweenMessages;
            _timer.Elapsed += OnTimerElapsed;
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

            _routingKeys = routingKeys.ToList();
            if (_routingKeys.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(routingKeys), "Missing routing keys");
            }
            
            _timer.Start();
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

            DetachEvents();

            _timer.Stop();
            _timerSemaphoreSlim.Release();
            _timerSemaphoreSlim.Dispose();
        }

        /// <summary>
        /// Handles the <see cref="EventingBasicConsumer.Received"/> event
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the instance raising the event.</param>
        /// <param name="basicDeliverEventArgs">The <see cref="BasicDeliverEventArgs"/> instance containing the event data.</param>
        private void ConsumerOnDataReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            _lastMessageReceived = DateTime.Now;
            Received?.Invoke(this, basicDeliverEventArgs);
        }

        private void ConsumerOnShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ExecutionLog.LogInformation($"The consumer {_consumer.ConsumerTag} is shutdown.");
        }

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ExecutionLog.LogInformation($"The channel with channelNumber {_channel.ChannelNumber} is shutdown.");
        }

        private void CreateAndAttachEvents()
        {
            ExecutionLog.LogInformation("Opening the channel ...");
            _channel = _channelFactory.CreateChannel();
            ExecutionLog.LogInformation($"Channel opened with channelNumber: {_channel.ChannelNumber}");
            var declareResult = _channel.QueueDeclare();

            foreach (var routingKey in _routingKeys)
            {
                ExecutionLog.LogInformation($"Binding queue={declareResult.QueueName} with routingKey={routingKey}");
                _channel.QueueBind(declareResult.QueueName, "unifiedfeed", routingKey);
            }

            _channel.ModelShutdown += ChannelOnModelShutdown;
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += ConsumerOnDataReceived;
            _consumer.Shutdown += ConsumerOnShutdown;
            _channel.BasicConsume(declareResult.QueueName, true, _consumer);

            _lastMessageReceived = DateTime.Now;
        }

        private void DetachEvents()
        {
            ExecutionLog.LogInformation($"Closing the channel with channelNumber: {_channel?.ChannelNumber}");
            if (_consumer != null)
            {
                _consumer.Received -= ConsumerOnDataReceived;
                _consumer.Shutdown -= ConsumerOnShutdown;
                _consumer = null;
            }
            if (_channel != null)
            {
                _channel.ModelShutdown -= ChannelOnModelShutdown;
                _channel.Dispose();
                _channel = null;
            }
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            Task.Run(async () => {
                await OnTimerElapsedAsync().ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Invoked when the internally used timer elapses
        /// </summary>
        private async Task OnTimerElapsedAsync()
        {
            await _timerSemaphoreSlim.WaitAsync().ConfigureAwait(false);
            if (IsOpened)
            {
                try
                {
                    if (_channel == null)
                    {
                        CreateAndAttachEvents();
                    }

                    if(_channelFactory.ConnectionCreated > _lastMessageReceived)
                    {
                        // it means, the connection was reseted in between
                        DetachEvents();
                        CreateAndAttachEvents();
                    }

                    var lastMessageDiff = DateTime.Now - _lastMessageReceived;
                    if (_lastMessageReceived > DateTime.MinValue && lastMessageDiff > _maxTimeBetweenMessages)
                    {
                        var isOpen = _channelFactory.IsConnectionOpen() ? "s" : string.Empty;
                        ExecutionLog.LogWarning($"There were no message{isOpen} in more then {_maxTimeBetweenMessages.TotalSeconds}s for the channel with channelNumber: {_channel?.ChannelNumber}. Last message arrived: {_lastMessageReceived}");
                        DetachEvents();
                        ExecutionLog.LogInformation($"Resetting connection for the channel with channelNumber: {_channel?.ChannelNumber}");
                        _channelFactory.ResetConnection();
                        ExecutionLog.LogInformation($"Resetting connection finished for the channel with channelNumber: {_channel?.ChannelNumber}");
                        CreateAndAttachEvents();
                    }
                }
                catch (Exception e)
                {
                    ExecutionLog.LogWarning("Error checking connection: " + e.Message);
                }
            }
            
            _timerSemaphoreSlim.Release();
        }
    }
}