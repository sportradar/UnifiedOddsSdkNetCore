/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents an odds for a player outcome(selection)
    /// </summary>
    /// <seealso cref="IOutcomeOdds" />
    public interface IPlayerOutcomeOdds : IOutcomeOddsV1
    {
        /// <summary>
        /// Asynchronously gets the team to which the associated player belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the async operation</returns>
        Task<ITeamCompetitor> GetCompetitorAsync();

        /// <summary>
        /// Gets the value indicating whether the associated team is home or away
        /// </summary>
        /// <value>The value indicating whether the associated team is home or away</value>
        HomeAway HomeOrAwayTeam { get; }
    }
}