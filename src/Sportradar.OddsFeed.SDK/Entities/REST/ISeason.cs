/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a tournament season
    /// </summary>
    public interface ISeason : ILongTermEvent
    {
        /// <summary>
        /// Asynchronously gets a <see cref="ISeasonCoverage"/> representing the season coverage
        /// </summary>
        /// <returns>A <see cref="Task{ISeasonCoverage}"/> representing the asynchronous operation</returns>
        Task<ISeasonCoverage> GetSeasonCoverageAsync();

        /// <summary>
        /// Asynchronously gets the list of the <see cref="IGroup"/> instances belonging to the season
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
        Task<IEnumerable<IGroup>> GetGroupsAsync();

        /// <summary>
        /// Asynchronously gets the list of all <see cref="ICompetition"/> that belongs to the season schedule
        /// </summary>
        /// <returns>The list of all <see cref="ICompetition"/> that belongs to the season schedule</returns>
        Task<IEnumerable<ICompetition>> GetScheduleAsync();

        /// <summary>
        /// Asynchronously gets the <see cref="IRound"/> specifying the current round of the tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="IRound"/> specifying the current round of the tournament associated with the current instance</returns>
        Task<IRound> GetCurrentRoundAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="string"/> representation of the current season year
        /// </summary>
        /// <returns>Asynchronously returns a <see cref="string"/> representation of the season year</returns>
        Task<string> GetYearAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="ITournamentInfo"/> representing the tournament info
        /// </summary>
        /// <returns>A <see cref="Task{ITournamentInfo}"/> representing the asynchronous operation</returns>
        Task<ITournamentInfo> GetTournamentInfoAsync();

        /// <summary>
        /// Asynchronously gets the list of competitors
        /// </summary>
        /// <returns>The list of competitors</returns>
        Task<IEnumerable<ICompetitor>> GetCompetitorsAsync();
    }
}