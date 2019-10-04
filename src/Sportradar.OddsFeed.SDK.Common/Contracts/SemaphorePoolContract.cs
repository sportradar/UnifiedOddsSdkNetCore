/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Contracts
{
    [ContractClassFor(typeof(ISemaphorePool))]
    internal abstract class SemaphorePoolContract : ISemaphorePool
    {
        public void Dispose()
        {

        }

        public Task<SemaphoreSlim> Acquire(string id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Ensures(Contract.Result<Task<SemaphoreSlim>>() != null);
            return Contract.Result<Task<SemaphoreSlim>>();
        }

        public void Release(string id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
        }
    }
}
