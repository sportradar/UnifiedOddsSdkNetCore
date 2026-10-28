// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    /// <summary>
    /// A class used to connect to the RabbitMQ broker
    /// </summary>
    /// <seealso cref="IRabbitMqChannel" />
    internal class RabbitMqChannel : IRabbitMqChannel
    {
        private readonly ILogger _logger;

        internal readonly IChannelFactory ChannelFactory;

        internal IModel Channel;

        internal EventingBasicConsumer BasicConsumer;

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

        private MessageInterest _messageInterest;

        private List<string> _routingKeys;

        internal DateTime ChannelStarted;

        internal DateTime LastMessageReceived;

        /// <summary>
        /// A <see cref="ISdkTimer"/> instance used to trigger periodic connection test
        /// </summary>
        private readonly ISdkTimer _timer;

        private readonly TimeSpan _maxTimeBetweenMessages;

        private readonly string _accessToken;

        private SemaphoreSlim _timerSemaphoreSlim;

        internal SemaphoreSlim TimerSemaphore
        {
            get => _timerSemaphoreSlim;
            set => _timerSemaphoreSlim = value;
        }

        public RabbitMqChannel(IChannelFactory channelFactory, ISdkTimer timer, TimeSpan maxTimeBetweenMessages, string accessToken, ILogger<RabbitMqChannel> logger)
        {
            _ = Guard.Argument(channelFactory, nameof(channelFactory)).NotNull();
            _ = Guard.Argument(logger, nameof(logger)).NotNull();

            ChannelFactory = channelFactory;
            _routingKeys = new List<string>();
            ChannelStarted = DateTime.MinValue;
            LastMessageReceived = DateTime.MinValue;
            _logger = logger;

            _timer = timer;
            _maxTimeBetweenMessages = maxTimeBetweenMessages;
            _accessToken = accessToken;
            _timer.Elapsed += OnTimerElapsed;
        }

        /// <summary>
        /// Opens the current channel and binds the created queue to provided routing keys
        /// </summary>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session using this instance</param>
        /// <param name="routingKeys">A <see cref="IEnumerable{String}"/> specifying the routing keys of the constructed queue.</param>
        public void Open(MessageInterest interest, IEnumerable<string> routingKeys)
        {
            var keys = routingKeys?.ToList();
            if (keys.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(routingKeys), "Missing routing keys");
            }

            if (Interlocked.CompareExchange(ref _isOpened, 1, 0) != 0)
            {
                _logger.LogError("Opening an already opened channel is not allowed");
                throw new InvalidOperationException("The instance is already opened");
            }

            _timerSemaphoreSlim = new SemaphoreSlim(1);
            _messageInterest = interest;
            _routingKeys = keys;
            try
            {
                _timer.Start();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error starting the channel timer");
                Interlocked.Exchange(ref _isOpened, 0);
                StopTimerAndReleaseSemaphore();
                throw;
            }
        }

        /// <summary>
        /// Closes the current channel
        /// </summary>
        /// <exception cref="InvalidOperationException">The instance is already closed</exception>
        public void Close()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 0, 1) != 1)
            {
                _logger.LogError("Cannot close the channel on channelNumber: {ChannelNumber}, because this channel is already closed", TryReadChannelNumber());
                throw new InvalidOperationException("The instance is already closed");
            }

            try
            {
                DetachEventsAndCloseChannel();
            }
            finally
            {
                StopTimerAndReleaseSemaphore();
            }
        }

        /// <summary>
        /// Handles the <see cref="EventingBasicConsumer.Received"/> event
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the instance raising the event.</param>
        /// <param name="basicDeliverEventArgs">The <see cref="BasicDeliverEventArgs"/> instance containing the event data.</param>
        private void BasicConsumerOnDataReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            LastMessageReceived = DateTime.Now;
            Received?.Invoke(this, basicDeliverEventArgs);
        }

        private void BasicConsumerOnShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            var reason = shutdownEventArgs.ReplyText == null ? string.Empty : ClearAccessTokenFrom(shutdownEventArgs.ReplyText);
            _logger.LogInformation("The consumer is shutdown. ConsumerTag={ConsumerTag}, Reason={ErrorMessage}", BasicConsumer?.ConsumerTags, reason);
        }

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            var reason = shutdownEventArgs.ReplyText == null ? string.Empty : ClearAccessTokenFrom(shutdownEventArgs.ReplyText);
            _logger.LogInformation("The channel is shutdown. ChannelNumber={ChannelNumber}, Reason={ErrorMessage}", TryReadChannelNumber(), reason);
        }

        private void CreateChannelAndAttachEvents()
        {
            _logger.LogInformation("Opening the channel ...");
            Channel = ChannelFactory.CreateChannel();
            _logger.LogInformation("Channel opened with channelNumber: {ChannelNumber}. MaxAllowedTimeBetweenMessages={MaxTimeBetweenMessages}s",
                                   Channel.ChannelNumber.ToString(CultureInfo.InvariantCulture),
                                   _maxTimeBetweenMessages.TotalSeconds.ToString(CultureInfo.InvariantCulture));

            var queueName = BindChannelWithRoutingKeys();

            var interestName = _messageInterest == null ? "system" : _messageInterest.Name;
            var consumerTag = $"UfSdk-{SdkInfo.SdkType}|{SdkInfo.GetVersion()}|{Environment.OSVersion}|{Environment.Version}|{interestName}|{Channel.ChannelNumber.ToString()}|{DateTime.UtcNow:yyyyMMdd-HHmmss}|{SdkInfo.GetGuid(8)}";
            BasicConsumer = new EventingBasicConsumer(Channel);
            BasicConsumer.Received += BasicConsumerOnDataReceived;
            BasicConsumer.Shutdown += BasicConsumerOnShutdown;
            Channel.BasicConsume(queueName, true, consumerTag, BasicConsumer);
            Channel.ModelShutdown += ChannelOnModelShutdown;
            _logger.LogInformation("BasicConsume for channel={ChannelNumber}, queue={ChannelQueueName} and consumer tag {ConsumerTag} executed", Channel.ChannelNumber.ToString(), queueName, consumerTag);

            LastMessageReceived = DateTime.MinValue;
            ChannelStarted = DateTime.Now;
        }

        private string BindChannelWithRoutingKeys()
        {
            var declareResult = Channel.QueueDeclare();

            foreach (var routingKey in _routingKeys)
            {
                _logger.LogInformation("Binding queue={ChannelQueueName} with routingKey={RoutingKey}", declareResult.QueueName, routingKey);
                Channel.QueueBind(declareResult.QueueName, "unifiedfeed", routingKey);
            }

            return declareResult.QueueName;
        }

        private void DetachEventsAndCloseChannel()
        {
            var channelNumber = TryReadChannelNumber();
            _logger.LogInformation("Closing the channel with channelNumber: {ChannelNumber}", channelNumber);
            if (BasicConsumer != null)
            {
                BasicConsumer.Received -= BasicConsumerOnDataReceived;
                BasicConsumer.Shutdown -= BasicConsumerOnShutdown;
                BasicConsumer = null;
            }

            var channel = Channel;
            Channel = null;
            ChannelStarted = DateTime.MinValue;
            if (channel == null)
            {
                return;
            }

            try
            {
                channel.ModelShutdown -= ChannelOnModelShutdown;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error detaching channel shutdown on channelNumber: {ChannelNumber}", channelNumber);
            }

            try
            {
                channel.Close();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error closing channel on channelNumber: {ChannelNumber}", channelNumber);
            }

            try
            {
                channel.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error disposing channel on channelNumber: {ChannelNumber}", channelNumber);
            }
        }

        private void StopTimerAndReleaseSemaphore()
        {
            try
            {
                _timer.Stop();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error stopping the channel timer");
            }

            var semaphore = _timerSemaphoreSlim;
            _timerSemaphoreSlim = null;
            semaphore?.ReleaseSafe();
            try
            {
                semaphore?.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error disposing the channel timer semaphore");
            }
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            Task.Run(OnTimerElapsedAsync);
        }

        /// <summary>
        /// Invoked when the internally used timer elapses
        /// </summary>
        private async Task OnTimerElapsedAsync()
        {
            var semaphore = _timerSemaphoreSlim;
            if (semaphore == null)
            {
                _logger.LogDebug("Connection check skipped because the channel is not opened");
                return;
            }

            try
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
                _logger.LogDebug("Connection check skipped because the channel is closed");
                return;
            }

            try
            {
                if (IsOpened)
                {
                    try
                    {
                        RecreateChannelIfNoMessagesArrivedWithinInterval();

                        RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStart();

                        RecreateChannelIfConnectionIsNewer();

                        CreateChannelIfDoesNotExist();
                    }
                    catch (Exception e)
                    {
                        var channelNumber = TryReadChannelNumber();
                        if (Channel != null && ChannelStarted == DateTime.MinValue)
                        {
                            DetachEventsAndCloseChannel();
                        }

                        _logger.LogWarning(e, "Error checking connection for channelNumber: {ChannelNumber}", channelNumber);
                    }
                }
            }
            finally
            {
                semaphore.ReleaseSafe();
            }
        }

        private string TryReadChannelNumber()
        {
            try
            {
                return Channel?.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        private void CreateChannelIfDoesNotExist()
        {
            if (Channel == null)
            {
                _logger.LogInformation("Creating connection channel and attaching events ...");
                CreateChannelAndAttachEvents();
            }
        }

        private void RecreateChannelIfConnectionIsNewer()
        {
            // it means, the connection was reset in between
            if (ChannelFactory.ConnectionCreated > ChannelStarted)
            {
                _logger.LogInformation("Recreating connection channel and attaching events ...");
                DetachEventsAndCloseChannel();
                CreateChannelAndAttachEvents();
            }
        }

        private void RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStart()
        {
            // no messages arrived in last _maxTimeBetweenMessages seconds, from the start of the channel
            var channelStartedDiff = DateTime.Now - ChannelStarted;
            if (LastMessageReceived != DateTime.MinValue
                || ChannelStarted == DateTime.MinValue
                || channelStartedDiff <= _maxTimeBetweenMessages)
            {
                return;
            }

            var isOpen = ChannelFactory.IsConnectionOpen() ? "s" : string.Empty;
            _logger.LogWarning("There were no message{Text} in more than {MaxTimeBetweenMessages}s for the channel with channelNumber: {ChannelNumber}. Last message arrived: {LastMessageReceived}. Recreating channel ...",
                               isOpen,
                               _maxTimeBetweenMessages.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                               Channel?.ChannelNumber.ToString(CultureInfo.InvariantCulture),
                               LastMessageReceived.ToString(CultureInfo.InvariantCulture));
            DetachEventsAndCloseChannel();
            CreateChannelAndAttachEvents();
        }

        private void RecreateChannelIfNoMessagesArrivedWithinInterval()
        {
            // we have received messages in the past, but not in last _maxTimeBetweenMessages seconds
            var lastMessageDiff = DateTime.Now - LastMessageReceived;
            if (LastMessageReceived == DateTime.MinValue || lastMessageDiff <= _maxTimeBetweenMessages)
            {
                return;
            }
            var isOpen = ChannelFactory.IsConnectionOpen() ? "s" : string.Empty;
            _logger.LogWarning("There were no message{Text} in more than {MaxTimeBetweenMessages}s for the channel with channelNumber: {ChannelNumber}. Last message arrived: {LastMessageReceived}",
                               isOpen,
                               _maxTimeBetweenMessages.TotalSeconds.ToString(CultureInfo.InvariantCulture),
                               Channel?.ChannelNumber.ToString(CultureInfo.InvariantCulture),
                               LastMessageReceived.ToString(CultureInfo.InvariantCulture));

            DetachEventsAndCloseChannel();
            CreateChannelAndAttachEvents();
        }

        private string ClearAccessTokenFrom(string replyText)
        {
            return _accessToken.IsNullOrEmpty() ? replyText : SdkInfo.ClearSensitiveData(replyText, _accessToken);
        }
    }
}
