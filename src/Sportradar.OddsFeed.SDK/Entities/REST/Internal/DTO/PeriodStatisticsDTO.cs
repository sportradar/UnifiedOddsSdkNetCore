/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    internal class PeriodStatisticsDTO
    {
        public string PeriodName { get; }

        // ReSharper disable once InconsistentNaming
        public IEnumerable<TeamStatisticsDTO> TeamStatisticsDTOs { get; }

        internal PeriodStatisticsDTO(matchPeriod period, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(period, nameof(period)).NotNull();

            PeriodName = period.name;
            if (period.teams == null || !period.teams.Any())
            {
                return;
            }
            var teams = new List<TeamStatisticsDTO>();
            foreach (var teamStatistics in period.teams)
            {
                teams.Add(new TeamStatisticsDTO(teamStatistics, homeAwayCompetitors));
            }

            TeamStatisticsDTOs = teams;
        }
    }
}
