/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IProducerRecoveryManagerFactory))]
    internal abstract class ProducerRecoveryManagerFactoryContract : IProducerRecoveryManagerFactory
    {
        public IProducerRecoveryManager GetRecoveryTracker(IProducer producer, IEnumerable<MessageInterest> allInterests)
        {
            Contract.Ensures(Contract.Result<IProducerRecoveryManager>() != null);
            return Contract.Result<IProducerRecoveryManager>();
        }

        public ISessionMessageManager CreateSessionMessageManager()
        {
            Contract.Ensures(Contract.Result<ISessionMessageManager>() != null);
            return Contract.Result<ISessionMessageManager>();
        }
    }
}
