/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IOddsFeedSession))]
    internal abstract class OddsFeedSessionContract : IOddsFeedSession
    {
        [Pure]
        public string Name
        {

            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return Contract.Result<string>();
            }
        }

        public event EventHandler<OddsChangeEventArgs<ISportEvent>> OnOddsChange;

        public event EventHandler<BetStopEventArgs<ISportEvent>> OnBetStop;

        public event EventHandler<BetSettlementEventArgs<ISportEvent>> OnBetSettlement;

        public event EventHandler<RollbackBetSettlementEventArgs<ISportEvent>> OnRollbackBetSettlement;

        public event EventHandler<BetCancelEventArgs<ISportEvent>> OnBetCancel;

        public event EventHandler<RollbackBetCancelEventArgs<ISportEvent>> OnRollbackBetCancel;

        public event EventHandler<FixtureChangeEventArgs<ISportEvent>> OnFixtureChange;

        public event EventHandler<UnparsableMessageEventArgs> OnUnparsableMessageReceived;

        public ISpecificEntityDispatcher<T> CreateSportSpecificMessageDispatcher<T>() where T : ISportEvent
        {
            Contract.Ensures(Contract.Result<IEntityDispatcher<T>>() != null);
            return Contract.Result<ISpecificEntityDispatcher<T>>();
        }
    }
}
