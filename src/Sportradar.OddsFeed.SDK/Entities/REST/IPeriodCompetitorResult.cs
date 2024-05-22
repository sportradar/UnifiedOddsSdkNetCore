// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes representing competitor result per period
    /// </summary>
    public interface IPeriodCompetitorResult
    {
        /// <summary>
        /// Gets the competitor id
        /// </summary>
        /// <value>The competitor id</value>
        Urn Id { get; }

        /// <summary>
        /// Gets the competitor results
        /// </summary>
        /// <value>The results</value>
        IEnumerable<ICompetitorResult> CompetitorResults { get; }
    }
}
