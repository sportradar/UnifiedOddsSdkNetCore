/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing jersey
    /// </summary>
    public interface IJerseyV1 : IJersey
    {
        /// <summary>
        /// Gets the stripes color of the jersey
        /// </summary>
        /// <value>The stripes color of the jersey</value>
        string StripesColor { get; }

        /// <summary>
        /// Gets the split color of the jersey
        /// </summary>
        /// <value>The split color of the jersey</value>
        string SplitColor { get; }

        /// <summary>
        /// Gets the shirt type of the jersey
        /// </summary>
        /// <value>The shirt type of the jersey</value>
        string ShirtType { get; }

        /// <summary>
        /// Gets the sleeve detail of the jersey
        /// </summary>
        /// <value>The sleeve detail of the jersey</value>
        string SleeveDetail { get; }
    }
}
