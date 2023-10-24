/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    internal class TeamStatisticsDto
    {
        public Urn TeamId { get; }

        public string Name { get; }

        public HomeAway? HomeOrAway { get; }

        public int? Cards { get; }

        public int? YellowCards { get; }

        public int? RedCards { get; }

        public int? YellowRedCards { get; }

        public int? CornerKicks { get; }

        public int? GreenCards { get; }

        // from feed
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed here")]
        internal TeamStatisticsDto(string name, Urn teamId, HomeAway? homeAway, int? yellowCards, int? redCards, int? yellowRedCards, int? cornerKicks, int? greenCards)
        {
            Name = name; // not available on the AMQP message
            TeamId = teamId; // not available on the AMQP message
            HomeOrAway = homeAway;
            YellowCards = yellowCards;
            RedCards = redCards;
            YellowRedCards = yellowRedCards;
            CornerKicks = cornerKicks;
            GreenCards = greenCards;
            var valueExists = false;
            var c = 0;
            if (yellowCards != null)
            {
                valueExists = true;
                c += yellowCards.Value;
            }
            if (redCards != null)
            {
                valueExists = true;
                c += redCards.Value;
            }
            if (yellowRedCards != null)
            {
                valueExists = true;
                c += yellowRedCards.Value;
            }
            if (greenCards != null)
            {
                valueExists = true;
                c += greenCards.Value;
            }
            Cards = valueExists ? c : (int?)null;
        }

        // from API
        internal TeamStatisticsDto(teamStatistics statistics, IDictionary<HomeAway, Urn> homeAwayCompetitors)
        {
            Guard.Argument(statistics, nameof(statistics)).NotNull();

            Name = statistics.name;
            TeamId = !string.IsNullOrEmpty(statistics.id)
                ? Urn.Parse(statistics.id)
                : null;

            HomeOrAway = null;
            if (TeamId != null && homeAwayCompetitors != null)
            {
                var x = homeAwayCompetitors.Where(w => w.Value.Equals(Urn.Parse(statistics.id))).ToList();
                if (x.Any())
                {
                    HomeOrAway = x.First().Key == HomeAway.Home ? HomeAway.Home : HomeAway.Away;
                }
            }

            if (statistics.statistics != null)
            {
                if (int.TryParse(statistics.statistics.yellow_cards, out var tmp))
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
    }
}
