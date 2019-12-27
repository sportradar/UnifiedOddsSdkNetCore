/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics.Health;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Metrics
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide <see cref="HealthCheckResult"/> for the SDK
    /// </summary>
    internal interface IHealthStatusProvider
    {
        /// <summary>
        /// Registers the health check which will be periodically triggered
        /// </summary>
        void RegisterHealthCheck();

        /// <summary>
        /// Starts the health check and returns <see cref="HealthCheckResult"/>
        /// </summary>
        HealthCheckResult StartHealthCheck();
    }
}
