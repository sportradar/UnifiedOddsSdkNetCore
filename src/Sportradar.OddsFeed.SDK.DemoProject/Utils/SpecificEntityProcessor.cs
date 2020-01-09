/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using App.Metrics;
using App.Metrics.Timer;
using Microsoft.Extensions.Logging;
using Dawn;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
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
        /// Default context for metrics
        /// </summary>
        private const string MetricsContext = "SpecificEntityProcessor";
        
        private readonly TimerOptions _timerOnOddsChangeReceived = new TimerOptions { Context = MetricsContext, Name = "OnOddsChangeReceived", MeasurementUnit = Unit.Items };
        private readonly TimerOptions _timerOnBetStopReceived = new TimerOptions { Context = MetricsContext, Name = "OnBetStopReceived", MeasurementUnit = Unit.Items };
        private readonly TimerOptions _timerOnBetSettlementReceived = new TimerOptions { Context = MetricsContext, Name = "OnBetSettlementReceived", MeasurementUnit = Unit.Items };
        private readonly TimerOptions _timerOnRollbackBetSettlement = new TimerOptions { Context = MetricsContext, Name = "OnRollbackBetSettlement", MeasurementUnit = Unit.Items };
        private readonly TimerOptions _timerOnBetCancel = new TimerOptions { Context = MetricsContext, Name = "OnBetCancel", MeasurementUnit = Unit.Items };
        private readonly TimerOptions _timerOnRollbackBetCancel = new TimerOptions { Context = MetricsContext, Name = "OnRollbackBetCancel", MeasurementUnit = Unit.Items };
        private readonly TimerOptions _timerOnFixtureChange = new TimerOptions { Context = MetricsContext, Name = "OnFixtureChange", MeasurementUnit = Unit.Items };

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

            _log = log ?? SdkLoggerFactory.GetLogger(typeof(SpecificEntityProcessor<T>)); // new NullLogger<SpecificEntityProcessor<T>>();
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

            using var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOnOddsChangeReceived, $"{e.GetOddsChange().Event.Id}");
            var oddsChange = e.GetOddsChange();
            _log.LogInformation($"OddsChange received. EventId:{oddsChange.Event.Id} Producer:{oddsChange.Producer.Name} RequestId:{oddsChange.RequestId}");
            _sportEntityWriter?.WriteData(oddsChange.Event);
            _marketWriter?.WriteMarketNamesForEvent(oddsChange.Markets);
            _log.LogInformation($"OddsChange received. EventId:{oddsChange.Event.Id}. Processing took {t.Elapsed.TotalMilliseconds}ms.");
        }

        /// <summary>
        /// Invoked when bet stop message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBetStopReceived(object sender, BetStopEventArgs<T> e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            using var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOnBetStopReceived, $"{e.GetBetStop().Event.Id}");
            var betStop = e.GetBetStop();
            _log.LogInformation($"BetStop received. EventId:{betStop.Event.Id} Producer:{betStop.Producer.Name}, Groups:{betStop.Groups}, RequestId:{betStop.RequestId}");
            _sportEntityWriter?.WriteData(betStop.Event);
            _log.LogInformation($"BetStop received. EventId:{betStop.Event.Id}. Processing took {t.Elapsed.TotalMilliseconds}ms.");
        }

        /// <summary>
        /// Invoked when bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetSettlementReceived(object sender, BetSettlementEventArgs<T> e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            using var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOnBetSettlementReceived, $"{e.GetBetSettlement().Event.Id}");
            var betSettlement = e.GetBetSettlement();
            _log.LogInformation($"BetSettlement received. EventId:{betSettlement.Event.Id} Producer:{betSettlement.Producer.Name}, RequestId:{betSettlement.RequestId}, Market count:{betSettlement.Markets.Count()}");
            _sportEntityWriter?.WriteData(betSettlement.Event);
            _marketWriter?.WriteMarketNamesForEvent(betSettlement.Markets);
            _log.LogInformation($"BetSettlement received. EventId:{betSettlement.Event.Id}. Processing took {t.Elapsed.TotalMilliseconds}ms.");
        }

        /// <summary>
        /// Invoked when bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetCancel(object sender, BetCancelEventArgs<T> e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            using var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOnBetCancel, $"{e.GetBetCancel().Event.Id}");
            var betCancel = e.GetBetCancel();
            _log.LogInformation($"BetCancel received. Producer:{betCancel.Producer}, RequestId:{betCancel.RequestId}, MarketCount:{betCancel.Markets.Count()}");
            _sportEntityWriter?.WriteData(betCancel.Event);
            _marketWriter?.WriteMarketNamesForEvent(betCancel.Markets);
            _log.LogInformation($"BetCancel received. EventId:{betCancel.Event.Id}. Processing took {t.Elapsed.TotalMilliseconds}ms.");
        }

        /// <summary>
        /// Invoked when rollback bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<T> e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            using var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOnRollbackBetSettlement, $"{e.GetBetSettlementRollback().Event.Id}");
            var settlementRollback = e.GetBetSettlementRollback();
            _log.LogInformation($"RollbackBetSettlement received. Producer:{settlementRollback.Producer.Name}, RequestId:{settlementRollback.RequestId}, MarketCount:{settlementRollback.Markets.Count()}");
            _sportEntityWriter?.WriteData(settlementRollback.Event);
            _marketWriter?.WriteMarketNamesForEvent(settlementRollback.Markets);
            _log.LogInformation($"RollbackBetSettlement received. EventId:{settlementRollback.Event.Id}. Processing took {t.Elapsed.TotalMilliseconds}ms.");
        }

        /// <summary>
        /// Invoked when rollback bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<T> e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            using var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOnRollbackBetCancel, $"{e.GetBetCancelRollback().Event.Id}");
            var cancelRollback = e.GetBetCancelRollback();
            _log.LogInformation($"RollbackBetCancel received. Producer:{cancelRollback.Producer.Name}, RequestId:{cancelRollback.RequestId}, MarketCount:{cancelRollback.Markets.Count()}");
            _sportEntityWriter?.WriteData(cancelRollback.Event);
            _marketWriter?.WriteMarketNamesForEvent(cancelRollback.Markets);
            _log.LogInformation($"RollbackBetCancel received. EventId:{cancelRollback.Event.Id}. Processing took {t.Elapsed.TotalMilliseconds}ms.");
        }

        /// <summary>
        /// Invoked when fixture change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnFixtureChange(object sender, FixtureChangeEventArgs<T> e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            using var t = SdkMetricsFactory.MetricsRoot.Measure.Timer.Time(_timerOnFixtureChange, $"{e.GetFixtureChange().Event.Id}");
            var fixtureChange = e.GetFixtureChange();
            _log.LogInformation($"FixtureChange received. Producer:{fixtureChange.Producer.Name}, RequestId:{fixtureChange.RequestId}, EventId:{fixtureChange.Event.Id}");
            _sportEntityWriter?.WriteData(fixtureChange.Event);
            _log.LogInformation($"FixtureChange received. EventId:{fixtureChange.Event.Id}. Processing took {t.Elapsed.TotalMilliseconds}ms.");
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
}