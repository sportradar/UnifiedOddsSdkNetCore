/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Threading;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Class used to dispatch SDK non-global messages
    /// </summary>
    internal abstract class EntityDispatcher<T> : EntityDispatcherBase, IEntityDispatcherInternal, IEntityDispatcher<T> where T : ISportEvent
    {
        /// <summary>
        /// A value used to store information whether the current <see cref="OddsFeedSession"/> is opened
        /// 0 indicate closed, 1 indicate opened
        /// </summary>
        private long _isOpened;

        /// <summary>
        /// A <see cref="IFeedMessageMapper"/> used to map the feed messages to messages used by the SDK
        /// </summary>
        protected readonly IFeedMessageMapper MessageMapper;

        /// <summary>
        /// A <see cref="IEnumerable{CultureInfo}"/> specifying the default languages as specified in the configuration
        /// </summary>
        protected readonly IEnumerable<CultureInfo> DefaultCultures;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="OddsFeedSession"/> is opened
        /// </summary>
        public bool IsOpened => Interlocked.Read(ref _isOpened) == 1;

        /// <summary>
        /// Raised when a odds change message is received from the feed
        /// </summary>
        public event EventHandler<OddsChangeEventArgs<T>> OnOddsChange;

        /// <summary>
        /// Raised when a bet stop message is received from the feed
        /// </summary>
        public event EventHandler<BetStopEventArgs<T>> OnBetStop;

        /// <summary>
        /// Raised when a bet settlement message is received from the feed
        /// </summary>
        public event EventHandler<BetSettlementEventArgs<T>> OnBetSettlement;

        /// <summary>
        /// Raised when a rollback bet settlement is received from the feed
        /// </summary>
        public event EventHandler<RollbackBetSettlementEventArgs<T>> OnRollbackBetSettlement;

        /// <summary>
        /// Raised when a bet cancel message is received from the feed
        /// </summary>
        public event EventHandler<BetCancelEventArgs<T>> OnBetCancel;

        /// <summary>
        /// Raised when a rollback bet cancel message is received from the feed
        /// </summary>
        public event EventHandler<RollbackBetCancelEventArgs<T>> OnRollbackBetCancel;

        /// <summary>
        /// Raised when a fixture change message is received from the feed
        /// </summary>
        public event EventHandler<FixtureChangeEventArgs<T>> OnFixtureChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDispatcher{T}"/> class
        /// </summary>
        /// <param name="messageMapper">A <see cref="IFeedMessageMapper"/> used to map the feed messages to messages used by the SDK</param>
        /// <param name="defaultCultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the default languages as specified in the configuration</param>
        internal EntityDispatcher(IFeedMessageMapper messageMapper, IEnumerable<CultureInfo> defaultCultures)
        {
            Guard.Argument(messageMapper).NotNull();
            Guard.Argument(defaultCultures).NotNull().NotEmpty();

            MessageMapper = messageMapper;
            DefaultCultures = defaultCultures as IReadOnlyCollection<CultureInfo>;
        }

        /// <summary>
        /// Dispatches the <see cref="odds_change"/> message
        /// </summary>
        /// <param name="message">The <see cref="odds_change"/> message to dispatch</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void DispatchOddsChange(odds_change message, byte[] rawMessage)
        {
            var eventArgs = new OddsChangeEventArgs<T>(MessageMapper, message, DefaultCultures, rawMessage);
            Dispatch(OnOddsChange, eventArgs, message);
        }

        /// <summary>
        /// Dispatches the <see cref="bet_stop"/> message
        /// </summary>
        /// <param name="message">The <see cref="bet_stop"/> message to dispatch</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void DispatchBetStop(bet_stop message, byte[] rawMessage)
        {
            var eventArgs = new BetStopEventArgs<T>(MessageMapper, message, DefaultCultures, rawMessage);
            Dispatch(OnBetStop, eventArgs, message);
        }

        /// <summary>
        /// Dispatches the <see cref="bet_settlement"/> message
        /// </summary>
        /// <param name="message">The <see cref="bet_settlement"/> message to dispatch</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void DispatchBetSettlement(bet_settlement message, byte[] rawMessage)
        {
            var eventArgs = new BetSettlementEventArgs<T>(MessageMapper, message, DefaultCultures, rawMessage);
            Dispatch(OnBetSettlement, eventArgs, message);
        }

        /// <summary>
        /// Dispatches the <see cref="rollback_bet_settlement"/> message
        /// </summary>
        /// <param name="message">The <see cref="rollback_bet_settlement"/> message to dispatch</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void DispatchRollbackBetSettlement(rollback_bet_settlement message, byte[] rawMessage)
        {
            var eventArgs = new RollbackBetSettlementEventArgs<T>(MessageMapper, message, DefaultCultures, rawMessage);
            Dispatch(OnRollbackBetSettlement, eventArgs, message);
        }

        /// <summary>
        /// Dispatches the <see cref="bet_cancel"/> message
        /// </summary>
        /// <param name="message">The <see cref="bet_cancel"/> message to dispatch</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void DispatchBetCancel(bet_cancel message, byte[] rawMessage)
        {
            var eventArgs = new BetCancelEventArgs<T>(MessageMapper, message, DefaultCultures, rawMessage);
            Dispatch(OnBetCancel, eventArgs, message);
        }

        /// <summary>
        /// Dispatches the <see cref="rollback_bet_cancel"/> message
        /// </summary>
        /// <param name="message">The <see cref="rollback_bet_cancel"/> message to dispatch</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void DispatchRollbackBetCancel(rollback_bet_cancel message, byte[] rawMessage)
        {
            var eventArgs = new RollbackBetCancelEventArgs<T>(MessageMapper, message, DefaultCultures, rawMessage);
            Dispatch(OnRollbackBetCancel, eventArgs, message);
        }

        /// <summary>
        /// Dispatches the <see cref="fixture_change"/> message
        /// </summary>
        /// <param name="message">The <see cref="fixture_change"/> message to dispatch</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        private void DispatchFixtureChange(fixture_change message, byte[] rawMessage)
        {
            var eventArgs = new FixtureChangeEventArgs<T>(MessageMapper, message, DefaultCultures, rawMessage);
            Dispatch(OnFixtureChange, eventArgs, message);
        }

        /// <summary>
        /// When overridden in derived class, it executes steps needed when opening the instance
        /// </summary>
        protected abstract void OnOpening();

        /// <summary>
        /// When overridden in derived class, it executes steps needed when closing the instance
        /// </summary>
        protected abstract void OnClosing();

        /// <summary>
        /// Opens the current <see cref="IOddsFeedSession"/> instance so it will start delivering messages from the feed
        /// </summary>
        public void Open()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 1, 0) == 1)
            {
                throw new InvalidOperationException("Current OddsFeedSession is already opened");
            }

            OnOpening();
        }

        /// <summary>
        /// Closes the current <see cref="IOddsFeedSession"/> so it will no longer deliver messages
        /// </summary>
        /// <remarks>The <see cref="Close"/> method does not dispose resources associated with the current instance so the instance can be re-opened by calling the <see cref="Open"/> method. In order to dispose resources associated
        /// with the current instance you must call the <see cref="IDisposable.Dispose"/> method. Once the instance is disposed it can no longer be opened.</remarks>
        public void Close()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 0, 1) == 0)
            {
                throw new InvalidOperationException("Current OddsFeedSession is already closed");
            }

            OnClosing();
        }

        /// <summary>
        /// Dispatches the provided <see cref="FeedMessage"/>
        /// </summary>
        /// <param name="feedMessage"></param>
        /// <param name="rawMessage"></param>
        public virtual void Dispatch(FeedMessage feedMessage, byte[] rawMessage)
        {
            var oddsChange = feedMessage as odds_change;
            if (oddsChange != null)
            {
                DispatchOddsChange(oddsChange, rawMessage);
                return;
            }

            var betStop = feedMessage as bet_stop;
            if (betStop != null)
            {
                DispatchBetStop(betStop, rawMessage);
                return;
            }

            var betSettlement = feedMessage as bet_settlement;
            if (betSettlement != null)
            {
                DispatchBetSettlement(betSettlement, rawMessage);
                return;
            }

            var rollbackBetSettlement = feedMessage as rollback_bet_settlement;
            if (rollbackBetSettlement != null)
            {
                DispatchRollbackBetSettlement(rollbackBetSettlement, rawMessage);
                return;
            }

            var betCancel = feedMessage as bet_cancel;
            if (betCancel != null)
            {
                DispatchBetCancel(betCancel, rawMessage);
                return;
            }

            var rollbackBetCancel = feedMessage as rollback_bet_cancel;
            if (rollbackBetCancel != null)
            {
                DispatchRollbackBetCancel(rollbackBetCancel, rawMessage);
                return;
            }

            var fixtureChange = feedMessage as fixture_change;
            if (fixtureChange != null)
            {
                DispatchFixtureChange(fixtureChange, rawMessage);
                return;
            }
            throw new ArgumentException($"FeedMessage of type '{feedMessage.GetType().Name}' is not supported.");
        }
    }
}
