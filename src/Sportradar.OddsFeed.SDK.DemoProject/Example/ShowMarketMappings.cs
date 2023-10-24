/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    /// <summary>
    /// An example demonstrating getting and displaying all Markets with Outcomes
    /// </summary>
    /// <seealso cref="MarketWriter"/>
    public class ShowMarketMappings : ExampleBase
    {
        private readonly CultureInfo _culture;
        private MarketMappingsWriter _marketMappingsWriter;

        public ShowMarketMappings(ILogger<ShowMarketMappings> logger, CultureInfo culture)
            : base(logger)
        {
            _culture = culture;
        }

        public override void Run(MessageInterest messageInterest)
        {
            Log.LogInformation("Running the Markets Names example");

            Log.LogInformation("Retrieving configuration from application configuration file");
            var configuration = UofSdk.GetConfigurationBuilder().BuildFromConfigFile();
            var uofSdk = RegisterServicesAndGetUofSdk(configuration);
            AttachToGlobalEvents(uofSdk);

            Log.LogInformation("Creating IUofSessions");
            var session = uofSdk.GetSessionBuilder()
                .SetMessageInterest(messageInterest)
                .Build();

            _marketMappingsWriter = new MarketMappingsWriter(TaskProcessor, _culture, Log);

            AttachToGlobalEvents(uofSdk);
            AttachToSessionEvents(session);

            Log.LogInformation("Opening the sdk instance");
            uofSdk.Open();
            Log.LogInformation("Example successfully started. Hit <enter> to quit");
            Console.WriteLine(string.Empty);
            Console.ReadLine();

            Log.LogInformation("Closing / disposing the sdk instance");
            uofSdk.Close();

            DetachFromGlobalEvents(uofSdk);
            DetachFromSessionEvents(session);

            Log.LogInformation("Waiting for asynchronous operations to complete");
            var waitResult = TaskProcessor.WaitForTasks();
            Log.LogInformation("Waiting for tasks completed. Result:{WaitResult}", waitResult);
            Log.LogInformation("Stopped");
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IUofSdk"/>
        /// </summary>
        /// <param name="session">A <see cref="IUofSession"/> instance </param>
        private new void AttachToSessionEvents(IUofSession session)
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
        private new void DetachFromSessionEvents(IUofSession session)
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
            _marketMappingsWriter.WriteMarketNamesForEvent(baseEntity.Markets);
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
            _marketMappingsWriter.WriteMarketNamesForEvent(baseEntity.Markets);
        }

        private void SessionOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
        {
            var baseEntity = betSettlementEventArgs.GetBetSettlement();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
            _marketMappingsWriter.WriteMarketNamesForEvent(baseEntity.Markets);
        }

        private void SessionOnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> rollbackBetSettlementEventArgs)
        {
            var baseEntity = rollbackBetSettlementEventArgs.GetBetSettlementRollback();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
            _marketMappingsWriter.WriteMarketNamesForEvent(baseEntity.Markets);
        }

        private void SessionOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
        {
            var baseEntity = rollbackBetCancelEventArgs.GetBetCancelRollback();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Producer, baseEntity.RequestId);
            _marketMappingsWriter.WriteMarketNamesForEvent(baseEntity.Markets);
        }

        private void SessionOnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs unparsableMessageEventArgs)
        {
            Log.LogInformation("{MessageType} message came for event {EventId}", unparsableMessageEventArgs.MessageType.GetType().Name, unparsableMessageEventArgs.EventId);
        }
    }
}
