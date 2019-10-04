/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a competing team
    /// </summary>
    public interface ITeamCompetitorV1 : ITeamCompetitor
    {
        /// <summary>
        /// Gets the division
        /// </summary>
        int? Division { get; }
    }
}
