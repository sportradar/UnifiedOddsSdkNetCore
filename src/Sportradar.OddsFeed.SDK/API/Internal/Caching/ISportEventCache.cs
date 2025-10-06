// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache instances
    /// </summary>
    internal interface ISportEventCache : ISdkCache, IHealthStatusProvider, IDisposable, IExportableSdkCache
    {
        /// <summary>
        /// Gets a <see cref="SportEventCacheItem"/> instance representing cached sport event data
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the id of the sport event which cached representation to return</param>
        /// <returns>a <see cref="SportEventCacheItem"/> instance representing cached sport event data</returns>
        ISportEventCacheItem GetEventCacheItem(Urn id);

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{Urn}"/> containing id's of sport events, which belong to the specified tournament
        /// </summary>
        /// <param name="tournamentId">A <see cref="Urn"/> representing the tournament identifier</param>
        /// <param name="culture">The culture to fetch the data</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        Task<IEnumerable<Tuple<Urn, Urn>>> GetEventIdsAsync(Urn tournamentId, CultureInfo culture);

        /// <summary>
        /// Asynchronous gets a <see cref="IEnumerable{Urn}"/> containing id's of sport events, which are scheduled for specified date
        /// </summary>
        /// <param name="date">The date for which to retrieve the schedule, or a null reference to get currently live events</param>
        /// <param name="culture">The culture to fetch the data</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        Task<IEnumerable<Tuple<Urn, Urn>>> GetEventIdsAsync(DateTime? date, CultureInfo culture);

        /// <summary>
        /// Asynchronously gets a list of active <see cref="IEnumerable{TournamentInfoCacheItem}"/>
        /// </summary>
        /// <remarks>Lists all <see cref="TournamentInfoCacheItem"/> that are cached (once schedule is loaded)</remarks>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        Task<IEnumerable<TournamentInfoCacheItem>> GetActiveTournamentsAsync(CultureInfo culture = null);

        /// <summary>
        /// Adds fixture timestamp to cache so that the next fixture calls for the event goes through non-cached fixture provider
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the event</param>
        void AddFixtureTimestamp(Urn id);

        /// <summary>
        /// Deletes the sport events from cache which are scheduled before specific DateTime
        /// </summary>
        /// <param name="before">The scheduled DateTime used to delete sport events from cache</param>
        /// <returns>Number of deleted items</returns>
        int DeleteSportEventsFromCache(DateTime before);

        /// <summary>
        /// Asynchronous gets a <see cref="Urn"/> of event's sport id
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the event identifier</param>
        /// <returns>A <see cref="Task{T}"/> representing an asynchronous operation</returns>
        Task<Urn> GetEventSportIdAsync(Urn id);
    }
}
