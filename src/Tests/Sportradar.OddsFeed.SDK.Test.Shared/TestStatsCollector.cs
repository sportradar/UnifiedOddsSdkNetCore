/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using App.Metrics.Health;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestStatsCollector : IHealthStatusProvider
    {
        public void RegisterHealthCheck()
        {
            //HealthChecks.RegisterHealthCheck("TestStatsCollector", new Func<HealthCheckResult>(StartHealthCheck));
        }

        public HealthCheckResult StartHealthCheck()
        {
            return StaticRandom.I1000 > 500 ? HealthCheckResult.Healthy("Ok") : HealthCheckResult.Unhealthy("Not OK");
        }
    }
}
