using System;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes representing additional uof sdk configuration / settings
    /// </summary>
    public interface IUofAdditionalConfiguration
    {
        /// <summary>
        /// Gets the timeout for automatically collecting statistics
        /// </summary>
        TimeSpan StatisticsInterval { get; }

        /// <summary>
        /// Indicates if the market mapping should be included when requesting market descriptions from API
        /// </summary>
        /// <remarks>False - market mappings are included (default)</remarks>
        bool OmitMarketMappings { get; }
    }
}
