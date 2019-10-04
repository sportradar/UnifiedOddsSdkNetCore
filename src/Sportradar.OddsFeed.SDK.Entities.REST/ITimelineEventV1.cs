/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing timeline event
    /// </summary>
    /// <remarks>Interface will be merged into base <see cref="ITimelineEvent"/> in next major version scheduled for January 2019</remarks>
    public interface ITimelineEventV1 : ITimelineEvent
    {
        /// <summary>
        /// Gets the x
        /// </summary>
        /// <value>The x</value>
        new int? X { get; }
        /// <summary>
        /// Gets the y
        /// </summary>
        /// <value>The y</value>
        new int? Y { get; }
        /// <summary>
        /// Gets the match status code
        /// </summary>
        /// <value>The match status code</value>
        int? MatchStatusCode { get; }
        /// <summary>
        /// Gets the match clock
        /// </summary>
        /// <value>The match clock</value>
        string MatchClock { get; }
    }
}
