// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    internal class PeriodStatisticsDto
    {
        public string PeriodName { get; }

        public IEnumerable<TeamStatisticsDto> TeamStatisticsDtos { get; }

        internal PeriodStatisticsDto(matchPeriod period, IDictionary<HomeAway, Urn> homeAwayCompetitors)
        {
            Guard.Argument(period, nameof(period)).NotNull();

            PeriodName = period.name;
            if (period.teams == null || !period.teams.Any())
            {
                return;
            }
            var teams = new List<TeamStatisticsDto>();
            foreach (var teamStatistics in period.teams)
            {
                teams.Add(new TeamStatisticsDto(teamStatistics, homeAwayCompetitors));
            }

            TeamStatisticsDtos = teams;
        }
    }
}
