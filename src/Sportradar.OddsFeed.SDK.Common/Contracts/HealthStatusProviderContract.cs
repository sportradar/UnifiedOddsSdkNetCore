/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using App.Metrics.Health;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Contracts
{
    [ContractClassFor(typeof(IHealthStatusProvider))]
    internal abstract class HealthStatusProviderContract : IHealthStatusProvider
    {
        public void RegisterHealthCheck()
        {
        }

        public HealthCheckResult StartHealthCheck()
        {
            return Contract.Result<HealthCheckResult>();
        }
    }
}
