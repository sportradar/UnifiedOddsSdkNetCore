/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    /// <summary>
    /// A Multi-Session example demonstrating using two sessions for Low and High priority messages
    /// </summary>
    public class MultiSession
    {
        private readonly ILog _log;

        public MultiSession(ILog log)
        {
            _log = log;
        }

        public void Run()
        {
            _log.Info("Running the OddsFeed SDK Multi-Session example");

            var configuration = Feed.GetConfigurationBuilder().SetAccessTokenFromConfigFile().SelectIntegration().LoadFromConfigFile().Build();
            var oddsFeed = new Feed(configuration);
            AttachToFeedEvents(oddsFeed);

            _log.Info("Creating IOddsFeedSessions");

             var sessionHigh = oddsFeed.CreateBuilder()
                    .SetMessageInterest(MessageInterest.HighPriorityMessages)
                    .Build();
            var sessionLow = oddsFeed.CreateBuilder()
                    .SetMessageInterest(MessageInterest.LowPriorityMessages)
                    .Build();

            _log.Info("Attaching to events for session with message interest 'HighPriorityMessages'");
            AttachToSessionHighEvents(sessionHigh);
            _log.Info("Attaching to events for session with message interest 'LowPriorityMessages'");
            AttachToSessionLowEvents(sessionLow);

            _log.Info("Opening the feed instance");
            oddsFeed.Open();
            _log.Info("Example successfully started. Hit <enter> to quit");
            Console.WriteLine(string.Empty);
            Console.ReadLine();

            _log.Info("Closing / disposing the feed");
            oddsFeed.Close();

            DetachFromFeedEvents(oddsFeed);
            DetachFromSessionHighEvents(sessionHigh);
            DetachFromSessionLowEvents(sessionLow);

            _log.Info("Stopped");
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="oddsFeed">A <see cref="IOddsFeed"/> instance </param>
        private void AttachToFeedEvents(IOddsFeed oddsFeed)
        {
            Contract.Requires(oddsFeed != null);

            _log.Info("Attaching to feed events");
            oddsFeed.ProducerUp += OnProducerUp;
            oddsFeed.ProducerDown += OnProducerDown;
            oddsFeed.Disconnected += OnDisconnected;
            oddsFeed.Closed += OnClosed;
        }

        /// <summary>
        /// Detaches from events defined by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="oddsFeed">A <see cref="IOddsFeed"/> instance</param>
        private void DetachFromFeedEvents(IOddsFeed oddsFeed)
        {
            Contract.Requires(oddsFeed != null);

            _log.Info("Detaching from feed events");
            oddsFeed.ProducerUp -= OnProducerUp;
            oddsFeed.ProducerDown -= OnProducerDown;
            oddsFeed.Disconnected -= OnDisconnected;
            oddsFeed.Closed -= OnClosed;
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IOddsFeedSession"/>
        /// </summary>
        /// <param name="session">A <see cref="IOddsFeedSession"/> instance </param>
        private void AttachToSessionHighEvents(IOddsFeedSession session)
        {
            Contract.Requires(session != null);

            _log.Info("Attaching to session (high) events");
            session.OnUnparsableMessageReceived += SessionHighOnUnparsableMessageReceived;
            session.OnBetCancel += SessionHighOnBetCancel;
            session.OnBetSettlement += SessionHighOnBetSettlement;
            session.OnBetStop += SessionHighOnBetStop;
            session.OnFixtureChange += SessionHighOnFixtureChange;
            session.OnOddsChange += SessionHighOnOddsChange;
            session.OnRollbackBetCancel += SessionHighOnRollbackBetCancel;
            session.OnRollbackBetSettlement += SessionHighOnRollbackBetSettlement;
        }

        /// <summary>
        /// Detaches from events defined by <see cref="IOddsFeedSession"/>
        /// </summary>
        /// <param name="session">A <see cref="IOddsFeedSession"/> instance</param>
        private void DetachFromSessionHighEvents(IOddsFeedSession session)
        {
            Contract.Requires(session != null);

            _log.Info("Detaching from session (high) events");
            session.OnUnparsableMessageReceived -= SessionHighOnUnparsableMessageReceived;
            session.OnBetCancel -= SessionHighOnBetCancel;
            session.OnBetSettlement -= SessionHighOnBetSettlement;
            session.OnBetStop -= SessionHighOnBetStop;
            session.OnFixtureChange -= SessionHighOnFixtureChange;
            session.OnOddsChange -= SessionHighOnOddsChange;
            session.OnRollbackBetCancel -= SessionHighOnRollbackBetCancel;
            session.OnRollbackBetSettlement -= SessionHighOnRollbackBetSettlement;
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IOddsFeedSession"/>
        /// </summary>
        /// <param name="session">A <see cref="IOddsFeedSession"/> instance </param>
        private void AttachToSessionLowEvents(IOddsFeedSession session)
        {
            Contract.Requires(session != null);

            _log.Info("Attaching to session (low) events");
            session.OnUnparsableMessageReceived += SessionLowOnUnparsableMessageReceived;
            session.OnBetCancel += SessionLowOnBetCancel;
            session.OnBetSettlement += SessionLowOnBetSettlement;
            session.OnBetStop += SessionLowOnBetStop;
            session.OnFixtureChange += SessionLowOnFixtureChange;
            session.OnOddsChange += SessionLowOnOddsChange;
            session.OnRollbackBetCancel += SessionLowOnRollbackBetCancel;
            session.OnRollbackBetSettlement += SessionLowOnRollbackBetSettlement;
        }

        /// <summary>
        /// Detaches from events defined by <see cref="IOddsFeedSession"/>
        /// </summary>
        /// <param name="session">A <see cref="IOddsFeedSession"/> instance</param>
        private void DetachFromSessionLowEvents(IOddsFeedSession session)
        {
            Contract.Requires(session != null);

            _log.Info("Detaching from session (low) events");
            session.OnUnparsableMessageReceived -= SessionLowOnUnparsableMessageReceived;
            session.OnBetCancel -= SessionLowOnBetCancel;
            session.OnBetSettlement -= SessionLowOnBetSettlement;
            session.OnBetStop -= SessionLowOnBetStop;
            session.OnFixtureChange -= SessionLowOnFixtureChange;
            session.OnOddsChange -= SessionLowOnOddsChange;
            session.OnRollbackBetCancel -= SessionLowOnRollbackBetCancel;
            session.OnRollbackBetSettlement -= SessionLowOnRollbackBetSettlement;
        }

        private void SessionHighOnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> rollbackBetSettlementEventArgs)
        {
            var baseEntity = rollbackBetSettlementEventArgs.GetBetSettlementRollback();
            WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionHighOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
        {
            var baseEntity = rollbackBetCancelEventArgs.GetBetCancelRollback();
            WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionHighOnOddsChange(object sender, OddsChangeEventArgs<ISportEvent> oddsChangeEventArgs)
        {
            var baseEntity = oddsChangeEventArgs.GetOddsChange();
            WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionHighOnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> fixtureChangeEventArgs)
        {
            var baseEntity = fixtureChangeEventArgs.GetFixtureChange();
            WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionHighOnBetStop(object sender, BetStopEventArgs<ISportEvent> betStopEventArgs)
        {
            var baseEntity = betStopEventArgs.GetBetStop();
            WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionHighOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
        {
            var baseEntity = betSettlementEventArgs.GetBetSettlement();
            WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionHighOnBetCancel(object sender, BetCancelEventArgs<ISportEvent> betCancelEventArgs)
        {
            var baseEntity = betCancelEventArgs.GetBetCancel();
            WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionHighOnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs unparsableMessageEventArgs)
        {
            _log.Info($"PREMATCH: {unparsableMessageEventArgs.MessageType.GetType()} message came for event {unparsableMessageEventArgs.EventId}.");
        }

        private void SessionLowOnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> rollbackBetSettlementEventArgs)
        {
            var baseEntity = rollbackBetSettlementEventArgs.GetBetSettlementRollback();
            WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionLowOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
        {
            var baseEntity = rollbackBetCancelEventArgs.GetBetCancelRollback();
            WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionLowOnOddsChange(object sender, OddsChangeEventArgs<ISportEvent> oddsChangeEventArgs)
        {
            var baseEntity = oddsChangeEventArgs.GetOddsChange();
            WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionLowOnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> fixtureChangeEventArgs)
        {
            var baseEntity = fixtureChangeEventArgs.GetFixtureChange();
            WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionLowOnBetStop(object sender, BetStopEventArgs<ISportEvent> betStopEventArgs)
        {
            var baseEntity = betStopEventArgs.GetBetStop();
            WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionLowOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
        {
            var baseEntity = betSettlementEventArgs.GetBetSettlement();
            WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionLowOnBetCancel(object sender, BetCancelEventArgs<ISportEvent> betCancelEventArgs)
        {
            var baseEntity = betCancelEventArgs.GetBetCancel();
            WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event);
        }

        private void SessionLowOnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs unparsableMessageEventArgs)
        {
            _log.Info($"PREMATCH: {unparsableMessageEventArgs.MessageType.GetType()} message came for event {unparsableMessageEventArgs.EventId}.");
        }

        /// <summary>
        /// Invoked when the connection to the feed is lost
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="eventArgs">The event arguments</param>
        private void OnDisconnected(object sender, EventArgs eventArgs)
        {
            _log.Warn("Connection to the feed lost");
        }

        /// <summary>
        /// Invoked when the the feed is closed
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnClosed(object sender, FeedCloseEventArgs e)
        {
            _log.Warn($"The feed is closed with the reason: {e.GetReason()}");
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes down
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerDown(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.Warn($"Producer {e.GetProducerStatusChange().Producer} is down");
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes up
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerUp(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.Info($"Producer {e.GetProducerStatusChange().Producer} is up");
        }

        private void WriteHighSportEntity(string msgType, ISportEvent message)
        {
            _log.Debug($"HIGH: {msgType.Replace("`1", string.Empty)} message for eventId: {message.Id}");
        }

        private void WriteLowSportEntity(string msgType, ISportEvent message)
        {
            _log.Debug($"LOW: {msgType.Replace("`1", string.Empty)} message for eventId: {message.Id}");
        }
    }
}
