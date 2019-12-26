/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport events of match type
    /// </summary>
    public interface IMatch : ICompetition
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IMatchStatus"/> containing information about the progress of the match
        /// </summary>
        /// <returns>A <see cref="Task{IMatchStatus}"/> containing information about the progress of the match</returns>
        new Task<IMatchStatus> GetStatusAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor"/> representing home competitor of the match associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        Task<ITeamCompetitor> GetHomeCompetitorAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor"/> representing away competitor of the match associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        Task<ITeamCompetitor> GetAwayCompetitorAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="ISeasonInfo"/> representing the season to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISeasonInfo}"/> representing the retrieval operation</returns>
        Task<ISeasonInfo> GetSeasonAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="IRound"/> representing the tournament round to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{IRound}"/> representing the retrieval operation</returns>
        Task<IRound> GetTournamentRoundAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="ILongTermEvent"/> representing the season to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ILongTermEvent}"/> representing the retrieval operation</returns>
        Task<ILongTermEvent> GetTournamentAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="IFixture"/> instance containing information about the arranged sport event
        /// </summary>
        /// <returns>A <see cref="Task{IFixture}"/> representing the retrieval operation</returns>
        /// <remarks>A Fixture is a sport event that has been arranged for a particular time and place</remarks>
        Task<IFixture> GetFixtureAsync();

        /// <summary>
        /// Asynchronously gets the associated event timeline
        /// </summary>
        /// <returns>A <see cref="Task{IEventTimeline}"/> representing the retrieval operation</returns>
        Task<IEventTimeline> GetEventTimelineAsync();

        /// <summary>
        /// Asynchronously gets the associated delayed info
        /// </summary>
        /// <returns>A <see cref="Task{IDelayedInfo}"/> representing the retrieval operation</returns>
        Task<IDelayedInfo> GetDelayedInfoAsync();
    }
}
