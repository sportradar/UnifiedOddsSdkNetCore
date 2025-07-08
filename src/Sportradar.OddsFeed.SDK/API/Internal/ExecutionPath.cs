// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// Defines the execution priority of a request.
    /// </summary>
    public enum ExecutionPath
    {
        /// <summary>
        /// A time-critical request that should fail fast if not processed quickly.
        /// </summary>
        TimeCritical,

        /// <summary>
        /// A non-time-critical request that can tolerate longer delays.
        /// </summary>
        NonTimeCritical,
    }
}
