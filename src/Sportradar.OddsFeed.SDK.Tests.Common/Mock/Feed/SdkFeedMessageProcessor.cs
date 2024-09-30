// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Concurrent;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public sealed class SdkFeedMessageProcessor
{
    private readonly IEntityDispatcher<ISportEvent> _dispatcher;

    public ConcurrentBag<UserReceivedMessage> ReceivedFeedMessages { get; }

    public string Name { get; }

    public SdkFeedMessageProcessor(IEntityDispatcher<ISportEvent> dispatcher, string name = null)
    {
        _dispatcher = dispatcher;
        ReceivedFeedMessages = new ConcurrentBag<UserReceivedMessage>();
        Name = name ?? ((IUofSession)dispatcher).Name;
    }

    /// <summary>
    /// Opens the current processor so it will start processing dispatched entities.
    /// </summary>
    public void Open()
    {
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
    private void OnOddsChangeReceived(object sender, OddsChangeEventArgs<ISportEvent> e)
    {
        var oddsChange = e.GetOddsChange();
        ReceivedFeedMessages.Add(new UserReceivedMessage(oddsChange.Timestamps.Created, oddsChange.Event, "OddsChange", oddsChange.Markets.ToList()));
    }

    /// <summary>
    /// Invoked when fixture change message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> e)
    {
        var fixtureChange = e.GetFixtureChange();
        ReceivedFeedMessages.Add(new UserReceivedMessage(fixtureChange.Timestamps.Created, fixtureChange.Event, "FixtureChange"));
    }

    /// <summary>
    /// Invoked when bet stop message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnBetStopReceived(object sender, BetStopEventArgs<ISportEvent> e)
    {
        var betStop = e.GetBetStop();
        ReceivedFeedMessages.Add(new UserReceivedMessage(betStop.Timestamps.Created, betStop.Event, "BetStop"));
    }

    /// <summary>
    /// Invoked when bet cancel message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnBetCancel(object sender, BetCancelEventArgs<ISportEvent> e)
    {
        var betCancel = e.GetBetCancel();
        ReceivedFeedMessages.Add(new UserReceivedMessage(betCancel.Timestamps.Created, betCancel.Event, "BetCancel", betCancel.Markets.ToList()));
    }

    /// <summary>
    /// Invoked when bet settlement message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnBetSettlementReceived(object sender, BetSettlementEventArgs<ISportEvent> e)
    {
        var betSettlement = e.GetBetSettlement();
        ReceivedFeedMessages.Add(new UserReceivedMessage(betSettlement.Timestamps.Created, betSettlement.Event, "BetSettlement", betSettlement.Markets.ToList()));
    }

    /// <summary>
    /// Invoked when rollback bet settlement message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> e)
    {
        var betSettlementRollback = e.GetBetSettlementRollback();
        ReceivedFeedMessages.Add(new UserReceivedMessage(betSettlementRollback.Timestamps.Created, betSettlementRollback.Event, "BetSettlementRollback", betSettlementRollback.Markets.ToList()));
    }

    /// <summary>
    /// Invoked when rollback bet cancel message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> e)
    {
        var betCancelRollback = e.GetBetCancelRollback();
        ReceivedFeedMessages.Add(new UserReceivedMessage(betCancelRollback.Timestamps.Created, betCancelRollback.Event, "BetCancelRollback", betCancelRollback.Markets.ToList()));
    }
}
