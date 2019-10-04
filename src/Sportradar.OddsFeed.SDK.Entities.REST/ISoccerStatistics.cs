/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing soccer match specific statistics
    /// </summary>
    public interface ISoccerStatistics
    {
        /// <summary>
        /// Gets the list of complete team statistics data
        /// </summary>
        /// <value>The list of complete team statistics data</value>
        IEnumerable<ITeamStatistics> TotalStatistics { get; }

        /// <summary>
        /// Gets the list of separate period statistics
        /// </summary>
        /// <value>The list of separate period statistics</value>
        IEnumerable<IPeriodStatistics> PeriodStatistics { get; }
    }
}
