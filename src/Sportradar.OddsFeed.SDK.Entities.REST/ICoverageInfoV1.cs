/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing coverage information
    /// </summary>
    public interface ICoverageInfoV1 : ICoverageInfo
    {
        /// <summary>
        /// Gets a <see cref="CoveredFrom"/> describing the coverage location
        /// </summary>
        CoveredFrom? CoveredFrom { get; }
    }
}