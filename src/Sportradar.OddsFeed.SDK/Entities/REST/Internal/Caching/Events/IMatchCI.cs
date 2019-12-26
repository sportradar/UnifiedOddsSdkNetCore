/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a match sport event
    /// </summary>
    public interface IMatchCI : ICompetitionCI
    {
        /// <summary>
        /// Asynchronously gets <see cref="CacheItem"/> instance providing basic information about
        /// the season to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<CacheItem> GetSeasonAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="RoundCI"/> instance describing the tournament round to which the
        /// sport event associated with current instance belongs to
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<RoundCI> GetTournamentRoundAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="URN"/> specifying the id of the tournament to which the sport event belongs to
        /// </summary>
        /// <returns>A <see cref="Task{URN}"/> representing the asynchronous operation</returns>
        Task<URN> GetTournamentIdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="IFixture"/> instance containing information about the arranged sport event
        /// </summary>
        /// <remarks>A Fixture is a sport event that has been arranged for a particular time and place</remarks>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IFixture> GetFixtureAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets the associated event timeline
        /// </summary>
        /// <remarks>The timeline is cached only after the event status indicates that the event has finished</remarks>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<EventTimelineCI> GetEventTimelineAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="DelayedInfoCI"/> instance providing delayed info
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DelayedInfoCI> GetDelayedInfoAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Merges the timeline
        /// </summary>
        /// <param name="timelineDTO">The timeline dto</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        void MergeTimeline(MatchTimelineDTO timelineDTO, CultureInfo culture, bool useLock);
    }
}