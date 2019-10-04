/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a team competing in a sport event
    /// </summary>
    public interface ICompetitorV2 : ICompetitorV1
    {
        /// <summary>
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        IRaceDriverProfile RaceDriverProfile { get; }
    }
}
