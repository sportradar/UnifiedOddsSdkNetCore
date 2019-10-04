/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IProducerRecoveryManager))]
    internal abstract class ProducerRecoveryManagerContract : IProducerRecoveryManager
    {
        public IProducer Producer
        {
            [Pure]
            get
            {
                return Contract.Result<IProducer>();
            }
        }

        public ProducerRecoveryStatus Status
        {
            [Pure]
            get
            {
                return Contract.Result<ProducerRecoveryStatus>();
            }

        }

        public event EventHandler<TrackerStatusChangeEventArgs> StatusChanged;

        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        public void CheckStatus()
        {
        }

        public void ProcessUserMessage(FeedMessage message, MessageInterest interest)
        {
            Contract.Requires(message != null);
            Contract.Requires(interest != null);
        }

        public void ProcessSystemMessage(FeedMessage message)
        {
            Contract.Requires(message != null);
        }

        public void ConnectionShutdown()
        {
        }
    }
}
