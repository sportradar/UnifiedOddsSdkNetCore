/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    public interface ISportDataProvider
    {
        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ISport}"/> representing all available sports in language specified by the <code>culture</code>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="ISport"/> instance representing the sport specified by it's id in the language specified by <code>culture</code>, or a null reference if sport with specified id does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> identifying the sport to retrieve.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{ISport}"/> representing the async operation</returns>
        Task<ISport> GetSportAsync(URN id, CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing currently live sport events in the language specified by <code>culture</code>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ICompetition>> GetLiveSportEventsAsync(CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing sport events scheduled for date specified by <code>date</code> in language specified by <code>culture</code>
        /// </summary>
        /// <param name="date">A <see cref="DateTime"/> specifying the day for which to retrieve the schedule</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<ICompetition>> GetSportEventsByDateAsync(DateTime date, CultureInfo culture = null);

        /// <summary>
        /// Gets a <see cref="ILongTermEvent"/> representing the specified tournament in language specified by <code>culture</code> or a null reference if the tournament with
        /// specified <code>id</code> does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the tournament to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ILongTermEvent"/> representing the specified tournament or a null reference if requested tournament does not exist</returns>
        ILongTermEvent GetTournament(URN id, CultureInfo culture = null);

        /// <summary>
        /// Gets a <see cref="ICompetition"/> representing the specified sport event in language specified by <code>culture</code> or a null reference if the sport event with
        /// specified <code>id</code> does not exist
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the sport event to retrieve</param>
        /// <param name="sportId">A <see cref="URN"/> of the sport this event belongs to</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetition"/> representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        ICompetition GetCompetition(URN id, URN sportId, CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitionStatus"/> for specific sport event
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the event for which <see cref="ICompetitionStatus"/> to be retrieved</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<ICompetitionStatus> GetSportEventStatusAsync(URN id);

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitor"/>
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id for which <see cref="ICompetitor"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetitor"/> representing the specified competitor or a null reference</returns>
        Task<ICompetitor> GetCompetitorAsync(URN id, CultureInfo culture = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IPlayerProfile"/>
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id for which <see cref="IPlayerProfile"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="IPlayerProfile"/> representing the specified player or a null reference</returns>
        Task<IPlayerProfile> GetPlayerProfileAsync(URN id, CultureInfo culture = null);

        /// <summary>
        /// Delete the sport event from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="ISportEvent"/> to be deleted</param>
        void DeleteSportEventFromCache(URN id);

        /// <summary>
        /// Delete the tournament from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="ILongTermEvent"/> to be deleted</param>
        void DeleteTournamentFromCache(URN id);

        /// <summary>
        /// Delete the competitor from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="ICompetitor"/> to be deleted</param>
        void DeleteCompetitorFromCache(URN id);

        /// <summary>
        /// Delete the player profile from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of <see cref="IPlayerProfile"/> to be deleted</param>
        void DeletePlayerProfileFromCache(URN id);
    }
}
