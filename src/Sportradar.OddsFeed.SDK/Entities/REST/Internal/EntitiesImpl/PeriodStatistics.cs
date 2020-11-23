/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    class PeriodStatistics : IPeriodStatistics
    {
        public string PeriodName { get; }
        public IEnumerable<ITeamStatistics> TeamStatistics { get; }

        public PeriodStatistics(PeriodStatisticsDTO dto)
        {
            PeriodName = dto.PeriodName;
            TeamStatistics = dto.TeamStatisticsDTOs?.Select(s => new TeamStatistics(s));
        }

        // ReSharper disable once InconsistentNaming
        public PeriodStatistics(string periodName, IEnumerable<TeamStatisticsDTO> teamStatisticsDTOs)
        {
            PeriodName = periodName;
            TeamStatistics = teamStatisticsDTOs?.Select(s => new TeamStatistics(s));
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
