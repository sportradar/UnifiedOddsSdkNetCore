/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Enumeration of all possible environments
    /// </summary>
    public enum SdkEnvironment
    {
        /// <summary>
        /// The staging
        /// </summary>
        [Obsolete("Use Integration")]
        Staging,
        /// <summary>
        /// The production
        /// </summary>
        Production,
        /// <summary>
        /// The custom
        /// </summary>
        Custom,
        /// <summary>
        /// The replay
        /// </summary>
        Replay,
        /// <summary>
        /// The integration
        /// </summary>
        Integration
    }
}
