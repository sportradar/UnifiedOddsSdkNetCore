/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// Class used to handle messages dispatched by non-specific entity dispatchers - the event is always represented as <see cref="ISportEvent"/>
    /// </summary>
    internal class EntityProcessor<T> : IEntityProcessor where T : ISportEvent
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private readonly ILogger _log;

        /// <summary>
        /// A <see cref="IEntityDispatcher{T}"/> used to obtain SDK messages
        /// </summary>
        private readonly IEntityDispatcher<T> _dispatcher;

        /// <summary>
        /// A <see cref="SportEntityWriter"/> used to write the sport entities data
        /// </summary>
        private readonly SportEntityWriter _sportEntityWriter;

        /// <summary>
        /// A <see cref="MarketWriter"/> used to write market and outcome data
        /// </summary>
        private readonly MarketWriter _marketWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="IEntityDispatcher{ISportEvent}"/> class
        /// </summary>
        /// <param name="dispatcher">A <see cref="IEntityDispatcher{ISportEvent}" /> whose dispatched entities will be processed by the current instance</param>
        /// <param name="sportEntityWriter">A <see cref="SportEntityWriter" /> used to output / log the dispatched entities</param>
        /// <param name="marketWriter">A <see cref="MarketWriter"/> used to write market and outcome data</param>
        /// <param name="log">A <see cref="ILogger" /> instance used for logging</param>
        public EntityProcessor(IEntityDispatcher<T> dispatcher, SportEntityWriter sportEntityWriter = null, MarketWriter marketWriter = null, ILogger log = null)
        {
            Guard.Argument(dispatcher, nameof(dispatcher)).NotNull();

            _dispatcher = dispatcher;
            _sportEntityWriter = sportEntityWriter;
            _marketWriter = marketWriter;
            _log = log ?? new NullLogger<EntityProcessor<T>>();
        }

        /// <summary>
        /// Invoked when odds change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnOddsChangeReceived(object sender, OddsChangeEventArgs<T> e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            var oddsChange = e.GetOddsChange();
            _log.LogInformation("[{ProducerInfo}] OddsChange received for event {EventId}{RequestId}", Helper.GetProducerInfo(oddsChange.Producer), oddsChange.Event.Id, Helper.GetRequestInfo(oddsChange.RequestId));
            _sportEntityWriter?.WriteData(oddsChange.Event);
            _marketWriter?.WriteMarketNamesForEvent(oddsChange.Markets);
        }

        /// <summary>
        /// Invoked when fixture change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnFixtureChange(object sender, FixtureChangeEventArgs<T> e)
        {
            var fixtureChange = e.GetFixtureChange();
            _log.LogInformation("[{ProducerInfo}] FixtureChange received for event {EventId}{RequestId}", Helper.GetProducerInfo(fixtureChange.Producer), fixtureChange.Event.Id, Helper.GetRequestInfo(fixtureChange.RequestId));
            _sportEntityWriter?.WriteData(fixtureChange.Event);
        }

        /// <summary>
        /// Invoked when bet stop message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBetStopReceived(object sender, BetStopEventArgs<T> e)
        {
            var betStop = e.GetBetStop();
            _log.LogInformation("[{ProducerInfo}] BetStop received for event {EventId} with groups [{Groups}]{RequestId}", Helper.GetProducerInfo(betStop.Producer), betStop.Event.Id, betStop.Groups, Helper.GetRequestInfo(betStop.RequestId));
            _sportEntityWriter?.WriteData(betStop.Event);
        }

        /// <summary>
        /// Invoked when bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetCancel(object sender, BetCancelEventArgs<T> e)
        {
            var betCancel = e.GetBetCancel();
            _log.LogInformation("[{ProducerInfo}] BetCancel received for event {EventId} with markets {MarketsCount} {RequestId}", Helper.GetProducerInfo(betCancel.Producer), betCancel.Event.Id, betCancel.Markets.Count(), Helper.GetRequestInfo(betCancel.RequestId));
            _sportEntityWriter?.WriteData(betCancel.Event);
            _marketWriter?.WriteMarketNamesForEvent(betCancel.Markets);
        }

        /// <summary>
        /// Invoked when bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetSettlementReceived(object sender, BetSettlementEventArgs<T> e)
        {
            var betSettlement = e.GetBetSettlement();
            _log.LogInformation("[{ProducerInfo}] BetSettlement received for event {EventId} with markets {MarketsCount} {RequestId}", Helper.GetProducerInfo(betSettlement.Producer), betSettlement.Event.Id, betSettlement.Markets.Count(), Helper.GetRequestInfo(betSettlement.RequestId));
            _sportEntityWriter?.WriteData(betSettlement.Event);
            _marketWriter?.WriteMarketNamesForEvent(betSettlement.Markets);
        }

        /// <summary>
        /// Invoked when rollback bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<T> e)
        {
            var cancelRollback = e.GetBetCancelRollback();
            _log.LogInformation("[{ProducerInfo}] RollbackBetCancel received for event {EventId} with markets {MarketsCount} {RequestId}", Helper.GetProducerInfo(cancelRollback.Producer), cancelRollback.Event.Id, cancelRollback.Markets.Count(), Helper.GetRequestInfo(cancelRollback.RequestId));
            _sportEntityWriter?.WriteData(cancelRollback.Event);
            _marketWriter?.WriteMarketNamesForEvent(cancelRollback.Markets);
        }

        /// <summary>
        /// Invoked when rollback bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<T> e)
        {
            var settlementRollback = e.GetBetSettlementRollback();
            _log.LogInformation("[{ProducerInfo}] BetStop received for event {EventId} with markets {MarketsCount} {RequestId}", Helper.GetProducerInfo(settlementRollback.Producer), settlementRollback.Event.Id, settlementRollback.Markets.Count(), Helper.GetRequestInfo(settlementRollback.RequestId));
            _sportEntityWriter?.WriteData(settlementRollback.Event);
            _marketWriter?.WriteMarketNamesForEvent(settlementRollback.Markets);
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
    }
}
