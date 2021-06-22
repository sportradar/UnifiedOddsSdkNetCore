/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal.Metrics;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache <see cref="SportEventStatusCI"/> instances
    /// </summary>
    internal interface ISportEventStatusCache : ISdkCache, IHealthStatusProvider, IDisposable
    {
        /// <summary>
        /// Gets the cached <see cref="SportEventStatusCI"/> instance associated with the sport event specified by the <code>eventId</code>"/>. If the instance associated
        /// with the specified event is not found, it tries to obtain it via API, if still cant, a <see cref="SportEventStatusCI"/> instance indicating a 'not started' event is returned.
        /// </summary>
        /// <param name="eventId">A <see cref="URN"/> representing the id of the sport event whose status to get</param>
        /// <returns>A <see cref="SportEventStatusCI"/> representing the status of the specified sport event</returns>
        Task<SportEventStatusCI> GetSportEventStatusAsync(URN eventId);

        /// <summary>
        /// Adds the event identifier for timeline ignore
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="producerId">The producer identifier.</param>
        /// <param name="messageType">Type of the feed message.</param>
        /// <remarks>Used for BetPal events to have ignored timeline event status cache</remarks>
        void AddEventIdForTimelineIgnore(URN eventId, int producerId, Type messageType);
    }
}