// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide health check for the SDK components
    /// </summary>
    internal interface IHealthStatusProvider : IHealthCheck
    {
    }
}
