/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    class TeamStatistics : ITeamStatistics
    {
        public URN TeamId { get; }
        public string Name { get; }
        public HomeAway? HomeAway { get; }
        public int? Cards { get; }
        public int? YellowCards { get; }
        public int? RedCards { get; }
        public int? YellowRedCards { get; }
        public int? CornerKicks { get; }
        /// <summary>
        /// Gets the total count of green cards
        /// </summary>
        /// <value>The total count of green cards</value>
        public int? GreenCards { get; }

        public TeamStatistics(TeamStatisticsDTO dto)
        {
            Guard.Argument(dto, nameof()).NotNull();

            TeamId = dto.TeamId;
            Name = dto.Name;
            HomeAway = dto.HomeOrAway;
            Cards = dto.Cards;
            YellowCards = dto.YellowCards;
            RedCards = dto.RedCards;
            YellowRedCards = dto.YellowRedCards;
            CornerKicks = dto.CornerKicks;
            GreenCards = dto.GreenCards;
        }
    }
}
