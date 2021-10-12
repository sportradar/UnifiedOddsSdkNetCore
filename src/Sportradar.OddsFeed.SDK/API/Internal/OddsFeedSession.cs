/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A <see cref="IOddsFeedSession"/> implementation representing a session with the feed
    /// </summary>
    internal class OddsFeedSession : EntityDispatcher<ISportEvent>, IOddsFeedSession
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(OddsFeedSession));

        /// <summary>
        /// A <see cref="IMessageReceiver"/> used to provide feed messages
        /// </summary>
        internal readonly IMessageReceiver MessageReceiver;

        /// <summary>
        /// A <see cref="IFeedMessageProcessor"/> instance used to process received messages
        /// </summary>
        private readonly IFeedMessageProcessor _messageProcessor;

        /// <summary>
        /// A <see cref="IFeedMessageValidator"/> instance used to validate received messages
        /// </summary>
        private readonly IFeedMessageValidator _messageValidator;

        /// <summary>
        /// The A <see cref="IMessageDataExtractor"/> instance used to extract basic message data from messages which could not be deserialized
        /// </summary>
        private readonly IMessageDataExtractor _messageDataExtractor;

        /// <summary>
        /// Specifies the type of messages handled by this <see cref="OddsFeedSession"/>
        /// </summary>
        internal readonly MessageInterest MessageInterest;

        /// <summary>
        /// A callback used to determine the routing key the session should use when connecting to the feed
        /// </summary>
        private readonly Func<OddsFeedSession, IEnumerable<string>> _getRoutingKeys;

        /// <summary>
        /// A <see cref="IDispatcherStore"/> implementation used to store and access user registered dispatchers
        /// </summary>
        private readonly IDispatcherStore _sportSpecificDispatchers;

        /// <summary>
        /// Gets the name of the session
        /// </summary>
        public string Name { get; } // => MessageInterest.Name;

        /// <summary>
        /// Raised when a message which cannot be parsed is received
        /// </summary>
        public event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="OddsFeedSession"/> class
        /// </summary>
        /// <param name="messageReceiver"> A <see cref="IMessageReceiver"/> used to provide feed messages</param>
        /// <param name="messageProcessor">A <see cref="IFeedMessageProcessor"/> instance used to process received messages</param>
        /// <param name="messageMapper">A <see cref="IFeedMessageMapper"/> used to map the feed messages to messages used by the SDK</param>
        /// <param name="messageValidator">A <see cref="IFeedMessageValidator"/> instance used to validate received messages</param>
        /// <param name="messageDataExtractor">A <see cref="IMessageDataExtractor"/> instance used to extract basic message data from messages which could not be deserialized</param>
        /// <param name="dispatcherStore">A <see cref="IDispatcherStore"/> implementation used to store and access user registered dispatchers</param>
        /// <param name="messageInterest">Specifies the type of messages handled by this <see cref="OddsFeedSession"/></param>
        /// <param name="defaultCultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the default languages as specified in the configuration</param>
        /// <param name="getRoutingKeys">Function to get appropriate routing keys based on the message interest</param>
        public OddsFeedSession(
            IMessageReceiver messageReceiver,
            IFeedMessageProcessor messageProcessor,
            IFeedMessageMapper messageMapper,
            IFeedMessageValidator messageValidator,
            IMessageDataExtractor messageDataExtractor,
            IDispatcherStore dispatcherStore,
            MessageInterest messageInterest,
            IEnumerable<CultureInfo> defaultCultures,
            Func<OddsFeedSession, IEnumerable<string>> getRoutingKeys)
            : base(messageMapper, defaultCultures)
        {
            Guard.Argument(messageReceiver, nameof(messageReceiver)).NotNull();
            Guard.Argument(messageInterest, nameof(messageInterest)).NotNull();
            Guard.Argument(messageProcessor, nameof(messageProcessor)).NotNull();
            Guard.Argument(messageValidator, nameof(messageValidator)).NotNull();
            Guard.Argument(messageDataExtractor, nameof(messageDataExtractor)).NotNull();
            Guard.Argument(dispatcherStore, nameof(dispatcherStore)).NotNull();

            MessageReceiver = messageReceiver;
            _messageProcessor = messageProcessor;
            _messageValidator = messageValidator;
            _messageDataExtractor = messageDataExtractor;
            MessageInterest = messageInterest;

            _sportSpecificDispatchers = dispatcherStore;

            _getRoutingKeys = getRoutingKeys;

            Name = char.ToUpperInvariant(MessageInterest.Name[0]) + MessageInterest.Name.Substring(1);
        }

        /// <summary>
        /// Handles the <see cref="IMessageReceiver.FeedMessageReceived"/> event
        /// </summary>
        /// <param name="sender">A <see cref="object"/> representation of the instance raising the event</param>
        /// <param name="e">A <see cref="FeedMessageReceivedEventArgs"/> instance containing event information</param>
        private void OnMessageReceived(object sender, FeedMessageReceivedEventArgs e)
        {
            var message = e.Message;
            var validationResult = _messageValidator.Validate(message);
            switch (validationResult)
            {
                case ValidationResult.FAILURE:
                    ExecutionLog.LogWarning($"{WriteMessageInterest()}Validation of message=[{message}] failed. Raising OnUnparsableMessageReceived event");
                    var messageType = _messageDataExtractor.GetMessageTypeFromMessage(message);
                    var eventArgs = new UnparsableMessageEventArgs(messageType, message.ProducerId.ToString(), message.EventId, e.RawMessage);
                    Dispatch(OnUnparsableMessageReceived, eventArgs, "OnUnparsableMessageReceived", message.ProducerId);
                    return;
                case ValidationResult.PROBLEMS_DETECTED:
                    ExecutionLog.LogWarning($"{WriteMessageInterest()}Problems were detected while validating message=[{message}], but the message is still eligible for further processing.");
                    _messageProcessor.ProcessMessage(message, MessageInterest, e.RawMessage);
                    return;
                case ValidationResult.SUCCESS:
                    ExecutionLog.LogDebug($"{WriteMessageInterest()}Message=[{message}] successfully validated. Continuing with message processing.");
                    _messageProcessor.ProcessMessage(message, MessageInterest, e.RawMessage);
                    return;
                default:
                    ExecutionLog.LogError($"{WriteMessageInterest()}ValidationResult {Enum.GetName(typeof(ValidationResult), validationResult)} is not supported. Aborting processing of message=[{message}].");
                    return;
            }
        }

        /// <summary>
        /// Handles the <see cref="IMessageReceiver.FeedMessageDeserializationFailed" /> event.
        /// </summary>
        /// <param name="sender">A <see cref="object"/> representation of the instance raising the event.</param>
        /// <param name="eventArgs">The <see cref="MessageDeserializationFailedEventArgs"/> instance containing the event data.</param>
        private void OnMessageDeserializationFailed(object sender, MessageDeserializationFailedEventArgs eventArgs)
        {
            var rawData = eventArgs.RawData as byte[] ?? eventArgs.RawData.ToArray();
            var basicMessageData = _messageDataExtractor.GetBasicMessageData(rawData);
            ExecutionLog.LogInformation($"{WriteMessageInterest()}Extracted the following data from unparsed message data: [{basicMessageData}], raising OnUnparsableMessageReceived event");
            var dispatchEventArgs = new UnparsableMessageEventArgs(basicMessageData.MessageType, basicMessageData.ProducerId, basicMessageData.EventId, rawData);
            var producerId = 0;
            if (!string.IsNullOrEmpty(basicMessageData.ProducerId))
            {
                int.TryParse(basicMessageData.ProducerId, out producerId);
            }
            Dispatch(OnUnparsableMessageReceived, dispatchEventArgs, "OnUnparsableMessageReceived", producerId);
        }

        /// <summary>
        /// Handles the <see cref="IFeedMessageProcessor.MessageProcessed"/> event
        /// </summary>
        /// <param name="sender">A <see cref="object"/> representation of the instance raising the event</param>
        /// <param name="e">A <see cref="FeedMessageReceivedEventArgs"/> instance containing event information</param>
        private void OnMessageProcessed(object sender, FeedMessageReceivedEventArgs e)
        {
            var processingTook = string.Empty;
            var sdkProcessingTime = DateTime.Now - SdkInfo.FromEpochTime(e.Message.ReceivedAt);
            if(sdkProcessingTime.TotalMilliseconds > 500)
            {
                processingTook = $" Sdk processing took {(int)sdkProcessingTime.TotalMilliseconds}ms";
            }
            ExecutionLog.LogDebug($"Dispatching {e.Message.GetType().Name} for {e.Message.EventId} ({e.Message.GeneratedAt}).{processingTook}");
            var dispatcher = SelectDispatcher(e.Message);
            dispatcher.Dispatch(e.Message, e.RawMessage);
        }

        /// <summary>
        /// Returns a <see cref="IEntityDispatcherInternal"/> which should be used to dispatch the provided <see cref="FeedMessage"/>
        /// </summary>
        /// <param name="message">The <see cref="FeedMessage"/> instance which needs to be dispatched.</param>
        /// <returns>a <see cref="IEntityDispatcherInternal"/> which should be used to dispatch the provided <see cref="FeedMessage"/></returns>
        private IEntityDispatcherInternal SelectDispatcher(FeedMessage message)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            if (!message.IsEventRelated)
            {
                return this;
            }

            var specificDispatcher = _sportSpecificDispatchers.Get(URN.Parse(message.EventId), message.SportId);
            return specificDispatcher == null
                ? this
                : (IEntityDispatcherInternal) specificDispatcher;
        }

        /// <summary>
        /// Dispatches the provided <see cref="FeedMessage"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="rawMessage"></param>
        public override void Dispatch(FeedMessage message, byte[] rawMessage)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var alive = message as alive;
            if (alive != null)
            {
                //ProcessAlive(alive);
                return;
            }

            var snapShotComplete = message as snapshot_complete;
            if (snapShotComplete != null)
            {
                //ProcessSnapshotComplete(snapShotComplete);
                //DispatchProducerUp(MessageMapperHelper.GetEnumValue<Product>(snapShotComplete.producer), snapShotComplete.timestamp);
                return;
            }

            base.Dispatch(message, rawMessage);
        }

        /// <summary>
        /// It executes steps needed when opening the instance
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected override void OnOpening()
        {
            MessageReceiver.FeedMessageReceived += OnMessageReceived;
            MessageReceiver.FeedMessageDeserializationFailed += OnMessageDeserializationFailed;
            _messageProcessor.MessageProcessed += OnMessageProcessed;

            var routingKeys = _getRoutingKeys.Invoke(this);
            MessageReceiver.Open(MessageInterest, routingKeys);
        }

        /// <summary>
        /// It executes steps needed when closing the instance
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected override void OnClosing()
        {
            MessageReceiver.FeedMessageReceived -= OnMessageReceived;
            MessageReceiver.FeedMessageDeserializationFailed -= OnMessageDeserializationFailed;
            _messageProcessor.MessageProcessed -= OnMessageProcessed;
            MessageReceiver.Close();
        }

        /// <summary>
        /// Constructs and returns a sport-specific <see cref="ISpecificEntityDispatcher{T}" /> instance allowing
        /// processing of messages containing entity specific information
        /// </summary>
        /// <typeparam name="T">A <see cref="ICompetition" /> derived type specifying the entities associated with the created <see cref="IEntityDispatcher{T}" /> instance</typeparam>
        /// <returns>The constructed <see cref="ISpecificEntityDispatcher{T}" /></returns>
        public ISpecificEntityDispatcher<T> CreateSportSpecificMessageDispatcher<T>() where T : ISportEvent
        {
            var dispatcher = new SpecificEntityDispatcher<T>(MessageMapper, DefaultCultures);
            _sportSpecificDispatchers.Add(dispatcher);
            return dispatcher;
        }

        private string WriteMessageInterest()
        {
            return $"{Name}: ";
        }
    }
}