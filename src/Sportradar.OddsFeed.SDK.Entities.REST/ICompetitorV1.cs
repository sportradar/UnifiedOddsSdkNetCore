/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a team competing in a sport event
    /// </summary>
    public interface ICompetitorV1 : ICompetitor
    {
        /// <summary>
        /// Gets the gender
        /// </summary>
        /// <value>The gender</value>
        string Gender { get; }
    }
}
