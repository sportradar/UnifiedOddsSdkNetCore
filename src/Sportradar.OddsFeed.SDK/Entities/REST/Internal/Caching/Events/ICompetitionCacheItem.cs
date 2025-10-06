// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a competition
    /// </summary>
    internal interface ICompetitionCacheItem : ISportEventCacheItem
    {
        /// <summary>
        /// Asynchronously fetch event summary associated with the current instance (saving done in <see cref="ISportEventStatusCache"/>)
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<bool> FetchSportEventStatusAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="BookingStatus"/> enum member providing booking status for the associated entity or a null reference if booking status is not known
        /// </summary>
        /// <returns>Asynchronously returns the <see cref="BookingStatus"/> if available</returns>
        Task<BookingStatus?> GetBookingStatusAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="VenueCacheItem"/> instance representing a venue where the sport event associated with the
        /// current instance will take place
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<VenueCacheItem> GetVenueAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="SportEventConditionsCacheItem"/> instance representing live conditions of the sport event associated with the current instance
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<SportEventConditionsCacheItem> GetConditionsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{Urn}"/> providing information about competitors competing in a sport event
        /// associated with the current instance
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<Urn>> GetCompetitorsIdsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="ReferenceIdCacheItem"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<ReferenceIdCacheItem> GetReferenceIdsAsync();

        /// <summary>
        /// Asynchronously get the list of available team qualifiers
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IDictionary<Urn, string>> GetCompetitorsQualifiersAsync();

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCacheItem"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IDictionary<Urn, ReferenceIdCacheItem>> GetCompetitorsReferencesAsync();

        /// <summary>
        /// Merges the fixture
        /// </summary>
        /// <param name="fixtureDto">The fixture dto</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        void MergeFixture(FixtureDto fixtureDto, CultureInfo culture, bool useLock);

        /// <summary>
        /// Asynchronously gets a liveOdds
        /// </summary>
        /// <returns>A liveOdds</returns>
        Task<string> GetLiveOddsAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="SportEventType"/> for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="SportEventType"/> for the associated sport event.</returns>
        Task<SportEventType?> GetSportEventTypeAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="StageType"/> for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="StageType"/> for the associated sport event.</returns>
        Task<StageType?> GetStageTypeAsync();
    }
}
