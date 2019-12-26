/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines an interface representing season coverage info
    /// </summary>
    public interface ISeasonCoverage
    {
        /// <summary>
        /// Gets the identifier of the season
        /// </summary>
        URN SeasonId { get; }

        /// <summary>
        /// Gets the string representation of the maximum coverage available for the season associated with the current instance
        /// </summary>
        string MaxCoverageLevel { get; }

        /// <summary>
        /// Gets the name of the minimum coverage guaranteed for the season associated with the current instance
        /// </summary>
        string MinCoverageLevel { get; }

        /// <summary>
        /// Gets the max covered value
        /// </summary>
        int? MaxCovered { get; }

        /// <summary>
        /// Gets the played value
        /// </summary>
        int Played { get; }

        /// <summary>
        /// Gets the scheduled value
        /// </summary>
        int Scheduled { get; }
    }
}
