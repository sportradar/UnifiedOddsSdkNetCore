// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes representing additional uof sdk configuration / settings
    /// </summary>
    public interface IUofUsageConfiguration
    {
        /// <summary>
        /// Indicates if the sdk usage (metrics) export is enabled
        /// </summary>
        /// <remarks>Default value is true</remarks>
        bool IsExportEnabled { get; }

        /// <summary>
        /// Get the interval in seconds in which the usage metrics are exported
        /// </summary>
        int ExportIntervalInSec { get; }

        /// <summary>
        /// Get the timeout for exporting usage metrics
        /// </summary>
        int ExportTimeoutInSec { get; }

        /// <summary>
        /// Get the host to which the usage metrics are exported
        /// </summary>
        string Host { get; }
    }
}
