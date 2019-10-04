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
    public interface IMatchStatusV2 : IMatchStatusV1
    {
        /// <summary>
        /// Gets the indicator wither the event is decided by fed
        /// </summary>
        bool? DecidedByFed { get; }
    }
}
