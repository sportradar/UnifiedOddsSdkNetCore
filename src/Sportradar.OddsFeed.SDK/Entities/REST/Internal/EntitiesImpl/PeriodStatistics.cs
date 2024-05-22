// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class PeriodStatistics : IPeriodStatistics
    {
        public string PeriodName { get; }
        public IEnumerable<ITeamStatistics> TeamStatistics { get; }

        public PeriodStatistics(PeriodStatisticsDto dto)
        {
            PeriodName = dto.PeriodName;
            TeamStatistics = dto.TeamStatisticsDtos?.Select(s => new TeamStatistics(s));
        }

        public PeriodStatistics(string periodName, IEnumerable<TeamStatisticsDto> teamStatisticsDtos)
        {
            PeriodName = periodName;
            TeamStatistics = teamStatisticsDtos?.Select(s => new TeamStatistics(s));
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var teamStats = string.Join(" | ", TeamStatistics);
            return $"PeriodName={PeriodName}, TeamStats=[{teamStats}]";
        }
    }
}
