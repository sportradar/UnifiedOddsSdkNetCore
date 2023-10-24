/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Class SportEventStatusMapperBase with method for creating base <see cref="SportEventStatusCacheItem"/>
    /// </summary>
    internal abstract class SportEventStatusMapperBase
    {
        /// <summary>
        /// Creates a base <see cref="SportEventStatusCacheItem"/> instance
        /// </summary>
        /// <returns>Returns base <see cref="SportEventStatusCacheItem"/> instance</returns>
        public SportEventStatusCacheItem CreateNotStarted()
        {
            return new SportEventStatusCacheItem(new SportEventStatusDto(new sportEventStatus { status = (int)EventStatus.Unknown, match_status = -1 }, null), null);
        }
    }
}
