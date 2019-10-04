/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(ISessionMessageManager))]
    internal abstract class SessionMessageManagerContract : ISessionMessageManager
    {
        public void StashMessages(IProducer producer, long requestId)
        {
            Contract.Requires(producer != null);
            //Contract.Requires(requestId > 0);
        }

        public void ReleaseMessages(IProducer producer, long requestId)
        {
            Contract.Requires(producer != null);
            //Contract.Requires(requestId > 0);
        }

        //public event EventHandler<AliveEventArgs> AliveReceived;

        //public event EventHandler<SnapshotCompleteEventArgs> SnapshotCompleteReceived;

        //public event EventHandler<ProducerStatusChangeEventArgs> ProducerDownReceived;

        public event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;
    }
}
