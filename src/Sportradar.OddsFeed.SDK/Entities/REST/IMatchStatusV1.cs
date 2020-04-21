/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a match status
    /// </summary>
    /// <remarks>Interface will be merged into base <see cref="IMatchStatus"/> in next major version</remarks>
    /// <seealso cref="ICompetitionStatus" />
    public interface IMatchStatusV1 : IMatchStatus
    {
        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the home competitor competing on the associated sport event</value>
        new decimal? HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the away competitor competing on the associated sport event</value>
        new decimal? AwayScore { get; }
    }
}
