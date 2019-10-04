/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to map (convert) feed message to those used by the SDK
    /// </summary>
    [ContractClass(typeof(FeedMessageMapperContract))]
    internal interface IFeedMessageMapper
    {
        /// <summary>
        /// Maps (converts) the provided <see cref="snapshot_complete"/> instance to the <see cref="ISnapshotCompleted"/> instance
        /// </summary>
        /// <param name="message">A <see cref="snapshot_complete"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="ISnapshotCompleted"/> instance constructed from information in the provided <see cref="snapshot_complete"/></returns>
        ISnapshotCompleted MapSnapShotCompleted(snapshot_complete message);

        /// <summary>
        /// Maps (converts) the provided <see cref="alive"/> instance to the <see cref="IAlive"/> instance
        /// </summary>
        /// <param name="message">A <see cref="alive"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="IAlive"/> instance constructed from information in the provided <see cref="alive"/></returns>
        IAlive MapAlive(alive message);

        /// <summary>
        /// Maps (converts) the provided <see cref="fixture_change"/> instance to the <see cref="IFixtureChange{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="fixture_change"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IFixtureChange{T}"/> instance constructed from information in the provided <see cref="fixture_change"/></returns>
        IFixtureChange<T> MapFixtureChange<T>(fixture_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        /// <summary>
        /// Maps (converts) the provided <see cref="bet_stop"/> instance to the <see cref="IBetStop{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="bet_stop"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IBetStop{T}"/> instance constructed from information in the provided <see cref="bet_stop"/></returns>
        IBetStop<T> MapBetStop<T>(bet_stop message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        /// <summary>
        /// Maps (converts) the provided <see cref="bet_cancel"/> instance to the <see cref="IBetCancel{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="bet_cancel"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IBetCancel{T}"/> instance constructed from information in the provided <see cref="bet_cancel"/></returns>
        IBetCancel<T> MapBetCancel<T>(bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        /// <summary>
        /// Maps (converts) the provided <see cref="rollback_bet_cancel"/> instance to the <see cref="IRollbackBetCancel{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="rollback_bet_cancel"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IRollbackBetCancel{T}"/> instance constructed from information in the provided <see cref="rollback_bet_cancel"/></returns>
        IRollbackBetCancel<T> MapRollbackBetCancel<T>(rollback_bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        /// <summary>
        /// Maps (converts) the provided <see cref="bet_settlement"/> instance to the <see cref="IBetSettlement{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="bet_settlement"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IBetSettlement{T}"/> instance constructed from information in the provided <see cref="bet_settlement"/></returns>
        IBetSettlement<T> MapBetSettlement<T>(bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        /// <summary>
        /// Maps (converts) the provided <see cref="rollback_bet_settlement"/> instance to the <see cref="IRollbackBetSettlement{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="rollback_bet_settlement"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IRollbackBetSettlement{T}"/> instance constructed from information in the provided <see cref="rollback_bet_settlement"/></returns>
        IRollbackBetSettlement<T> MapRollbackBetSettlement<T>(rollback_bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        /// <summary>
        /// Maps (converts) the provided <see cref="odds_change"/> instance to the <see cref="IOddsChange{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="odds_change"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IOddsChange{T}"/> instance constructed from information in the provided <see cref="odds_change"/></returns>
        IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;

        /// <summary>
        /// Maps (converts) the provided <see cref="cashout"/> instance to the <see cref="ICashOutProbabilities{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="cashout"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="ICashOutProbabilities{T}"/> instance constructed from information in the provided <see cref="cashout"/></returns>
        ICashOutProbabilities<T> MapCashOutProbabilities<T>(cashout message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent;
    }
}