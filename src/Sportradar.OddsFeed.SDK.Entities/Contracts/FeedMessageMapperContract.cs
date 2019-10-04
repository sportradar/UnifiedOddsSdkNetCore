/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using cashout = Sportradar.OddsFeed.SDK.Messages.REST.cashout;

namespace Sportradar.OddsFeed.SDK.Entities.Contracts
{
    [ContractClassFor(typeof(IFeedMessageMapper))]
    internal abstract class FeedMessageMapperContract : IFeedMessageMapper
    {
        public ISnapshotCompleted MapSnapShotCompleted(snapshot_complete message)
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<ISnapshotCompleted>() != null);

            return Contract.Result<ISnapshotCompleted>();

        }

        public IAlive MapAlive(alive message)
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IAlive>() != null);

            return Contract.Result<IAlive>();

        }

        public IFixtureChange<T> MapFixtureChange<T>(fixture_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IFixtureChange<T>>() != null);

            return Contract.Result<IFixtureChange<T>>();

        }

        public IBetStop<T> MapBetStop<T>(bet_stop message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IBetStop<T>>() != null);

            return Contract.Result<IBetStop<T>>();

        }

        public IBetCancel<T> MapBetCancel<T>(bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IBetCancel<T>>() != null);

            return Contract.Result<IBetCancel<T>>();

        }

        public IRollbackBetCancel<T> MapRollbackBetCancel<T>(rollback_bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IRollbackBetCancel<T>>() != null);

            return Contract.Result<IRollbackBetCancel<T>>();

        }

        public IBetSettlement<T> MapBetSettlement<T>(bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IBetSettlement<T>>() != null);

            return Contract.Result<IBetSettlement<T>>();
        }

        public IRollbackBetSettlement<T> MapRollbackBetSettlement<T>(rollback_bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IRollbackBetSettlement<T>>() != null);

            return Contract.Result<IRollbackBetSettlement<T>>();

        }

        public IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<IOddsChange<T>>() != null);

            return Contract.Result<IOddsChange<T>>();

        }

        public ICashOutProbabilities<T> MapCashOutProbabilities<T>(cashout message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Contract.Requires(message != null);
            Contract.Ensures(Contract.Result<ICashOutProbabilities<T>>() != null);

            return Contract.Result<ICashOutProbabilities<T>>();
        }
    }
}
