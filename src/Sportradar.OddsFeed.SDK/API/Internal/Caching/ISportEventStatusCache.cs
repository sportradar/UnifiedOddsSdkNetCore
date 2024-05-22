// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to cache <see cref="SportEventStatusCacheItem"/> instances
    /// </summary>
    internal interface ISportEventStatusCache : ISdkCache, IHealthStatusProvider, IDisposable
    {
        /// <summary>
        /// Gets the cached <see cref="SportEventStatusCacheItem"/> instance associated with the sport event specified by the <c>eventId</c>"/>. If the instance associated
        /// with the specified event is not found, it tries to obtain it via API, if still cant, a <see cref="SportEventStatusCacheItem"/> instance indicating a 'not started' event is returned.
        /// </summary>
        /// <param name="eventId">A <see cref="Urn"/> representing the id of the sport event whose status to get</param>
        /// <returns>A <see cref="SportEventStatusCacheItem"/> representing the status of the specified sport event</returns>
        Task<SportEventStatusCacheItem> GetSportEventStatusAsync(Urn eventId);

        /// <summary>
        /// Adds the event identifier for timeline ignore
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="producerId">The producer identifier.</param>
        /// <param name="messageType">Type of the feed message.</param>
        /// <remarks>Used for BetPal events to have ignored timeline event status cache</remarks>
        void AddEventIdForTimelineIgnore(Urn eventId, int producerId, Type messageType);
    }
}
