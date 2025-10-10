/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example;

/// <summary>
/// A Multi-Session example demonstrating using two sessions for Low and High priority messages
/// </summary>
public class MultiSession : ExampleBase
{
    private readonly UofClientAuthentication.IPrivateKeyJwtData _clientAuthentication;

    public MultiSession(ILogger<MultiSession> logger, UofClientAuthentication.IPrivateKeyJwtData clientAuthentication)
        : base(logger)
    {
        _clientAuthentication = clientAuthentication;
    }

    public override void Run()
    {
        Log.LogInformation("Running the Multi-Session example");

        var configuration = UofSdk.GetConfigurationBuilder().SetClientAuthentication(_clientAuthentication).BuildFromConfigFile();
        var uofSdk = RegisterServicesAndGetUofSdk(configuration);
        AttachToGlobalEvents(uofSdk);

        Log.LogInformation("Creating IUofSessions");

        var sessionHigh = uofSdk.GetSessionBuilder()
                                .SetMessageInterest(MessageInterest.HighPriorityMessages)
                                .Build();
        var sessionLow = uofSdk.GetSessionBuilder()
                               .SetMessageInterest(MessageInterest.LowPriorityMessages)
                               .Build();

        Log.LogInformation("Attaching to events for session with message interest 'HighPriorityMessages'");
        AttachToSessionHighEvents(sessionHigh);
        Log.LogInformation("Attaching to events for session with message interest 'LowPriorityMessages'");
        AttachToSessionLowEvents(sessionLow);

        Log.LogInformation("Opening the sdk instance");
        uofSdk.Open();
        Log.LogInformation("Example successfully started. Hit <enter> to quit");
        Console.WriteLine(string.Empty);
        Console.ReadLine();

        Log.LogInformation("Closing / disposing the sdk instance");
        uofSdk.Close();

        DetachFromGlobalEvents(uofSdk);
        DetachFromSessionHighEvents(sessionHigh);
        DetachFromSessionLowEvents(sessionLow);

        Log.LogInformation("Stopped");
    }

    /// <summary>
    /// Attaches to events raised by <see cref="IUofSession"/>
    /// </summary>
    /// <param name="session">A <see cref="IUofSession"/> instance </param>
    private void AttachToSessionHighEvents(IUofSession session)
    {
        Guard.Argument(session, nameof(session)).NotNull();

        Log.LogInformation("Attaching to session events (High)");
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
    /// Detaches from events defined by <see cref="IUofSession"/>
    /// </summary>
    /// <param name="session">A <see cref="IUofSession"/> instance</param>
    private void DetachFromSessionHighEvents(IUofSession session)
    {
        Guard.Argument(session, nameof(session)).NotNull();

        Log.LogInformation("Detaching from session events (High)");
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
    /// Attaches to events raised by <see cref="IUofSession"/>
    /// </summary>
    /// <param name="session">A <see cref="IUofSession"/> instance </param>
    private void AttachToSessionLowEvents(IUofSession session)
    {
        Guard.Argument(session, nameof(session)).NotNull();

        Log.LogInformation("Attaching to session events (Low)");
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
    /// Detaches from events defined by <see cref="IUofSession"/>
    /// </summary>
    /// <param name="session">A <see cref="IUofSession"/> instance</param>
    private void DetachFromSessionLowEvents(IUofSession session)
    {
        Guard.Argument(session, nameof(session)).NotNull();

        Log.LogInformation("Detaching from session events (Low)");
        session.OnUnparsableMessageReceived -= SessionLowOnUnparsableMessageReceived;
        session.OnBetCancel -= SessionLowOnBetCancel;
        session.OnBetSettlement -= SessionLowOnBetSettlement;
        session.OnBetStop -= SessionLowOnBetStop;
        session.OnFixtureChange -= SessionLowOnFixtureChange;
        session.OnOddsChange -= SessionLowOnOddsChange;
        session.OnRollbackBetCancel -= SessionLowOnRollbackBetCancel;
        session.OnRollbackBetSettlement -= SessionLowOnRollbackBetSettlement;
    }

    private void SessionHighOnOddsChange(object sender, OddsChangeEventArgs<ISportEvent> oddsChangeEventArgs)
    {
        var baseEntity = oddsChangeEventArgs.GetOddsChange();
        WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionHighOnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> fixtureChangeEventArgs)
    {
        var baseEntity = fixtureChangeEventArgs.GetFixtureChange();
        WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionHighOnBetStop(object sender, BetStopEventArgs<ISportEvent> betStopEventArgs)
    {
        var baseEntity = betStopEventArgs.GetBetStop();
        WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionHighOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
    {
        var baseEntity = betSettlementEventArgs.GetBetSettlement();
        WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionHighOnBetCancel(object sender, BetCancelEventArgs<ISportEvent> betCancelEventArgs)
    {
        var baseEntity = betCancelEventArgs.GetBetCancel();
        WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionHighOnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs unparsableMessageEventArgs)
    {
        Log.LogInformation("[HIGH] {MessageType} message came for event {EventId}", unparsableMessageEventArgs.MessageType.GetType().Name, unparsableMessageEventArgs.EventId);
    }

    private void SessionHighOnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> rollbackBetSettlementEventArgs)
    {
        var baseEntity = rollbackBetSettlementEventArgs.GetBetSettlementRollback();
        WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionHighOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
    {
        var baseEntity = rollbackBetCancelEventArgs.GetBetCancelRollback();
        WriteHighSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionLowOnOddsChange(object sender, OddsChangeEventArgs<ISportEvent> oddsChangeEventArgs)
    {
        var baseEntity = oddsChangeEventArgs.GetOddsChange();
        WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionLowOnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> fixtureChangeEventArgs)
    {
        var baseEntity = fixtureChangeEventArgs.GetFixtureChange();
        WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionLowOnBetStop(object sender, BetStopEventArgs<ISportEvent> betStopEventArgs)
    {
        var baseEntity = betStopEventArgs.GetBetStop();
        WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionLowOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
    {
        var baseEntity = betSettlementEventArgs.GetBetSettlement();
        WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionLowOnBetCancel(object sender, BetCancelEventArgs<ISportEvent> betCancelEventArgs)
    {
        var baseEntity = betCancelEventArgs.GetBetCancel();
        WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionLowOnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs unparsableMessageEventArgs)
    {
        Log.LogInformation("[LOW] {MessageType} message came for event {EventId}", unparsableMessageEventArgs.MessageType.GetType().Name, unparsableMessageEventArgs.EventId);
    }

    private void SessionLowOnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> rollbackBetSettlementEventArgs)
    {
        var baseEntity = rollbackBetSettlementEventArgs.GetBetSettlementRollback();
        WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void SessionLowOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
    {
        var baseEntity = rollbackBetCancelEventArgs.GetBetCancelRollback();
        WriteLowSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.RequestId);
    }

    private void WriteHighSportEntity(string msgType, ISportEvent message, long? requestId)
    {
        var msgRequestId = requestId.HasValue ? $" ({requestId.Value})" : string.Empty;
        Log.LogInformation("[HIGH] {MsgType} message for eventId: {EventId}{RequestId}", msgType.Replace("`1", string.Empty), message.Id, msgRequestId);
    }

    private void WriteLowSportEntity(string msgType, ISportEvent message, long? requestId)
    {
        var msgRequestId = requestId.HasValue ? $" ({requestId.Value})" : string.Empty;
        Log.LogInformation("[LOW]  {MsgType} message for eventId: {EventId}{RequestId}", msgType.Replace("`1", string.Empty), message.Id, msgRequestId);
    }
}
