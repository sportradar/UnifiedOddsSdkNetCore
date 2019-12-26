/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing period statistics
    /// </summary>
    public interface IPeriodStatistics
    {
        /// <summary>
        /// Gets the name of the period
        /// </summary>
        /// <value>The name of the period</value>
        string PeriodName { get; }

        /// <summary>
        /// Gets the list of team statistics for specific period
        /// </summary>
        /// <value>The list of team statistics for specific period</value>
        IEnumerable<ITeamStatistics> TeamStatistics { get; }
    }
}
