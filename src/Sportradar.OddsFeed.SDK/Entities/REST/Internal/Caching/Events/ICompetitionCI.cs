/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a competition
    /// </summary>
    internal interface ICompetitionCI : ISportEventCI
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
        /// Asynchronously gets a <see cref="VenueCI"/> instance representing a venue where the sport event associated with the
        /// current instance will take place
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<VenueCI> GetVenueAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="SportEventConditionsCI"/> instance representing live conditions of the sport event associated with the current instance
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<SportEventConditionsCI> GetConditionsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{URN}"/> providing information about competitors competing in a sport event
        /// associated with the current instance
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IEnumerable<URN>> GetCompetitorsIdsAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets <see cref="ReferenceIdCI"/> associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<ReferenceIdCI> GetReferenceIdsAsync();

        /// <summary>
        /// Asynchronously get the list of available team qualifiers
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IDictionary<URN, string>> GetCompetitorsQualifiersAsync();

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IDictionary<URN, ReferenceIdCI>> GetCompetitorsReferencesAsync();

        /// <summary>
        /// Merges the fixture
        /// </summary>
        /// <param name="fixtureDTO">The fixture dto</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        void MergeFixture(FixtureDTO fixtureDTO, CultureInfo culture, bool useLock);

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

        /// <summary>
        /// Asynchronously get the list of competitors marked as virtual
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        Task<IList<URN>> GetCompetitorsVirtualAsync();
    }
}