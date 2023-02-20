/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Linq;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// System session handles system feed messages
    /// </summary>
    /// <seealso cref="IFeedSystemSession" />
    internal class FeedSystemSession : IFeedSystemSession
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        private readonly ILogger _log = SdkLoggerFactory.GetLoggerForExecution(typeof(FeedSystemSession));

        /// <summary>
        /// A value used to store information whether the current <see cref="OddsFeedSession"/> is opened
        /// 0 indicate closed, 1 indicate opened
        /// </summary>
        private long _isOpened;

        /// <summary>
        /// Value indicating whether the current instance was/is disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// A <see cref="IMessageReceiver"/> used to provide feed messages
        /// </summary>
        private readonly IMessageReceiver _messageReceiver;

        /// <summary>
        /// A <see cref="IFeedMessageValidator"/> instance used to validate received messages
        /// </summary>
        private readonly IFeedMessageValidator _messageValidator;

        /// <summary>
        /// The A <see cref="IMessageDataExtractor"/> instance used to extract basic message data from messages which could not be deserialized
        /// </summary>
        private readonly IMessageDataExtractor _messageDataExtractor;

        /// <summary>
        /// Gets a value indicating whether the current instance is opened
        /// </summary>
        /// <value><c>true</c> if this instance is opened; otherwise, <c>false</c>.</value>
        public bool IsOpened => Interlocked.Read(ref _isOpened) == 1;

        /// <summary>
        /// Raised when a alive message is received from the feed
        /// </summary>
        public event EventHandler<FeedMessageReceivedEventArgs> AliveReceived;

        /// <summary>
        /// Raised when a message which cannot be parsed is received
        /// </summary>
        public event EventHandler<UnparsableMessageEventArgs> UnparsableMessageReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="OddsFeedSession"/> class
        /// </summary>
        /// <param name="messageReceiver"> A <see cref="IMessageReceiver"/> used to provide feed messages</param>
        /// <param name="messageValidator">A <see cref="IFeedMessageValidator"/> instance used to validate received messages</param>
        /// <param name="messageDataExtractor">A <see cref="IMessageDataExtractor"/> instance used to extract basic message data from messages which could not be deserialized</param>
        public FeedSystemSession(IMessageReceiver messageReceiver, IFeedMessageValidator messageValidator, IMessageDataExtractor messageDataExtractor)
        {
            Guard.Argument(messageReceiver, nameof(messageReceiver)).NotNull();
            Guard.Argument(messageValidator, nameof(messageValidator)).NotNull();
            Guard.Argument(messageDataExtractor, nameof(messageDataExtractor)).NotNull();

            _messageReceiver = messageReceiver;
            _messageValidator = messageValidator;
            _messageDataExtractor = messageDataExtractor;
        }

        /// <summary>
        /// Processes the received <see cref="FeedMessage"/>
        /// </summary>
        /// <param name="feedMessage">The <see cref="FeedMessage"/> to process.</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void ProcessMessage(FeedMessage feedMessage, byte[] rawMessage)
        {
            if (feedMessage is alive alive)
            {
                var args = new FeedMessageReceivedEventArgs(alive, null, rawMessage);
                Dispatch(AliveReceived, args, "AliveReceived");
            }
        }

        /// <summary>
        /// Handles the <see cref="IMessageReceiver.FeedMessageReceived"/> event
        /// </summary>
        /// <param name="sender">A <see cref="object"/> representation of the instance raising the event</param>
        /// <param name="e">A <see cref="FeedMessageReceivedEventArgs"/> instance containing event information</param>
        private void OnFeedMessageReceived(object sender, FeedMessageReceivedEventArgs e)
        {
            var message = e.Message;
            var validationResult = _messageValidator.Validate(message);
            switch (validationResult)
            {
                case ValidationResult.Failure:
                    _log.LogWarning($"Validation of message=[{message}] failed. Raising OnUnparsableMessageReceived event");
                    MessageType messageType;
                    try
                    {
                        messageType = _messageDataExtractor.GetMessageTypeFromMessage(message);
                    }
                    catch (ArgumentException ex)
                    {
                        _log.LogError(ex, $"An error occurred while determining the MessageType of the message whose validation has failed. Message={message}");
                        return;
                    }
                    var eventArgs = new UnparsableMessageEventArgs(messageType, message.ProducerId.ToString(), message.EventId, e.RawMessage);
                    Dispatch(UnparsableMessageReceived, eventArgs, "OnUnparsableMessageReceived");
                    return;
                case ValidationResult.ProblemsDetected:
                    _log.LogWarning($"Problems were detected while validating message=[{message}], but the message is still eligible for further processing.");
                    return;
                case ValidationResult.Success:
                    _log.LogDebug($"Message=[{message}] successfully validated. Continuing with message processing");
                    ProcessMessage(message, e.RawMessage);
                    return;
                default:
                    _log.LogError($"ValidationResult {Enum.GetName(typeof(ValidationResult), validationResult)} is not supported. Aborting processing of message=[{message}]");
                    return;
            }
        }

        /// <summary>
        /// Handles the <see cref="IMessageReceiver.FeedMessageDeserializationFailed" /> event.
        /// </summary>
        /// <param name="sender">A <see cref="object"/> representation of the instance raising the event.</param>
        /// <param name="eventArgs">The <see cref="MessageDeserializationFailedEventArgs"/> instance containing the event data.</param>
        private void OnFeedMessageDeserializationFailed(object sender, MessageDeserializationFailedEventArgs eventArgs)
        {
            var rawData = eventArgs.RawData as byte[] ?? eventArgs.RawData.ToArray();
            var basicMessageData = _messageDataExtractor.GetBasicMessageData(rawData);
            _log.LogInformation($"Extracted the following data from unparsed message data: [{basicMessageData}], raising OnUnparsableMessageReceived event");
            var dispatchmentEventArgs = new UnparsableMessageEventArgs(basicMessageData.MessageType, basicMessageData.ProducerId, basicMessageData.EventId, rawData);
            Dispatch(UnparsableMessageReceived, dispatchmentEventArgs, "OnUnparsableMessageReceived");
        }

        /// <summary>
        /// Raises the specified event
        /// </summary>
        /// <typeparam name="T">The type of the event arguments</typeparam>
        /// <param name="handler">A <see cref="EventHandler{T}"/> representing the event</param>
        /// <param name="eventArgs">Event arguments</param>
        /// <param name="eventName">The name of the event</param>
        private void Dispatch<T>(EventHandler<T> handler, T eventArgs, string eventName)
        {
            if (handler == null)
            {
                _log.LogWarning($"No event listeners attached to event {eventName}.");
                return;
            }

            try
            {
                handler(this, eventArgs);
                _log.LogInformation($"Successfully raised event {eventName}.");
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, $"Client threw an exception when handling event {eventName}.");
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool disposing)
        {
            if (_isDisposed || !disposing)
            {
                return;
            }

            _isDisposed = true;
            if (Interlocked.Read(ref _isOpened) == 1)
            {
                Close();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Opens the current instance
        /// </summary>
        /// <exception cref="InvalidOperationException">Current FeedSystemSession is already opened</exception>
        public void Open()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 1, 0) == 1)
            {
                throw new InvalidOperationException("Current FeedSystemSession is already opened");
            }

            _messageReceiver.FeedMessageReceived += OnFeedMessageReceived;
            _messageReceiver.FeedMessageDeserializationFailed += OnFeedMessageDeserializationFailed;
            _messageReceiver.Open(null, FeedRoutingKeyBuilder.GetLiveKeys());
        }

        /// <summary>
        /// Closes the current instance
        /// </summary>
        /// <exception cref="InvalidOperationException">Current FeedSystemSession is already closed</exception>
        public void Close()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 0, 1) == 0)
            {
                throw new InvalidOperationException("Current FeedSystemSession is already closed");
            }
            _messageReceiver.FeedMessageReceived -= OnFeedMessageReceived;
            _messageReceiver.FeedMessageDeserializationFailed -= OnFeedMessageDeserializationFailed;
            _messageReceiver.Close();
        }
    }
}
