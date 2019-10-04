/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a match status
    /// </summary>
    /// <remarks>Interface will be merged into base <see cref="ITimelineEvent"/> in next major version scheduled for January 2019</remarks>
    /// <seealso cref="ICompetitionStatus" />
    public interface IMatchStatusV1 : IMatchStatus
    {
        /// <summary>
        /// Gets the penalty score of the home competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        int? HomePenaltyScore { get; }

        /// <summary>
        /// Gets the penalty score of the away competitor competing on the associated sport event (for Ice Hockey)
        /// </summary>
        int? AwayPenaltyScore { get; }
    }
}
