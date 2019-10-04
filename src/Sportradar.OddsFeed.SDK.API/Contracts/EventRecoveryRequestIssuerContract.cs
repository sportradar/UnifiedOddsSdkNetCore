/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IEventRecoveryRequestIssuer))]
    internal abstract class EventRecoveryRequestIssuerContract : IEventRecoveryRequestIssuer
    {
        public Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId)
        {
            Contract.Requires(producer != null);
            Contract.Requires(eventId != null);
            Contract.Ensures(Contract.Result<long>() > 0);
            return Contract.Result<Task<long>>();
        }

        public Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId)
        {
            Contract.Requires(producer != null);
            Contract.Requires(eventId != null);
            Contract.Ensures(Contract.Result<long>() > 0);
            return Contract.Result<Task<long>>();
        }
    }
}
