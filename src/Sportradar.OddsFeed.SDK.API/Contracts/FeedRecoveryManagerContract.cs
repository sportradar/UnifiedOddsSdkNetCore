/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IFeedRecoveryManager))]
    internal abstract class FeedRecoveryManagerContract : IFeedRecoveryManager
    {
        [Pure]
        public bool IsOpened => Contract.Result<bool>();

        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;

        public event EventHandler<FeedCloseEventArgs> CloseFeed;

        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        public void Close() => Contract.Ensures(!IsOpened);

        public ISessionMessageManager CreateSessionMessageManager()
        {
            Contract.Ensures(Contract.Result<ISessionMessageManager>() != null);
            return Contract.Result<ISessionMessageManager>();
        }

        public void Open(IEnumerable<MessageInterest> interests)
        {
            Contract.Requires(interests != null);
            Contract.Ensures(IsOpened);
        }

        public void ConnectionShutdown()
        {

        }
    }
}
