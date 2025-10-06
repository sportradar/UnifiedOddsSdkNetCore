// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    class TeamStatistics : ITeamStatistics
    {
        public Urn TeamId { get; }
        public string Name { get; }
        public HomeAway? HomeAway { get; }
        public int? Cards { get; }
        public int? YellowCards { get; }
        public int? RedCards { get; }
        public int? YellowRedCards { get; }
        public int? CornerKicks { get; }
        public int? GreenCards { get; }

        public TeamStatistics(TeamStatisticsDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

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

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"TeamId={TeamId}, Name={Name}, HomeAway={HomeAway}, Cards={Cards}, YellowCards={YellowCards}, RedCards={RedCards}, YellowRedCards={YellowRedCards}, GreenCards={GreenCards}, CornerKicks={CornerKicks}";
        }
    }
}
