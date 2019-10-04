/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    class SoccerStatistics : ISoccerStatistics
    {
        public IEnumerable<ITeamStatistics> TotalStatistics { get; }
        public IEnumerable<IPeriodStatistics> PeriodStatistics { get; }

        // ReSharper disable InconsistentNaming
        public SoccerStatistics(IEnumerable<TeamStatisticsDTO> totalStatisticsDTOs,
            IEnumerable<PeriodStatisticsDTO> periodStatisticsDTOs)
        {
            TotalStatistics = totalStatisticsDTOs?.Select(s => new TeamStatistics(s));
            PeriodStatistics = periodStatisticsDTOs?.Select(s => new PeriodStatistics(s));
        }
    }
}
