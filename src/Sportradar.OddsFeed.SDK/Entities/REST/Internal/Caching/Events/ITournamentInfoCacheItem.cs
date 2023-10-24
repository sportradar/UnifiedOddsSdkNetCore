/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a long term event
    /// </summary>
    /// <seealso cref="SportEventCacheItem" />
    internal interface ITournamentInfoCacheItem : ISportEventCacheItem
    {
        /// <summary>
        /// Asynchronously gets category identifier as an asynchronous operation.
        /// </summary>
        /// <returns>A <see cref="Task{Urn}" /> representing the asynchronous operation</returns>
        Task<Urn> GetCategoryIdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="TournamentCoverageCacheItem"/> providing information about coverage of the tournament
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<TournamentCoverageCacheItem> GetTournamentCoverageAsync();

        /// <summary>
        /// Asynchronously gets a list of <see cref="CompetitorCacheItem"/> ids providing information about competitors competing in a sport event
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<Urn>> GetCompetitorsIdsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a list of <see cref="CompetitorCacheItem"/> ids providing information about competitors competing in a sport event
        /// </summary>
        /// <param name="culture">A languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<Urn>> GetCompetitorsIdsAsync(CultureInfo culture);

        /// <summary>
        /// Asynchronously gets a <see cref="CurrentSeasonInfoCacheItem"/> providing information about current season
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<CurrentSeasonInfoCacheItem> GetCurrentSeasonInfoAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a list of <see cref="GroupCacheItem"/> providing information about groups
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<GroupCacheItem>> GetGroupsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a list of <see cref="CompetitionCacheItem"/> providing information about competitions
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<Urn>> GetScheduleAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="RoundCacheItem"/> providing information about current round
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<RoundCacheItem> GetCurrentRoundAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="string"/> providing information about year
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<string> GetYearAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="TournamentInfoBasicCacheItem"/> providing information about current round
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<TournamentInfoBasicCacheItem> GetTournamentInfoAsync();

        /// <summary>
        /// Asynchronously gets <see cref="ReferenceIdCacheItem"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<ReferenceIdCacheItem> GetReferenceIdsAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="SeasonCoverageCacheItem"/> providing information about coverage of the season
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<SeasonCoverageCacheItem> GetSeasonCoverageAsync();

        /// <summary>
        /// Asynchronously gets a list of <see cref="Urn"/> of the associated seasons
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}"/> representing an async operation</returns>
        Task<IEnumerable<Urn>> GetSeasonsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCacheItem"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IDictionary<Urn, ReferenceIdCacheItem>> GetCompetitorsReferencesAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        Task<bool?> GetExhibitionGamesAsync();
    }
}
