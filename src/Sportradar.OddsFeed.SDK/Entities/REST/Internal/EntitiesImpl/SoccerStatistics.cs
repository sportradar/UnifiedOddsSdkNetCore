// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class SoccerStatistics : ISoccerStatistics
    {
        public IEnumerable<ITeamStatistics> TotalStatistics { get; }
        public IEnumerable<IPeriodStatistics> PeriodStatistics { get; }

        public SoccerStatistics(IEnumerable<TeamStatisticsDto> totalStatisticsDto, IEnumerable<PeriodStatisticsDto> periodStatisticsDto)
        {
            TotalStatistics = totalStatisticsDto?.Select(s => new TeamStatistics(s));
            PeriodStatistics = periodStatisticsDto?.Select(s => new PeriodStatistics(s));
        }
    }
}
