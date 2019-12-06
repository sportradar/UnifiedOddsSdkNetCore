/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for sport event status statistics. The status can be receiver through messages or fetched from the API
    /// </summary>
    public class SportEventStatisticsDTO
    {
        public IEnumerable<TeamStatisticsDTO> TotalStatisticsDTOs { get; }

        public IEnumerable<PeriodStatisticsDTO> PeriodStatisticsDTOs { get; internal set; }

        public SportEventStatisticsDTO(statisticsType result)
        {
            Guard.Argument(result).NotNull();

            var totalStatisticsDTOs = new List<TeamStatisticsDTO>();
            totalStatisticsDTOs.Add(new TeamStatisticsDTO(
                HomeAway.Home,
                result.yellow_cards.home,
                result.red_cards.home,
                result.yellow_red_cards.home,
                result.corners.home,
                result.green_cards == null ? 0 : result.green_cards.home
            ));
            totalStatisticsDTOs.Add(new TeamStatisticsDTO(
                HomeAway.Away,
                result.yellow_cards.away,
                result.red_cards.away,
                result.yellow_red_cards.away,
                result.corners.away,
                result.green_cards == null ? 0 : result.green_cards.away
            ));
            TotalStatisticsDTOs = totalStatisticsDTOs;

            PeriodStatisticsDTOs = null;
        }

        public SportEventStatisticsDTO(matchStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(statistics).NotNull();

            var teamStats = new List<TeamStatisticsDTO>();
            if (statistics.totals != null)
            {
                foreach (var total in statistics.totals)
                {
                    teamStats.Add(new TeamStatisticsDTO(total, homeAwayCompetitors));
                }
                TotalStatisticsDTOs = teamStats;
            }

            if (statistics.periods != null)
            {
                var periodStats = new List<PeriodStatisticsDTO>();
                foreach (var period in statistics.periods)
                {
                    periodStats.Add(new PeriodStatisticsDTO(period, homeAwayCompetitors));
                }
                PeriodStatisticsDTOs = periodStats;
            }
        }
    }
}
