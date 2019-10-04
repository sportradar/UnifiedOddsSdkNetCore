/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    public class PeriodStatisticsDTO
    {
        public string PeriodName { get; }

        public IEnumerable<TeamStatisticsDTO> TeamStatisticsDTOs { get; }

        internal PeriodStatisticsDTO(matchPeriod period, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Contract.Requires(period != null);

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
