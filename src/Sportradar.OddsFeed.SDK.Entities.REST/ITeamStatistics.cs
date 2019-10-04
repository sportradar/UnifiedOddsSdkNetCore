/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing team statistics
    /// </summary>
    public interface ITeamStatistics
    {
        /// <summary>
        /// Gets an indication if the statistics are for the home or away team
        /// </summary>
        /// <value>An indication if the statistics are for the home or away team</value>
        HomeAway? HomeAway { get; }

        /// <summary>
        /// Gets the total count of received cards
        /// </summary>
        /// <value>The total count of received cards</value>
        int? Cards { get; }

        /// <summary>
        /// Gets the total count of yellow cards
        /// </summary>
        /// <value>The total count of yellow cards</value>
        int? YellowCards { get; }

        /// <summary>
        /// Gets the total count of red cards
        /// </summary>
        /// <value>The total count of red cards</value>
        int? RedCards { get; }

        /// <summary>
        /// Gets the total count of yellow-red cards
        /// </summary>
        /// <value>The total count of yellow-red cards</value>
        int? YellowRedCards { get; }

        /// <summary>
        /// Gets the total count of corner kicks
        /// </summary>
        /// <value>The total count of corner kicks</value>
        int? CornerKicks { get; }
    }
}
