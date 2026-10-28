// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Common.Enums
{
    /// <summary>
    /// Enumeration of all possible environments
    /// </summary>
    public enum SdkEnvironment
    {
        /// <summary>
        /// The production
        /// </summary>
        Production,

        /// <summary>
        /// The custom
        /// </summary>
        Custom,

        /// <summary>
        /// The replay with integration credentials
        /// </summary>
        ReplayWithIntegrationCredentials,

        /// <summary>
        /// The replay with production credentials
        /// </summary>
        ReplayWithProductionCredentials,

        /// <summary>
        /// The integration
        /// </summary>
        Integration,

        /// <summary>
        /// The global production
        /// </summary>
        GlobalProduction,

        /// <summary>
        /// The global integration
        /// </summary>
        GlobalIntegration
    }
}
