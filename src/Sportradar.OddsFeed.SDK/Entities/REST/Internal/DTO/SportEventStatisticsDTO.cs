﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
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
    internal class SportEventStatisticsDTO
    {
        public IEnumerable<TeamStatisticsDTO> TotalStatisticsDTOs { get; internal set; }

        public IEnumerable<PeriodStatisticsDTO> PeriodStatisticsDTOs { get; internal set; }

        // from feed
        public SportEventStatisticsDTO(statisticsType result)
        {
            Guard.Argument(result, nameof(result)).NotNull();

            var totalStatisticsDTOs = new List<TeamStatisticsDTO>();
            totalStatisticsDTOs.Add(new TeamStatisticsDTO(
                null,
                null,
                HomeAway.Home,
                result.yellow_cards?.home,
                result.red_cards?.home,
                result.yellow_red_cards?.home,
                result.corners?.home,
                result.green_cards?.home
            ));
            totalStatisticsDTOs.Add(new TeamStatisticsDTO(
                null,
                null,
                HomeAway.Away,
                result.yellow_cards?.away,
                result.red_cards?.away,
                result.yellow_red_cards?.away,
                result.corners?.away,
                result.green_cards?.away
            ));
            TotalStatisticsDTOs = totalStatisticsDTOs;

            PeriodStatisticsDTOs = null;
        }

        // from API
        public SportEventStatisticsDTO(matchStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Guard.Argument(statistics, nameof(statistics)).NotNull();

            var teamStats = new List<TeamStatisticsDTO>();
            if (statistics.totals != null && statistics.totals.Any())
            {
                // can here be more then 1 sub-array? 
                foreach (var total in statistics.totals)
                {
                    foreach (var teamStatistics in total)
                    {
                        teamStats.Add(new TeamStatisticsDTO(teamStatistics, homeAwayCompetitors));
                    }
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
