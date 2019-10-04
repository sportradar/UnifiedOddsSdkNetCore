/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Class SportEventStatusMapperBase with method for creating base <see cref="SportEventStatusCI"/>
    /// </summary>
    internal abstract class SportEventStatusMapperBase
    {
        /// <summary>
        /// Creates a base <see cref="SportEventStatusCI"/> instance
        /// </summary>
        /// <returns>Returns base <see cref="SportEventStatusCI"/> instance</returns>
        public SportEventStatusCI CreateNotStarted()
        {
            return new SportEventStatusCI(new SportEventStatusDTO(new sportEventStatus {status = (int)EventStatus.Unknown, match_status = -1}, null), null);
        }
    }
}
