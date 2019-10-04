/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Defines a contract for classes implementing getting information from UF Sports API
    /// </summary>
    public interface IDataRouterManager
    {
        /// <summary>
        /// Occurs when data from Sports API arrives
        /// </summary>
        event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        /// <summary>
        /// Gets the <see cref="SportEventSummaryDTO"/> or its derived type from the summary endpoint
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetSportEventSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the <see cref="FixtureDTO"/> from the fixture endpoint
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="useCachedProvider">Should the cached provider be used</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetSportEventFixtureAsync(URN id, CultureInfo culture, bool useCachedProvider, ISportEventCI requester);

        /// <summary>
        /// Gets all tournaments for sport endpoint
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        Task GetAllTournamentsForAllSportAsync(CultureInfo culture);

        /// <summary>
        /// Gets all categories for sport endpoint
        /// </summary>
        /// <param name="id">The id of the sport to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        Task GetSportCategoriesAsync(URN id, CultureInfo culture);

        /// <summary>
        /// Gets all available sports endpoint
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        Task GetAllSportsAsync(CultureInfo culture);

        /// <summary>
        /// Gets the currently live sport events
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId each belongs to</returns>
        Task<IEnumerable<Tuple<URN, URN>>> GetLiveSportEventsAsync(CultureInfo culture);

        /// <summary>
        /// Gets the sport events for specific date
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForDateAsync(DateTime date, CultureInfo culture);

        /// <summary>
        /// Gets the sport events for specific tournament
        /// </summary>
        /// <param name="id">The id of the tournament</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The list of ids of the sport events with the sportId belonging to specified tournament</returns>
        Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the player profile endpoint
        /// </summary>
        /// <param name="id">The id of the player</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetPlayerProfileAsync(URN id, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the competitor endpoint
        /// </summary>
        /// <param name="id">The id of the competitor</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetCompetitorAsync(URN id, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the seasons for tournament
        /// </summary>
        /// <param name="id">The id of the tournament</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The list of ids of the seasons for specified tournament</returns>
        Task<IEnumerable<URN>> GetSeasonsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the information about ongoing sport event (timeline)
        /// </summary>
        /// <param name="id">The id of the sport event</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetInformationAboutOngoingEventAsync(URN id, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the market descriptions (static list of market descriptions)
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        Task GetMarketDescriptionsAsync(CultureInfo culture);

        /// <summary>
        /// Gets the variant market description (dynamic - single - variant market description)
        /// </summary>
        /// <param name="id">The id of the market</param>
        /// <param name="variant">The variant URN</param>
        /// <param name="culture">The culture to be fetched</param>
        Task GetVariantMarketDescriptionAsync(int id, string variant, CultureInfo culture);

        /// <summary>
        /// Gets the variant descriptions (static list of variant descriptions)
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        Task GetVariantDescriptionsAsync(CultureInfo culture);

        /// <summary>
        /// Gets the <see cref="DrawDTO"/> from lottery draw summary endpoint
        /// </summary>
        /// <param name="drawId">The id of the draw to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetDrawSummaryAsync(URN drawId, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the <see cref="DrawDTO"/> from the lottery draw fixture endpoint
        /// </summary>
        /// <param name="drawId">The id of the draw to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetDrawFixtureAsync(URN drawId, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the lottery draw schedule
        /// </summary>
        /// <param name="lotteryId">The id of the lottery</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The lottery with its schedule</returns>
        Task GetLotteryScheduleAsync(URN lotteryId, CultureInfo culture, ISportEventCI requester);

        /// <summary>
        /// Gets the seasons for tournament
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of ids of the seasons for specified tournament</returns>
        Task<IEnumerable<URN>> GetAllLotteriesAsync(CultureInfo culture);

        /// <summary>
        /// Gets the available selections for event
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>The available selections for event</returns>
        Task<IAvailableSelections> GetAvailableSelectionsAsync(URN id);

        /// <summary>
        /// Gets the probability calculation for the specified selections
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be calculated</param>
        /// <returns>The probability calculation for the specified selections</returns>
        Task<ICalculation> CalculateProbability(IEnumerable<ISelection> selections);

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all fixtures that have changed in the last 24 hours</returns>
        Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(CultureInfo culture);

        /// <summary>
        /// Gets the list of almost all events we are offering prematch odds for
        /// </summary>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        Task<IEnumerable<Tuple<URN, URN>>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture);

        /// <summary>
        /// Gets the list of all the available tournaments for a specific sport
        /// </summary>
        /// <param name="sportId">The specific sport id</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the available tournament ids with the sportId it belongs to</returns>
        Task<IEnumerable<Tuple<URN, URN>>> GetSportAvailableTournamentsAsync(URN sportId, CultureInfo culture);
    }
}
