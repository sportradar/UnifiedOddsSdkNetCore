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
    public class TeamStatisticsDTO
    {
        public URN TeamId { get; }

        public string Name { get; }

        public HomeAway? HomeOrAway { get; }

        public int Cards { get; }

        public int YellowCards { get; }

        public int RedCards { get; }

        public int YellowRedCards { get; }

        public int CornerKicks { get; }

        internal TeamStatisticsDTO(teamStatistics statistics, IDictionary<HomeAway, URN> homeAwayCompetitors)
        {
            Contract.Requires(statistics != null);

            Name = statistics.name;
            TeamId = !string.IsNullOrEmpty(statistics.id)
                ? URN.Parse(statistics.id)
                : null;

            HomeOrAway = null;
            if (TeamId != null && homeAwayCompetitors != null)
            {
                var x = homeAwayCompetitors.Where(w => w.Value.Equals(URN.Parse(statistics.id))).ToList();
                if (x.Any())
                {
                    HomeOrAway = x.First().Key == HomeAway.Home ? HomeAway.Home : HomeAway.Away;
                }
            }

            if (statistics.statistics != null)
            {
                int tmp;
                if (int.TryParse(statistics.statistics.yellow_cards, out tmp))
                {
                    YellowCards = tmp;
                }
                if (int.TryParse(statistics.statistics.red_cards, out tmp))
                {
                    RedCards = tmp;
                }
                if (int.TryParse(statistics.statistics.yellow_red_cards, out tmp))
                {
                    YellowRedCards = tmp;
                }
                if (int.TryParse(statistics.statistics.cards, out tmp))
                {
                    Cards = tmp;
                }
                if (int.TryParse(statistics.statistics.corner_kicks, out tmp))
                {
                    CornerKicks = tmp;
                }
            }
        }

        internal TeamStatisticsDTO(HomeAway? homeAway, int yellowCards, int redCards, int yellowRedCards, int cornerKicks)
        {
            Name = "";
            TeamId = null; // not available on the AMQP message
            HomeOrAway = homeAway;
            YellowCards = yellowCards;
            RedCards = redCards;
            YellowRedCards = yellowRedCards;
            Cards = yellowCards + redCards + yellowRedCards;
            CornerKicks = cornerKicks;
        }
    }
}
