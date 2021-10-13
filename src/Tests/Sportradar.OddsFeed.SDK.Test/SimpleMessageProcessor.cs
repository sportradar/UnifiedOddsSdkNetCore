using System;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Test
{
    public class SimpleMessageProcessor
    {
        /// <summary>
        /// A <see cref="ILogger" /> instance used for logging
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static ILogger _log;

        /// <summary>
        /// A <see cref="IEntityDispatcher{T}" /> used to obtain SDK messages
        /// </summary>
        private readonly IEntityDispatcher<ISportEvent> _dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMessageProcessor" /> class.
        /// </summary>
        /// <param name="dispatcher">
        /// A <see cref="IEntityDispatcher{ISportEvent}" /> whose dispatched entities will be processed by the current instance.
        /// </param>
        /// <param name="log">A <see cref="ILogger" /> instance used for logging</param>
        public SimpleMessageProcessor(IEntityDispatcher<ISportEvent> dispatcher, ILogger log = null)
        {
            Guard.Argument(dispatcher).NotNull();

            _dispatcher = dispatcher;
            _log = log ?? SdkLoggerFactory.GetLogger(typeof(SimpleMessageProcessor));
        }

        /// <summary>
        /// Opens the current processor so it will start processing dispatched entities.
        /// </summary>
        public void Open()
        {
            _log.LogInformation("Attaching to session events");
            _dispatcher.OnOddsChange += OnOddsChangeReceived;
            _dispatcher.OnBetCancel += OnBetCancel;
            _dispatcher.OnRollbackBetCancel += OnRollbackBetCancel;
            _dispatcher.OnBetStop += OnBetStopReceived;
            _dispatcher.OnBetSettlement += OnBetSettlementReceived;
            _dispatcher.OnRollbackBetSettlement += OnRollbackBetSettlement;
            _dispatcher.OnFixtureChange += OnFixtureChange;
        }

        /// <summary>
        /// Closes the current processor so it will no longer process dispatched entities
        /// </summary>
        public void Close()
        {
            _log.LogInformation("Detaching from session events");
            _dispatcher.OnOddsChange -= OnOddsChangeReceived;
            _dispatcher.OnBetCancel -= OnBetCancel;
            _dispatcher.OnRollbackBetCancel -= OnRollbackBetCancel;
            _dispatcher.OnBetStop -= OnBetStopReceived;
            _dispatcher.OnBetSettlement -= OnBetSettlementReceived;
            _dispatcher.OnRollbackBetSettlement -= OnRollbackBetSettlement;
            _dispatcher.OnFixtureChange -= OnFixtureChange;
        }

        /// <summary>
        /// Invoked when odds change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnOddsChangeReceived(object sender, OddsChangeEventArgs<ISportEvent> e)
        {
            WriteMessageData((IOddsFeedSession)sender, e.GetOddsChange(), DateTime.Now);
        }

        /// <summary>
        /// Invoked when fixture change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> e)
        {
            WriteMessageData((IOddsFeedSession)sender, e.GetFixtureChange(), DateTime.Now);
        }

        /// <summary>
        /// Invoked when bet stop message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBetStopReceived(object sender, BetStopEventArgs<ISportEvent> e)
        {
            WriteMessageData((IOddsFeedSession)sender, e.GetBetStop(), DateTime.Now);
        }

        /// <summary>
        /// Invoked when bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBetSettlementReceived(object sender, BetSettlementEventArgs<ISportEvent> e)
        {
            WriteMessageData((IOddsFeedSession)sender, e.GetBetSettlement(), DateTime.Now);
        }

        /// <summary>
        /// Invoked when rollback bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> e)
        {
            WriteMessageData((IOddsFeedSession)sender, e.GetBetSettlementRollback(), DateTime.Now);
        }

        /// <summary>
        /// Invoked when bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBetCancel(object sender, BetCancelEventArgs<ISportEvent> e)
        {
            WriteMessageData((IOddsFeedSession)sender, e.GetBetCancel(), DateTime.Now);
        }

        /// <summary>
        /// Invoked when rollback bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> e)
        {
            WriteMessageData((IOddsFeedSession)sender, e.GetBetCancelRollback(), DateTime.Now);
        }

        private void WriteMessageData(IOddsFeedSession session, IEventMessage<ISportEvent> message, DateTime messageUserReceived = default, double processingTotalMilliSeconds = 0)
        {

            var userStartProcessing = DateTime.Now;
            var sdkProcessingTime = (userStartProcessing - Helper.FromEpochTime(message.Timestamps.Received)).TotalMilliseconds;
            var createdToUserTime = (userStartProcessing - Helper.FromEpochTime(message.Timestamps.Created)).TotalMilliseconds;
            var pureSdkTime = messageUserReceived == default
                ? sdkProcessingTime
                : (userStartProcessing - messageUserReceived).TotalMilliseconds;

            var messageName = message.GetType().Name;
            messageName = messageName.Substring(0, messageName.IndexOf("`", StringComparison.InvariantCultureIgnoreCase));
            var requestId = message.RequestId.HasValue ? $", RequestId={message.RequestId}" : string.Empty;
            var producerMessage = $"[{message.Producer.Id}-{message.Producer.Name}|{Helper.ToEpochTime(message.Producer.LastTimestampBeforeDisconnect)}={message.Producer.LastTimestampBeforeDisconnect}|{!message.Producer.IsProducerDown}]";
            var processingTotalMilliSecondsStr = processingTotalMilliSeconds > 0 ? $", COMPLETED in {processingTotalMilliSeconds}ms" : string.Empty;
            _log.LogInformation($"{session.Name}: {messageName}:Producer={producerMessage}, GeneratedAt={message.Timestamps.Created}={Helper.FromEpochTime(message.Timestamps.Created)}, PureSdkProcessingTime={(int)pureSdkTime}ms, SdkProcessingTime={(int)sdkProcessingTime}ms, Behind={(int) createdToUserTime}ms, EventId={message.Event.Id}{requestId}{processingTotalMilliSecondsStr}.");

            if (message.Timestamps.Sent == 0)
            {
                _log.LogError($"Message {messageName} created {message.Timestamps.Created} on producer {message.Producer.Id}-{message.Producer.Name} does not have sent timestamp.");
            }
        }
    }
}
