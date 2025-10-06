// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes providing coverage information
    /// </summary>
    public interface ICoverageInfo : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="string"/> describing the level of the available coverage
        /// </summary>
        string Level { get; }

        /// <summary>
        /// Gets a value indicating whether the coverage represented by current <see cref="ICoverageInfo"/> is live coverage
        /// </summary>
        bool IsLive { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{String}"/> specifying what is included in the coverage represented by the
        /// current <see cref="ICoverageInfo"/> instance
        /// </summary>
        IEnumerable<string> Includes { get; }

        /// <summary>
        /// Gets a <see cref="CoveredFrom"/> describing the coverage location
        /// </summary>
        CoveredFrom? CoveredFrom { get; }
    }
}
