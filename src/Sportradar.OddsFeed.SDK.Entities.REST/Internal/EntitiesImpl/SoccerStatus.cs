/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    public class SoccerStatus : MatchStatus, ISoccerStatus
    {
        public ISoccerStatistics Statistics { get; }

        public SoccerStatus(SportEventStatusCI ci, ILocalizedNamedValueCache matchStatusesCache)
            : base(ci, matchStatusesCache)
        {
            if (ci?.SportEventStatistics != null)
            {
                Statistics = new SoccerStatistics(ci.SportEventStatistics.TotalStatisticsDTOs,
                                                  ci.SportEventStatistics.PeriodStatisticsDTOs);
            }
        }
    }
}
