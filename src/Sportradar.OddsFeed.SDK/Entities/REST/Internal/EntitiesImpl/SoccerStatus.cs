// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class SoccerStatus : MatchStatus, ISoccerStatus
    {
        public new ISoccerStatistics Statistics { get; }

        public SoccerStatus(SportEventStatusCacheItem ci, ILocalizedNamedValueCache matchStatusesCache)
            : base(ci, matchStatusesCache)
        {
            if (ci?.SportEventStatistics != null)
            {
                Statistics = new SoccerStatistics(ci.SportEventStatistics.TotalStatisticsDtos,
                                                  ci.SportEventStatistics.PeriodStatisticsDtos);
            }
        }
    }
}
