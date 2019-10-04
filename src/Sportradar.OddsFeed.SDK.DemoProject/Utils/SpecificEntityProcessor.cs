/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// A class used to process events dispatched from the SDK
    /// </summary>
    internal class SpecificEntityProcessor<T> : IEntityProcessor where T : ISportEvent
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for logging
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static ILog _log;

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
        /// <param name="log">A <see cref="ILog"/> instance used for logging</param>
        /// <param name="dispatcher">A <see cref="ISpecificEntityDispatcher{T}"/> used to obtain SDK messages</param>
        /// <param name="sportEntityWriter">A <see cref="SportEntityWriter"/> used to write the sport entities data</param>
        /// <param name="marketWriter">A <see cref="MarketWriter"/> used to write market and outcome data</param>
        public SpecificEntityProcessor(ILog log, ISpecificEntityDispatcher<T> dispatcher, SportEntityWriter sportEntityWriter = null, MarketWriter marketWriter = null)
        {
            Contract.Requires(dispatcher != null);
            Contract.Requires(log != null);

            _log = log;
            _dispatcher = dispatcher;
            _sportEntityWriter = sportEntityWriter;
            _marketWriter = marketWriter;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_log != null);
            Contract.Invariant(_dispatcher != null);
        }

        /// <summary>
        /// Invoked when bet stop message is received
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBetStopReceived(object sender, BetStopEventArgs<T> e)
        {
            var betstop = e.GetBetStop();
            _log.Info($"BetStop received. EventId:{betstop.Event.Id} Producer:{betstop.Producer}, Tag:{betstop.Groups}, RequestId:{betstop.RequestId}");
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
            _log.Info($"OddsChange received. EventId:{oddsChange.Event.Id} Producer:{oddsChange.Producer} RequestId:{oddsChange.RequestId}");
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
            _log.Info($"BetSettlement received. EventId:{betSettlement.Event.Id} Producer:{betSettlement.Producer}, RequestId:{betSettlement.RequestId}, Market count:{betSettlement.Markets.Count()}");
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
            _log.Info($"RollbackBetSettlement received. Producer:{settlementRollback.Producer}, RequestId:{settlementRollback.RequestId}, MarketCount:{settlementRollback.Markets.Count()}");
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
            _log.Info($"BetCancel received. Producer:{betCancel.Producer}, RequestId:{betCancel.RequestId}, MarketCount:{betCancel.Markets.Count()}");
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
            _log.Info($"RollbackBetCancel received. Producer:{cancelRollback.Producer}, RequestId:{cancelRollback.RequestId}, MarketCount:{cancelRollback.Markets.Count()}");
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
            _log.Info($"FixtureChange received. Producer:{fixtureChange.Producer}, RequestId:{fixtureChange.RequestId}, EventId:{fixtureChange.Event.Id}");
            _sportEntityWriter?.WriteData(fixtureChange.Event);
        }

        /// <summary>
        /// Opens the current processor so it will start processing dispatched entities
        /// </summary>
        public void Open()
        {
            _log.Info("Attaching to dispatcher events");
            _dispatcher.OnOddsChange += OnOddsChangeReceived;
            _dispatcher.OnBetCancel += OnBetCancel;
            _dispatcher.OnRollbackBetCancel += OnRollbackBetCancel;
            _dispatcher.OnBetStop += OnBetStopReceived;
            _dispatcher.OnBetSettlement += OnBetSettlementReceived;
            _dispatcher.OnRollbackBetSettlement += OnRollbackBetSettlement;
            _dispatcher.OnFixtureChange += OnFixtureChange;

            _log.Info("Opening the dispatcher");
            _dispatcher.Open();
            _log.Info("Event processor successfully opened");
        }

        /// <summary>
        /// Closes the current processor so it will no longer process dispatched entities
        /// </summary>
        public void Close()
        {
            _log.Info("Detaching from dispatcher events");
            _dispatcher.OnOddsChange -= OnOddsChangeReceived;
            _dispatcher.OnBetCancel -= OnBetCancel;
            _dispatcher.OnRollbackBetCancel -= OnRollbackBetCancel;
            _dispatcher.OnBetStop -= OnBetStopReceived;
            _dispatcher.OnBetSettlement -= OnBetSettlementReceived;
            _dispatcher.OnRollbackBetSettlement -= OnRollbackBetSettlement;
            _dispatcher.OnFixtureChange -= OnFixtureChange;

            _log.Info("Closing the dispatcher");
            _dispatcher.Close();
            _log.Info("Event processor successfully closed");
        }
    }
}