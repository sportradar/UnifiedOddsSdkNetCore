/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// Class used to handle messages dispatched by non-specific entity dispatchers - the event is always represented as <see cref="ISportEvent"/>
    /// </summary>
    internal class EntityProcessor<T> : IEntityProcessor where T : ISportEvent
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for logging
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Log = LogManager.GetLogger("EntityProcessor");

        /// <summary>
        /// A <see cref="IEntityDispatcher{ISportEvent}"/> used to obtain SDK messages
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
        /// Initializes a new instance of the <see cref="EntityProcessor"/> class
        /// </summary>
        /// <param name="dispatcher">A <see cref="IEntityDispatcher{ISportEvent}" /> whose dispatched entities will be processed by the current instance</param>
        /// <param name="writer">A <see cref="SportEntityWriter" /> used to output / log the dispatched entities</param>
        /// <param name="marketWriter">A <see cref="MarketWriter"/> used to write market and outcome data</param>
        public EntityProcessor(IEntityDispatcher<T> dispatcher, SportEntityWriter writer = null, MarketWriter marketWriter = null)
        {
            Contract.Requires(dispatcher != null);

            _dispatcher = dispatcher;
            _sportEntityWriter = writer;
            _marketWriter = marketWriter;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Log != null);
            Contract.Invariant(_dispatcher != null);
            Contract.Invariant(_sportEntityWriter != null);
        }

        /// <summary>
        /// Invoked when bet stop message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBetStopReceived(object sender, BetStopEventArgs<T> e)
        {
            var betstop = e.GetBetStop();
            Log.Info($"BetStop received. EventId:{betstop.Event.Id} Producer:{betstop.Producer}, Tag:{betstop.Groups}, RequestId:{betstop.RequestId}");
            _sportEntityWriter?.WriteData(betstop.Event);
        }

        /// <summary>
        /// Invoked when odds change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnOddsChangeReceived(object sender, OddsChangeEventArgs<T> e)
        {
            Contract.Requires(e != null);

            var oddsChange = e.GetOddsChange();
            Log.Info($"OddsChange received. EventId:{oddsChange.Event.Id} Producer:{oddsChange.Producer} RequestId:{oddsChange.RequestId}");
            _sportEntityWriter?.WriteData(oddsChange.Event);
            _marketWriter?.WriteMarketNamesForEvent(oddsChange.Markets);
        }

        /// <summary>
        /// Invoked when bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetSettlementReceived(object sender, BetSettlementEventArgs<T> e)
        {
            var betSettlement = e.GetBetSettlement();
            Log.Info($"BetSettlement received. EventId:{betSettlement.Event.Id} Producer:{betSettlement.Producer}, RequestId:{betSettlement.RequestId}, Market count:{betSettlement.Markets.Count()}");
            _sportEntityWriter?.WriteData(betSettlement.Event);
            _marketWriter?.WriteMarketNamesForEvent(betSettlement.Markets);
        }

        /// <summary>
        /// Invoked when rollback bet settlement message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<T> e)
        {
            var settlementRollback = e.GetBetSettlementRollback();
            Log.Info($"RollbackBetSettlement received. Producer:{settlementRollback.Producer}, RequestId:{settlementRollback.RequestId}, MarketCount:{settlementRollback.Markets.Count()}");
            _sportEntityWriter?.WriteData(settlementRollback.Event);
            _marketWriter?.WriteMarketNamesForEvent(settlementRollback.Markets);
        }

        /// <summary>
        /// Invoked when bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnBetCancel(object sender, BetCancelEventArgs<T> e)
        {
            var betCancel = e.GetBetCancel();
            Log.Info($"BetCancel received. Producer:{betCancel.Producer}, RequestId:{betCancel.RequestId}, MarketCount:{betCancel.Markets.Count()}");
            _sportEntityWriter?.WriteData(betCancel.Event);
            _marketWriter?.WriteMarketNamesForEvent(betCancel.Markets);
        }

        /// <summary>
        /// Invoked when rollback bet cancel message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<T> e)
        {
            var cancelRollback = e.GetBetCancelRollback();
            Log.Info($"RollbackBetCancel received. Producer:{cancelRollback.Producer}, RequestId:{cancelRollback.RequestId}, MarketCount:{cancelRollback.Markets.Count()}");
            _sportEntityWriter?.WriteData(cancelRollback.Event);
            _marketWriter?.WriteMarketNamesForEvent(cancelRollback.Markets);
        }

        /// <summary>
        /// Invoked when fixture change message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnFixtureChange(object sender, FixtureChangeEventArgs<T> e)
        {
            var fixtureChange = e.GetFixtureChange();
            Log.Info($"FixtureChange received. Producer:{fixtureChange.Producer}, RequestId:{fixtureChange.RequestId}, EventId:{fixtureChange.Event.Id}");
            _sportEntityWriter?.WriteData(fixtureChange.Event);
        }

        /// <summary>
        /// Opens the current processor so it will start processing dispatched entities.
        /// </summary>
        public void Open()
        {
            Log.Info("Attaching to session events");
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
            Log.Info("Detaching from session events");
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
