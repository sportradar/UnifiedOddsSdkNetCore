/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache <see cref="SportEventStatusCI"/> instances
    /// </summary>
    [ContractClass(typeof(SportEventStatusCacheContract))]
    internal interface ISportEventStatusCache : ISdkCache
    {
        /// <summary>
        /// Gets the cached <see cref="SportEventStatusCI"/> instance associated with the sport event specified by the <code>eventId</code>"/>. If the instance associated
        /// with the specified event is not found, it tries to obtain it via API, if still cant, a <see cref="SportEventStatusCI"/> instance indicating a 'not started' event is returned.
        /// </summary>
        /// <param name="eventId">A <see cref="URN"/> representing the id of the sport event whose status to get</param>
        /// <returns>A <see cref="SportEventStatusCI"/> representing the status of the specified sport event</returns>
        Task<SportEventStatusCI> GetSportEventStatusAsync(URN eventId);

        ///// <summary>
        ///// Adds the sport event status to the internal cache
        ///// </summary>
        ///// <param name="eventId">The eventId of the sport event status to be cached</param>
        ///// <param name="restSportEventStatus">The sport event status to be cached</param>
        //void AddSportEventStatus(URN eventId, SportEventStatusCI restSportEventStatus);

        ///// <summary>
        ///// Remove the sport event status from the internal cache
        ///// </summary>
        ///// <param name="eventId">The eventId of the sport event status to be removed</param>
        //void RemoveSportEventStatus(URN eventId);

        ///// <summary>
        ///// Removes the sport event from <see cref="ISportEventCache"/>
        ///// </summary>
        ///// <param name="eventId">The event identifier to be removed</param>
        //void RemoveSportEventFromCache(URN eventId);
    }
}