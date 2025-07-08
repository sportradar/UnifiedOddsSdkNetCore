﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// Defines a contract for classes implementing getting information from UF Sports API
    /// </summary>
    internal interface IDataRouterManager
    {
        /// <summary>
        /// Occurs when data from Sports API arrives
        /// </summary>
        event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        /// <summary>
        /// The exception handling strategy
        /// </summary>
        ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// Gets the <see cref="SportEventSummaryDto"/> or its derived type from the summary endpoint
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <param name="requestOptions">Request options for fetching summaries</param>
        Task GetSportEventSummaryAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester, RequestOptions requestOptions);

        /// <summary>
        /// Gets the <see cref="SportEventSummaryDto"/> or its derived type from the summary endpoint
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetSportEventSummaryAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the <see cref="FixtureDto"/> from the fixture endpoint
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="useCachedProvider">Should the cached provider be used</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetSportEventFixtureAsync(Urn id, CultureInfo culture, bool useCachedProvider, ISportEventCacheItem requester);

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
        Task GetSportCategoriesAsync(Urn id, CultureInfo culture);

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
        Task<IEnumerable<Tuple<Urn, Urn>>> GetLiveSportEventsAsync(CultureInfo culture);

        /// <summary>
        /// Gets the sport events for specific date
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        Task<IEnumerable<Tuple<Urn, Urn>>> GetSportEventsForDateAsync(DateTime date, CultureInfo culture);

        /// <summary>
        /// Gets the sport events for specific tournament
        /// </summary>
        /// <param name="id">The id of the tournament</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The list of ids of the sport events with the sportId belonging to specified tournament</returns>
        Task<IEnumerable<Tuple<Urn, Urn>>> GetSportEventsForTournamentAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the player profile endpoint
        /// </summary>
        /// <param name="id">The id of the player</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetPlayerProfileAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the competitor endpoint
        /// </summary>
        /// <param name="id">The id of the competitor</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetCompetitorAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the seasons for tournament
        /// </summary>
        /// <param name="id">The id of the tournament</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The list of ids of the seasons for specified tournament</returns>
        Task<IEnumerable<Urn>> GetSeasonsForTournamentAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the information about ongoing sport event (timeline)
        /// </summary>
        /// <param name="id">The id of the sport event</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The match timeline data object</returns>
        Task<MatchTimelineDto> GetInformationAboutOngoingEventAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the market descriptions (static list of market descriptions)
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        Task GetMarketDescriptionsAsync(CultureInfo culture);

        /// <summary>
        /// Gets the variant market description (dynamic - single - variant market description)
        /// </summary>
        /// <param name="id">The id of the market</param>
        /// <param name="variant">The variant urn</param>
        /// <param name="culture">The culture to be fetched</param>
        Task GetVariantMarketDescriptionAsync(int id, string variant, CultureInfo culture);

        /// <summary>
        /// Gets the variant descriptions (static list of variant descriptions)
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        Task GetVariantDescriptionsAsync(CultureInfo culture);

        /// <summary>
        /// Gets the <see cref="DrawDto"/> from lottery draw summary endpoint
        /// </summary>
        /// <param name="id">The id of the draw to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetDrawSummaryAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the <see cref="DrawDto"/> from the lottery draw fixture endpoint
        /// </summary>
        /// <param name="id">The id of the draw to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        Task GetDrawFixtureAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the lottery draw schedule
        /// </summary>
        /// <param name="lotteryId">The id of the lottery</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The lottery with its schedule</returns>
        Task GetLotteryScheduleAsync(Urn lotteryId, CultureInfo culture, ISportEventCacheItem requester);

        /// <summary>
        /// Gets the list of available lotteries
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="ignoreFail">if the fail should be ignored - when user does not have access</param>
        /// <returns>The list of combination of id of the lottery and associated sport id</returns>
        /// <remarks>This gets called only if WNS is available</remarks>
        Task<IEnumerable<Tuple<Urn, Urn>>> GetAllLotteriesAsync(CultureInfo culture, bool ignoreFail);

        /// <summary>
        /// Gets the available selections for event
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>The available selections for event</returns>
        Task<IAvailableSelections> GetAvailableSelectionsAsync(Urn id);

        /// <summary>
        /// Gets the probability calculation for the specified selections
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be calculated</param>
        /// <returns>The probability calculation for the specified selections</returns>
        Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections);

        /// <summary>
        /// Gets the probability calculation for the specified selections (filtered)
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be calculated</param>
        /// <returns>The probability calculation for the specified selections</returns>
        Task<ICalculationFilter> CalculateProbabilityFilteredAsync(IEnumerable<ISelection> selections);

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all fixtures that have changed in the last 24 hours</returns>
        Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(DateTime? after, Urn sportId, CultureInfo culture);
        /// <summary>
        /// Gets the list of almost all events we are offering prematch odds for
        /// </summary>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        Task<IEnumerable<Tuple<Urn, Urn>>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture);

        /// <summary>
        /// Gets the list of all the available tournaments for a specific sport
        /// </summary>
        /// <param name="sportId">The specific sport id</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the available tournament ids with the sportId it belongs to</returns>
        Task<IEnumerable<Tuple<Urn, Urn>>> GetSportAvailableTournamentsAsync(Urn sportId, CultureInfo culture);

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all results that have changed in the last 24 hours</returns>
        Task<IEnumerable<IResultChange>> GetResultChangesAsync(DateTime? after, Urn sportId, CultureInfo culture);

        /// <summary>
        /// Get stage event period summary as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <param name="competitorIds">The list of competitor ids to fetch the results for</param>
        /// <param name="periods">The list of period ids to fetch the results for</param>
        /// <returns>The periods summary or null if not found</returns>
        Task<PeriodSummaryDto> GetPeriodSummaryAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester, ICollection<Urn> competitorIds = null, ICollection<int> periods = null);
    }
}
