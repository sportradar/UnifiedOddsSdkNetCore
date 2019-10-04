/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing team statistics
    /// </summary>
    public interface ITeamStatisticsV1 : ITeamStatistics
    {
        /// <summary>
        /// Gets the team id
        /// </summary>
        /// <value>The team id</value>
        URN TeamId { get; }

        /// <summary>
        /// Gets the name
        /// </summary>
        /// <value>The name</value>
        string Name { get; }
    }
}
