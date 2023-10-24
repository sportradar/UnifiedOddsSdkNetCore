/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Class StageStatus
    /// </summary>
    /// <seealso cref="CompetitionStatus" />
    /// <seealso cref="IStageStatus" />
    internal class StageStatus : CompetitionStatus, IStageStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchStatus"/> class
        /// </summary>
        /// <param name="ci">The cache item</param>
        /// <param name="matchStatusesCache">The match statuses cache</param>
        public StageStatus(SportEventStatusCacheItem ci, ILocalizedNamedValueCache matchStatusesCache)
            : base(ci, matchStatusesCache)
        {
        }

        /// <summary>
        /// Get match status as an asynchronous operation
        /// </summary>
        /// <param name="culture">The culture used to fetch status id and description</param>
        /// <returns>ILocalizedNamedValue</returns>
        public async Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture)
        {
            return SportEventStatusCacheItem == null || SportEventStatusCacheItem.MatchStatusId < 0 || MatchStatusCache == null
                       ? null
                       : await MatchStatusCache.GetAsync(SportEventStatusCacheItem.MatchStatusId, new List<CultureInfo> { culture }).ConfigureAwait(false);
        }
    }
}
