/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IProducersProvider))]
    internal abstract class ProducersProviderContract : IProducersProvider
    {
        public Task<IEnumerable<IProducer>> GetProducersAsync()
        {
            return Contract.Result<Task<IEnumerable<IProducer>>>();
        }

        public IEnumerable<IProducer> GetProducers()
        {
            return Contract.Result<IEnumerable<IProducer>>();
        }
    }
}