/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Common.Logging;
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
        /// A <see cref="ILog"/> instance used for logging
        /// </summary>
        private readonly ILog _log = SdkLoggerFactory.GetLoggerForExecution(typeof(FeedSystemSession));

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
        /// A <see cref="IGlobalEventDispatcher"/> used to dispatch global events
        /// </summary>
        private readonly IGlobalEventDispatcher _globalEventDispatcher;

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
        /// A <see cref="IFeedMessageMapper"/> used to map the feed messages to messages used by the SDK
        /// </summary>
        private readonly IFeedMessageMapper _messageMapper;

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
        /// <param name="globalEventDispatcher">A <see cref="IGlobalEventDispatcher"/> used to dispatch global events</param>
        /// <param name="messageReceiver"> A <see cref="IMessageReceiver"/> used to provide feed messages</param>
        /// <param name="messageMapper">A <see cref="IFeedMessageMapper"/> used to map the feed messages to messages used by the SDK</param>
        /// <param name="messageValidator">A <see cref="IFeedMessageValidator"/> instance used to validate received messages</param>
        /// <param name="messageDataExtractor">A <see cref="IMessageDataExtractor"/> instance used to extract basic message data from messages which could not be deserialized</param>
        public FeedSystemSession(
            IGlobalEventDispatcher globalEventDispatcher,
            IMessageReceiver messageReceiver,
            IFeedMessageMapper messageMapper,
            IFeedMessageValidator messageValidator,
            IMessageDataExtractor messageDataExtractor)
        {
            Contract.Requires(globalEventDispatcher != null);
            Contract.Requires(messageReceiver != null);
            Contract.Requires(messageMapper != null);
            Contract.Requires(messageValidator != null);
            Contract.Requires(messageDataExtractor != null);

            _globalEventDispatcher = globalEventDispatcher;
            _messageReceiver = messageReceiver;
            _messageValidator = messageValidator;
            _messageMapper = messageMapper;
            _messageDataExtractor = messageDataExtractor;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_globalEventDispatcher != null);
            Contract.Invariant(_messageReceiver != null);
            Contract.Invariant(_messageValidator != null);
            Contract.Invariant(_messageMapper != null);
            Contract.Invariant(_messageDataExtractor != null);
        }

        /// <summary>
        /// Processes the received <see cref="FeedMessage"/>
        /// </summary>
        /// <param name="feedMessage">The <see cref="FeedMessage"/> to process.</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void ProcessMessage(FeedMessage feedMessage, byte[] rawMessage)
        {
            var alive = feedMessage as alive;
            if (alive != null)
            {
                var args = new FeedMessageReceivedEventArgs(alive, null,  rawMessage);
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
                case ValidationResult.FAILURE:
                    _log.Warn($"Validation of message=[{message}] failed. Raising OnUnparsableMessageReceived event");
                    MessageType messageType;
                    try
                    {
                        messageType = _messageDataExtractor.GetMessageTypeFromMessage(message);
                    }
                    catch (ArgumentException ex)
                    {
                        _log.Error($"An error occurred while determining the MessageType of the message whose validation has failed. Message={message}", ex);
                        return;
                    }
                    var eventArgs = new UnparsableMessageEventArgs(messageType, message.ProducerId.ToString(), message.EventId, e.RawMessage);
                    Dispatch(UnparsableMessageReceived, eventArgs, "OnUnparsableMessageReceived");
                    return;
                case ValidationResult.PROBLEMS_DETECTED:
                    _log.Warn($"Problems were detected while validating message=[{message}], but the message is still eligible for further processing.");
                    return;
                case ValidationResult.SUCCESS:
                    _log.Debug($"Message=[{message}] successfully validated. Continuing with message processing");
                    ProcessMessage(message, e.RawMessage);
                    return;
                default:
                    _log.ErrorFormat($"ValidationResult {Enum.GetName(typeof(ValidationResult), validationResult)} is not supported. Aborting processing of message=[{message}]");
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
            _log.Info($"Extracted the following data from unparsed message data: [{basicMessageData}], raising OnUnparsableMessageReceived event");
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
            Contract.Assume(_log != null);

            if (handler == null)
            {
                _log.Warn($"No event listeners attached to event {eventName}.");
                return;
            }

            try
            {
                handler(this, eventArgs);
                _log.Info($"Successfully raised event {eventName}.");
            }
            catch (Exception ex)
            {
                _log.Warn($"Client threw an exception when handling event {eventName}. Exception: {ex}");
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
        /// <exception cref="System.InvalidOperationException">Current FeedSystemSession is already opened</exception>
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
        /// <exception cref="System.InvalidOperationException">Current FeedSystemSession is already closed</exception>
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