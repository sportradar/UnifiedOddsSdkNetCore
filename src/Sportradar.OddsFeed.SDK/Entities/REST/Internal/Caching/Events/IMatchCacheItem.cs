// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a match sport event
    /// </summary>
    internal interface IMatchCacheItem : ICompetitionCacheItem
    {
        /// <summary>
        /// Asynchronously gets <see cref="SeasonCacheItem"/> instance providing basic information about
        /// the season to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<SeasonCacheItem> GetSeasonAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="RoundCacheItem"/> instance describing the tournament round to which the
        /// sport event associated with current instance belongs to
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<RoundCacheItem> GetTournamentRoundAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="Urn"/> specifying the id of the tournament to which the sport event belongs to
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{Urn}"/> representing the asynchronous operation</returns>
        Task<Urn> GetTournamentIdAsync(IEnumerable<CultureInfo> cultures);

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
        Task<EventTimelineCacheItem> GetEventTimelineAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="DelayedInfoCacheItem"/> instance providing delayed info
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<DelayedInfoCacheItem> GetDelayedInfoAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="CoverageInfoDto"/> instance providing coverage info
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<CoverageInfoCacheItem> GetCoverageInfoAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Merges the timeline
        /// </summary>
        /// <param name="timelineDto">The timeline dto</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        void MergeTimeline(MatchTimelineDto timelineDto, CultureInfo culture, bool useLock);
    }
}
