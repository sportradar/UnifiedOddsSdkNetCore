/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Specifies a contract defining events used for user notification
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of sport associated with <see cref="IEntityDispatcher{T}"/></typeparam>
    public interface IEntityDispatcher<T> where T : ISportEvent
    {
        /// <summary>
        /// Raised when a odds change message is received from the feed
        /// </summary>
        event EventHandler<OddsChangeEventArgs<T>> OnOddsChange;

        /// <summary>
        /// Raised when a bet stop message is received from the feed
        /// </summary>
        event EventHandler<BetStopEventArgs<T>> OnBetStop;

        /// <summary>
        /// Raised when a bet settlement message is received from the feed
        /// </summary>
        event EventHandler<BetSettlementEventArgs<T>> OnBetSettlement;

        /// <summary>
        /// Raised when a rollback bet settlement is received from the feed
        /// </summary>
        event EventHandler<RollbackBetSettlementEventArgs<T>> OnRollbackBetSettlement;

        /// <summary>
        /// Raised when a bet cancel message is received from the feed
        /// </summary>
        event EventHandler<BetCancelEventArgs<T>> OnBetCancel;

        /// <summary>
        /// Raised when a rollback bet cancel message is received from the feed
        /// </summary>
        event EventHandler<RollbackBetCancelEventArgs<T>> OnRollbackBetCancel;

        /// <summary>
        /// Raised when a fixture change message is received from the feed
        /// </summary>
        event EventHandler<FixtureChangeEventArgs<T>> OnFixtureChange;
    }
}
