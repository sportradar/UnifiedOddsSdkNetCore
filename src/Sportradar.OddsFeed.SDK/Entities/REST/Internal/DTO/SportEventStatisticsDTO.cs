/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for sport event status statistics. The status can be receiver through messages or fetched from the API
    /// </summary>
    internal class SportEventStatisticsDto
    {
        public IEnumerable<TeamStatisticsDto> TotalStatisticsDtos { get; internal set; }

        public IEnumerable<PeriodStatisticsDto> PeriodStatisticsDtos { get; internal set; }

        // from feed
        public SportEventStatisticsDto(statisticsType result)
        {
            Guard.Argument(result, nameof(result)).NotNull();

            TotalStatisticsDtos = new List<TeamStatisticsDto>
                                  {
                                      new TeamStatisticsDto(
                                                            null,
                                                            null,
                                                            HomeAway.Home,
                                                            result.yellow_cards?.home,
                                                            result.red_cards?.home,
                                                            result.yellow_red_cards?.home,
                                                            result.corners?.home,
                                                            result.green_cards?.home
                                                           ),
                                      new TeamStatisticsDto(
                                                            null,
                                                            null,
                                                            HomeAway.Away,
                                                            result.yellow_cards?.away,
                                                            result.red_cards?.away,
                                                            result.yellow_red_cards?.away,
                                                            result.corners?.away,
                                                            result.green_cards?.away
                                                           )
                                  };

            PeriodStatisticsDtos = null;
        }

        // from API
        public SportEventStatisticsDto(matchStatistics statistics, IDictionary<HomeAway, Urn> homeAwayCompetitors)
        {
            Guard.Argument(statistics, nameof(statistics)).NotNull();

            var teamStats = new List<TeamStatisticsDto>();
            if (statistics.totals?.Any() == true)
            {
                // can here be more then 1 sub-array? 
                foreach (var total in statistics.totals)
                {
                    foreach (var teamStatistics in total)
                    {
                        teamStats.Add(new TeamStatisticsDto(teamStatistics, homeAwayCompetitors));
                    }
                }

                TotalStatisticsDtos = teamStats;
            }

            if (statistics.periods != null)
            {
                var periodStats = new List<PeriodStatisticsDto>();
                foreach (var period in statistics.periods)
                {
                    periodStats.Add(new PeriodStatisticsDto(period, homeAwayCompetitors));
                }
                PeriodStatisticsDtos = periodStats;
            }
        }
    }
}
