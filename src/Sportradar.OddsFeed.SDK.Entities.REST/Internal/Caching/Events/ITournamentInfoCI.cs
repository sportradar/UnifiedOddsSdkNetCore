/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a long term event
    /// </summary>
    /// <seealso cref="SportEventCI" />
    public interface ITournamentInfoCI : ISportEventCI
    {
        /// <summary>
        /// Asynchronously gets category identifier as an asynchronous operation.
        /// </summary>
        /// <returns>A <see cref="Task{URN}" /> representing the asynchronous operation</returns>
        Task<URN> GetCategoryIdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="TournamentCoverageCI"/> providing information about coverage of the tournament
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<TournamentCoverageCI> GetTournamentCoverageAsync();

        /// <summary>
        /// Asynchronously gets a list of <see cref="CompetitorCI"/> providing information about competitors competing in a sport event
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<CompetitorCI>> GetCompetitorsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="CurrentSeasonInfoCI"/> providing information about current season
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<CurrentSeasonInfoCI> GetCurrentSeasonInfoAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a list of <see cref="GroupCI"/> providing information about groups
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<GroupCI>> GetGroupsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a list of <see cref="CompetitionCI"/> providing information about competitions
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<URN>> GetScheduleAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="RoundCI"/> providing information about current round
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<RoundCI> GetCurrentRoundAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="string"/> providing information about year
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<string> GetYearAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="TournamentInfoBasicCI"/> providing information about current round
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<TournamentInfoBasicCI> GetTournamentInfoAsync();

        /// <summary>
        /// Asynchronously gets <see cref="ReferenceIdCI"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<ReferenceIdCI> GetReferenceIdsAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="SeasonCoverageCI"/> providing information about coverage of the season
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<SeasonCoverageCI> GetSeasonCoverageAsync();

        /// <summary>
        /// Asynchronously gets a list of <see cref="URN"/> of the associated seasons
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<URN>> GetSeasonsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IDictionary<URN, ReferenceIdCI>> GetCompetitorsReferencesAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        Task<bool?> GetExhibitionGamesAsync();
    }
}
