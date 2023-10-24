using System;
using Dawn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    public abstract class ExampleBase
    {
        protected ILogger Log;
        protected TaskProcessor TaskProcessor;

        public ExampleBase(ILogger log)
        {
            Log = log ?? new NullLogger<ExampleBase>();
            TaskProcessor = new TaskProcessor(TimeSpan.FromSeconds(20));
        }

        public virtual void Run() { }

        public virtual void Run(MessageInterest messageInterest) { }

        public IUofSdk RegisterServicesAndGetUofSdk(IUofConfiguration uofConfiguration)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddLog4Net("log4net.config");
                })
                .ConfigureServices(configure => configure.AddUofSdk(uofConfiguration))
                .Build();

            Log.LogInformation("Creating UofSdk instance");
            return new UofSdk(host.Services);
        }

        public void LimitRecoveryRequests(IUofSdk uofSdk)
        {
            for (var i = 1; i < 20; i++)
            {
                uofSdk.ProducerManager.AddTimestampBeforeDisconnect(i, DateTime.Now.AddMinutes(-20));
            }
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IUofSdk"/>
        /// </summary>
        /// <param name="uofSdk">A <see cref="IUofSdk"/> instance </param>
        protected void AttachToGlobalEvents(IUofSdk uofSdk)
        {
            Guard.Argument(uofSdk, nameof(uofSdk)).NotNull();

            Log.LogInformation("Attaching to global events");
            uofSdk.ProducerUp += OnProducerUp;
            uofSdk.ProducerDown += OnProducerDown;
            uofSdk.Disconnected += OnDisconnected;
            uofSdk.Closed += OnClosed;
        }

        /// <summary>
        /// Detaches from events defined by <see cref="IUofSdk"/>
        /// </summary>
        /// <param name="uofSdk">A <see cref="IUofSdk"/> instance</param>
        protected void DetachFromGlobalEvents(IUofSdk uofSdk)
        {
            Guard.Argument(uofSdk, nameof(uofSdk)).NotNull();

            Log.LogInformation("Detaching from global events");
            uofSdk.ProducerUp -= OnProducerUp;
            uofSdk.ProducerDown -= OnProducerDown;
            uofSdk.Disconnected -= OnDisconnected;
            uofSdk.Closed -= OnClosed;
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IUofSdk"/>
        /// </summary>
        /// <param name="session">A <see cref="IUofSession"/> instance </param>
        protected void AttachToSessionEvents(IUofSession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();

            Log.LogInformation("Attaching to session events");
            session.OnUnparsableMessageReceived += SessionOnUnparsableMessageReceived;
            session.OnBetCancel += SessionOnBetCancel;
            session.OnBetSettlement += SessionOnBetSettlement;
            session.OnBetStop += SessionOnBetStop;
            session.OnFixtureChange += SessionOnFixtureChange;
            session.OnOddsChange += SessionOnOddsChange;
            session.OnRollbackBetCancel += SessionOnRollbackBetCancel;
            session.OnRollbackBetSettlement += SessionOnRollbackBetSettlement;
        }

        /// <summary>
        /// Detaches from events defined by <see cref="IUofSdk"/>
        /// </summary>
        /// <param name="session">A <see cref="IUofSession"/> instance</param>
        protected void DetachFromSessionEvents(IUofSession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();

            Log.LogInformation("Detaching from session events");
            session.OnUnparsableMessageReceived -= SessionOnUnparsableMessageReceived;
            session.OnBetCancel -= SessionOnBetCancel;
            session.OnBetSettlement -= SessionOnBetSettlement;
            session.OnBetStop -= SessionOnBetStop;
            session.OnFixtureChange -= SessionOnFixtureChange;
            session.OnOddsChange -= SessionOnOddsChange;
            session.OnRollbackBetCancel -= SessionOnRollbackBetCancel;
            session.OnRollbackBetSettlement -= SessionOnRollbackBetSettlement;
        }

        private void SessionOnOddsChange(object sender, OddsChangeEventArgs<ISportEvent> oddsChangeEventArgs)
        {
            var baseEntity = oddsChangeEventArgs.GetOddsChange();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
        }

        private void SessionOnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> fixtureChangeEventArgs)
        {
            var baseEntity = fixtureChangeEventArgs.GetFixtureChange();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
        }

        private void SessionOnBetStop(object sender, BetStopEventArgs<ISportEvent> betStopEventArgs)
        {
            var baseEntity = betStopEventArgs.GetBetStop();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
        }

        private void SessionOnBetCancel(object sender, BetCancelEventArgs<ISportEvent> betCancelEventArgs)
        {
            var baseEntity = betCancelEventArgs.GetBetCancel();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
        }

        private void SessionOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
        {
            var baseEntity = betSettlementEventArgs.GetBetSettlement();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
        }

        private void SessionOnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> rollbackBetSettlementEventArgs)
        {
            var baseEntity = rollbackBetSettlementEventArgs.GetBetSettlementRollback();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
        }

        private void SessionOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
        {
            var baseEntity = rollbackBetCancelEventArgs.GetBetCancelRollback();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
        }

        private void SessionOnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs unparsableMessageEventArgs)
        {
            Log.LogInformation("{MessageType} message came for event {EventId}", unparsableMessageEventArgs.MessageType.GetType().Name, unparsableMessageEventArgs.EventId);
        }

        /// <summary>
        /// Invoked when the connection to the rabbit connection is lost
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnDisconnected(object sender, EventArgs e)
        {
            Log.LogWarning("Connection to the rabbit is lost");
        }

        /// <summary>
        /// Invoked when the sdk instance is closed
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnClosed(object sender, FeedCloseEventArgs e)
        {
            Log.LogWarning("The sdk instance is closed with the reason: {Reason}", e.GetReason());
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes down
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerDown(object sender, ProducerStatusChangeEventArgs e)
        {
            Log.LogWarning("Producer {Producer} is down", e.GetProducerStatusChange().Producer);
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes up
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerUp(object sender, ProducerStatusChangeEventArgs e)
        {
            Log.LogInformation("Producer {Producer} is up", e.GetProducerStatusChange().Producer);
        }

        protected virtual void WriteSportEntity(string msgType, ISportEvent message, IProducer producer, long? requestId)
        {
            msgType = msgType.Replace("`1", string.Empty);

            Log.LogInformation("[{ProducerInfo}] {MsgType} received for event {EventId}{RequestId}", Helper.GetProducerInfo(producer), msgType, message.Id, Helper.GetRequestInfo(requestId));
        }
    }
}
