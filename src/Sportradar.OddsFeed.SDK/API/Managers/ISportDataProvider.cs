// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Api.Managers
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProvider
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ISport}"/> representing all available sports in language specified by the <c>culture</c>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="ISport"/> instance representing the sport specified by it's id in the language specified by <c>culture</c>, or a null reference if sport with specified id does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> identifying the sport to retrieve.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{ISport}"/> representing the async operation</returns>
        Task<ISport> GetSportAsync(Urn id, CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing currently live sport events in the language specified by <c>culture</c>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ICompetition>> GetLiveSportEventsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing sport events scheduled for date specified by <c>date</c> in language specified by <c>culture</c>
        /// </summary>
        /// <param name="scheduleDate">A <see cref="DateTime"/> specifying the day for which to retrieve the schedule</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ICompetition>> GetSportEventsByDateAsync(DateTime scheduleDate, CultureInfo culture = null);

        /// <summary>
        /// Gets a <see cref="ILongTermEvent"/> representing the specified tournament in language specified by <c>culture</c> or a null reference if the tournament with
        /// specified <c>id</c> does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the tournament to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ILongTermEvent"/> representing the specified tournament or a null reference if requested tournament does not exist</returns>
        ILongTermEvent GetTournament(Urn id, CultureInfo culture = null);

        /// <summary>
        /// Gets a <see cref="ICompetition"/> representing the specified sport event in language specified by <c>culture</c> or a null reference if the sport event with
        /// specified <c>id</c> does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the sport event to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetition"/> representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        ICompetition GetCompetition(Urn id, CultureInfo culture = null);

        /// <summary>
        /// Gets a <see cref="ICompetition"/> representing the specified sport event in language specified by <c>culture</c> or a null reference if the sport event with
        /// specified <c>id</c> does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the sport event to retrieve</param>
        /// <param name="sportId">A <see cref="Urn"/> of the sport this event belongs to</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetition"/> representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        ICompetition GetCompetition(Urn id, Urn sportId, CultureInfo culture = null);

        /// <summary>
        /// Gets a <see cref="ISportEvent"/> derived class representing the specified sport event in language specified by <c>culture</c> or a null reference if the sport event with
        /// specified <c>id</c> does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the sport event to retrieve</param>
        /// <param name="sportId">A <see cref="Urn"/> of the sport this event belongs to</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ISportEvent"/> derived class representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        ISportEvent GetSportEvent(Urn id, Urn sportId = null, CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitionStatus"/> for specific sport event
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the event for which <see cref="ICompetitionStatus"/> to be retrieved</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<ICompetitionStatus> GetSportEventStatusAsync(Urn id);

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitor"/>
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id for which <see cref="ICompetitor"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetitor"/> representing the specified competitor or a null reference</returns>
        Task<ICompetitor> GetCompetitorAsync(Urn id, CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IPlayerProfile"/>
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id for which <see cref="IPlayerProfile"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="IPlayerProfile"/> representing the specified player or a null reference</returns>
        Task<IPlayerProfile> GetPlayerProfileAsync(Urn id, CultureInfo culture = null);

        /// <summary>
        /// Delete the sport event from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="ISportEvent"/> to be deleted</param>
        /// <param name="includeEventStatusDeletion">Delete also <see cref="ISportEventStatus"/> from the cache</param>
        void DeleteSportEventFromCache(Urn id, bool includeEventStatusDeletion = false);

        /// <summary>
        /// Delete the tournament from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="ILongTermEvent"/> to be deleted</param>
        void DeleteTournamentFromCache(Urn id);

        /// <summary>
        /// Delete the competitor from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="ICompetitor"/> to be deleted</param>
        void DeleteCompetitorFromCache(Urn id);

        /// <summary>
        /// Delete the player profile from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="IPlayerProfile"/> to be deleted</param>
        void DeletePlayerProfileFromCache(Urn id);

        /// <summary>
        /// Asynchronously gets a list of <see cref="IEnumerable{ICompetition}"/>
        /// </summary>
        /// <remarks>Lists almost all events we are offering prematch odds for. This endpoint can be used during early startup to obtain almost all fixtures. This endpoint is one of the few that uses pagination.</remarks>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ICompetition>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a list of active <see cref="IEnumerable{ISportEvent}"/>
        /// </summary>
        /// <remarks>Lists all <see cref="ISportEvent"/> that are cached (once schedule is loaded)</remarks>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ISportEvent>> GetActiveTournamentsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a list of available <see cref="IEnumerable{ISportEvent}"/> for a specific sport
        /// </summary>
        /// <remarks>Lists all available tournaments for a sport event we provide coverage for</remarks>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ISportEvent>> GetAvailableTournamentsAsync(Urn sportId, CultureInfo culture = null);

        /// <summary>
        /// Deletes the sport events from cache which are scheduled before specified date
        /// </summary>
        /// <param name="before">The scheduled DateTime used to delete sport events from cache</param>
        /// <returns>Number of deleted items</returns>
        int DeleteSportEventsFromCache(DateTime before);

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <param name="cacheType">Specifies what type of cache items will be exported</param>
        /// <returns>Collection of <see cref="ExportableBase"/> containing all the items currently in the cache</returns>
        Task<IEnumerable<ExportableBase>> CacheExportAsync(CacheType cacheType);

        /// <summary>
        /// Imports provided items into caches
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableBase"/> containing the items to be imported</param>
        /// <returns>No return</returns>
        Task CacheImportAsync(IEnumerable<ExportableBase> items);

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all fixtures that have changed in the last 24 hours</returns>
        Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(CultureInfo culture = null);

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all fixtures that have changed in the last 24 hours</returns>
        Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(DateTime? after, Urn sportId, CultureInfo culture = null);

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all results that have changed in the last 24 hours</returns>
        Task<IEnumerable<IResultChange>> GetResultChangesAsync(CultureInfo culture = null);

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all results that have changed in the last 24 hours</returns>
        Task<IEnumerable<IResultChange>> GetResultChangesAsync(DateTime? after, Urn sportId, CultureInfo culture = null);

        /// <summary>
        /// Gets the list of available lotteries
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of available lotteries</returns>
        Task<IEnumerable<ILottery>> GetLotteriesAsync(CultureInfo culture = null);

        /// <summary>
        /// Get sport event period summary as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="competitorIds">The list of competitor ids to fetch the results for</param>
        /// <param name="periods">The list of period ids to fetch the results for</param>
        /// <returns>The period statuses or empty if not found</returns>
        Task<IEnumerable<IPeriodStatus>> GetPeriodStatusesAsync(Urn id, CultureInfo culture = null, IEnumerable<Urn> competitorIds = null, IEnumerable<int> periods = null);

        /// <summary>
        /// Get the associated event timeline for single culture
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <returns>The event timeline or empty if not found</returns>
        Task<IEnumerable<ITimelineEvent>> GetTimelineEventsAsync(Urn id, CultureInfo culture = null);
    }
}
