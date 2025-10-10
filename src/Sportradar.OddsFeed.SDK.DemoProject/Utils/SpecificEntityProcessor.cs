/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Globalization;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

/// <summary>
/// A class used to process events dispatched from the SDK
/// </summary>
internal class SpecificEntityProcessor<T> : IEntityProcessor where T : ISportEvent
{
    /// <summary>
    /// A <see cref="ILogger"/> instance used for logging
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private readonly ILogger _log;

    /// <summary>
    /// A <see cref="IEntityDispatcher{T}"/> used to obtain SDK messages
    /// </summary>
    private readonly ISpecificEntityDispatcher<T> _dispatcher;

    /// <summary>
    /// A <see cref="SportEntityWriter"/> used to write the sport entities data
    /// </summary>
    private readonly SportEntityWriter _sportEntityWriter;

    /// <summary>
    /// A <see cref="MarketWriter"/> used to write market and outcome data
    /// </summary>
    private readonly MarketWriter _marketWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecificEntityProcessor{T}"/> class
    /// </summary>
    /// <param name="dispatcher">A <see cref="ISpecificEntityDispatcher{T}"/> used to obtain SDK messages</param>
    /// <param name="sportEntityWriter">A <see cref="SportEntityWriter"/> used to write the sport entities data</param>
    /// <param name="marketWriter">A <see cref="MarketWriter"/> used to write market and outcome data</param>
    /// <param name="log">A <see cref="ILogger"/> instance used for logging</param>
    public SpecificEntityProcessor(ISpecificEntityDispatcher<T> dispatcher, SportEntityWriter sportEntityWriter = null, MarketWriter marketWriter = null, ILogger log = null)
    {
        Guard.Argument(dispatcher, nameof(dispatcher)).NotNull();
        Guard.Argument(log, nameof(log)).NotNull();

        _log = log ?? new NullLogger<SpecificEntityProcessor<T>>();
        _dispatcher = dispatcher;
        _sportEntityWriter = sportEntityWriter;
        _marketWriter = marketWriter;
    }

    /// <summary>
    /// Invoked when odds change message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    protected virtual void OnOddsChangeReceived(object sender, OddsChangeEventArgs<T> e)
    {
        Guard.Argument(e, nameof(e)).NotNull();

        using var tt = new TelemetryTracker(TelemetryConfig.OddsChangeReceived);
        var oddsChange = e.GetOddsChange();
        _log.LogInformation("[{ProducerInfo}] OddsChange received for event {EventId}{RequestId}", Helper.GetProducerInfo(oddsChange.Producer), oddsChange.Event.Id, Helper.GetRequestInfo(oddsChange.RequestId));
        _sportEntityWriter?.WriteData(oddsChange.Event);
        _marketWriter?.WriteMarketNamesForEvent(oddsChange.Markets);
        _log.LogInformation("[{ProducerInfo}] OddsChange processing for event {EventId}{RequestId} took {Elapsed} ms", Helper.GetProducerInfo(oddsChange.Producer), oddsChange.Event.Id, Helper.GetRequestInfo(oddsChange.RequestId), tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Invoked when fixture change message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnFixtureChange(object sender, FixtureChangeEventArgs<T> e)
    {
        Guard.Argument(e, nameof(e)).NotNull();

        using var tt = new TelemetryTracker(TelemetryConfig.FixtureChangeReceived);
        var fixtureChange = e.GetFixtureChange();
        _log.LogInformation("[{ProducerInfo}] FixtureChange received for event {EventId}{RequestId}", Helper.GetProducerInfo(fixtureChange.Producer), fixtureChange.Event.Id, Helper.GetRequestInfo(fixtureChange.RequestId));
        _sportEntityWriter?.WriteData(fixtureChange.Event);
        _log.LogInformation("[{ProducerInfo}] FixtureChange processing for event {EventId}{RequestId} took {Elapsed} ms", Helper.GetProducerInfo(fixtureChange.Producer), fixtureChange.Event.Id, Helper.GetRequestInfo(fixtureChange.RequestId), tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Invoked when bet stop message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    protected virtual void OnBetStopReceived(object sender, BetStopEventArgs<T> e)
    {
        Guard.Argument(e, nameof(e)).NotNull();

        using var tt = new TelemetryTracker(TelemetryConfig.BetStopReceived);
        var betStop = e.GetBetStop();
        _log.LogInformation("[{ProducerInfo}] BetStop received for event {EventId} with groups [{Groups}]{RequestId}", Helper.GetProducerInfo(betStop.Producer), betStop.Event.Id, betStop.Groups, Helper.GetRequestInfo(betStop.RequestId));
        _sportEntityWriter?.WriteData(betStop.Event);
        _log.LogInformation("[{ProducerInfo}] BetStop processing for event {EventId}{RequestId} took {Elapsed} ms", Helper.GetProducerInfo(betStop.Producer), betStop.Event.Id, Helper.GetRequestInfo(betStop.RequestId), tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Invoked when bet cancel message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnBetCancel(object sender, BetCancelEventArgs<T> e)
    {
        Guard.Argument(e, nameof(e)).NotNull();

        using var tt = new TelemetryTracker(TelemetryConfig.BetCancelReceived);
        var betCancel = e.GetBetCancel();
        _log.LogInformation("[{ProducerInfo}] BetCancel received for event {EventId} with markets {MarketsCount} {RequestId}", Helper.GetProducerInfo(betCancel.Producer), betCancel.Event.Id, betCancel.Markets.Count(), Helper.GetRequestInfo(betCancel.RequestId));
        _sportEntityWriter?.WriteData(betCancel.Event);
        _marketWriter?.WriteMarketNamesForEvent(betCancel.Markets);
        _log.LogInformation("[{ProducerInfo}] BetCancel processing for event {EventId}{RequestId} took {Elapsed} ms", Helper.GetProducerInfo(betCancel.Producer), betCancel.Event.Id, Helper.GetRequestInfo(betCancel.RequestId), tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Invoked when bet settlement message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnBetSettlementReceived(object sender, BetSettlementEventArgs<T> e)
    {
        Guard.Argument(e, nameof(e)).NotNull();

        using var tt = new TelemetryTracker(TelemetryConfig.BetSettlementReceived);
        var betSettlement = e.GetBetSettlement();
        _log.LogInformation("[{ProducerInfo}] BetSettlement received for event {EventId} with markets {MarketsCount} {RequestId}", Helper.GetProducerInfo(betSettlement.Producer), betSettlement.Event.Id, betSettlement.Markets.Count(), Helper.GetRequestInfo(betSettlement.RequestId));
        _sportEntityWriter?.WriteData(betSettlement.Event);
        _marketWriter?.WriteMarketNamesForEvent(betSettlement.Markets);
        _log.LogInformation("[{ProducerInfo}] BetSettlement processing for event {EventId}{RequestId} took {Elapsed} ms", Helper.GetProducerInfo(betSettlement.Producer), betSettlement.Event.Id, Helper.GetRequestInfo(betSettlement.RequestId), tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Invoked when rollback bet cancel message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<T> e)
    {
        Guard.Argument(e, nameof(e)).NotNull();

        using var tt = new TelemetryTracker(TelemetryConfig.RollbackBetCancelReceived);
        var cancelRollback = e.GetBetCancelRollback();
        _log.LogInformation("RollbackBetCancel received. EventId:{EventId}, Producer:{Producer}, RequestId:{RequestId}, Market count:{MarketsCount}", cancelRollback.Event.Id, cancelRollback.Producer, cancelRollback.RequestId.ToString(), cancelRollback.Markets.Count().ToString());
        _sportEntityWriter?.WriteData(cancelRollback.Event);
        _marketWriter?.WriteMarketNamesForEvent(cancelRollback.Markets);
        _log.LogInformation("[{ProducerInfo}] RollbackBetCancel processing for event {EventId}{RequestId} took {Elapsed} ms", Helper.GetProducerInfo(cancelRollback.Producer), cancelRollback.Event.Id, Helper.GetRequestInfo(cancelRollback.RequestId), tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Invoked when rollback bet settlement message is received
    /// </summary>
    /// <param name="sender">The instance raising the event</param>
    /// <param name="e">The event arguments</param>
    private void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<T> e)
    {
        Guard.Argument(e, nameof(e)).NotNull();

        using var tt = new TelemetryTracker(TelemetryConfig.RollbackBetSettlementReceived);
        var settlementRollback = e.GetBetSettlementRollback();
        _log.LogInformation("RollbackBetSettlement received. EventId:{EventId}, Producer:{Producer}, RequestId:{RequestId}, Market count:{MarketsCount}", settlementRollback.Event.Id, settlementRollback.Producer, settlementRollback.RequestId.ToString(), settlementRollback.Markets.Count().ToString());
        _sportEntityWriter?.WriteData(settlementRollback.Event);
        _marketWriter?.WriteMarketNamesForEvent(settlementRollback.Markets);
        _log.LogInformation("[{ProducerInfo}] RollbackBetSettlement processing for event {EventId}{RequestId} took {Elapsed} ms", Helper.GetProducerInfo(settlementRollback.Producer), settlementRollback.Event.Id, Helper.GetRequestInfo(settlementRollback.RequestId), tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Opens the current processor so it will start processing dispatched entities
    /// </summary>
    public void Open()
    {
        _log.LogInformation("Attaching to dispatcher events");
        _dispatcher.OnOddsChange += OnOddsChangeReceived;
        _dispatcher.OnBetCancel += OnBetCancel;
        _dispatcher.OnRollbackBetCancel += OnRollbackBetCancel;
        _dispatcher.OnBetStop += OnBetStopReceived;
        _dispatcher.OnBetSettlement += OnBetSettlementReceived;
        _dispatcher.OnRollbackBetSettlement += OnRollbackBetSettlement;
        _dispatcher.OnFixtureChange += OnFixtureChange;

        _log.LogInformation("Opening the dispatcher");
        _dispatcher.Open();
        _log.LogInformation("Event processor successfully opened");
    }

    /// <summary>
    /// Closes the current processor so it will no longer process dispatched entities
    /// </summary>
    public void Close()
    {
        _log.LogInformation("Detaching from dispatcher events");
        _dispatcher.OnOddsChange -= OnOddsChangeReceived;
        _dispatcher.OnBetCancel -= OnBetCancel;
        _dispatcher.OnRollbackBetCancel -= OnRollbackBetCancel;
        _dispatcher.OnBetStop -= OnBetStopReceived;
        _dispatcher.OnBetSettlement -= OnBetSettlementReceived;
        _dispatcher.OnRollbackBetSettlement -= OnRollbackBetSettlement;
        _dispatcher.OnFixtureChange -= OnFixtureChange;

        _log.LogInformation("Closing the dispatcher");
        _dispatcher.Close();
        _log.LogInformation("Event processor successfully closed");
    }
}
