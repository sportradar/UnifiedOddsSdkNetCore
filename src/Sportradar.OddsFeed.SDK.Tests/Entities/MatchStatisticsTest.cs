// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class MatchStatisticsTests
{
    [Fact]
    public void ConstructorShouldInitializeTotalStatistics()
    {
        // Arrange
        var totalStatisticsDto = new List<TeamStatisticsDto> { new("Team A", Urn.Parse("sr:team:1"), HomeAway.Home, 1, 0, 7, 3, 4), new("Team B", Urn.Parse("sr:team:2"), HomeAway.Away, 2, 1, 8, 5, 6) };

        // Act
        var matchStatistics = new MatchStatistics(totalStatisticsDto, null);

        // Assert
        matchStatistics.TotalStatistics.Should().NotBeNull();
        matchStatistics.TotalStatistics.Should().HaveCount(2);
        var firstTeam = matchStatistics.TotalStatistics.First();
        firstTeam.Name.Should().Be("Team A");
        firstTeam.TeamId.Should().Be(Urn.Parse("sr:team:1"));
        firstTeam.HomeAway.Should().Be(HomeAway.Home);
        firstTeam.Cards.Should().Be(12);
        firstTeam.YellowCards.Should().Be(1);
        firstTeam.RedCards.Should().Be(0);
        firstTeam.YellowRedCards.Should().Be(7);
        firstTeam.CornerKicks.Should().Be(3);
        firstTeam.GreenCards.Should().Be(4);
        var secondTeam = matchStatistics.TotalStatistics.Last();
        secondTeam.Name.Should().Be("Team B");
        secondTeam.TeamId.Should().Be(Urn.Parse("sr:team:2"));
        secondTeam.HomeAway.Should().Be(HomeAway.Away);
        secondTeam.Cards.Should().Be(17);
        secondTeam.YellowCards.Should().Be(2);
        secondTeam.RedCards.Should().Be(1);
        secondTeam.YellowRedCards.Should().Be(8);
        secondTeam.CornerKicks.Should().Be(5);
        secondTeam.GreenCards.Should().Be(6);
    }

    [Fact]
    public void ConstructorShouldInitializePeriodStatistics()
    {
        // Arrange
        var homeAwayCompetitors = new Dictionary<HomeAway, Urn> { { HomeAway.Home, Urn.Parse("sr:team:1") }, { HomeAway.Away, Urn.Parse("sr:team:2") } };
        var period = new matchPeriod
        {
            name = "First Half",
            teams = new teamStatistics[]
                                         {
                                             new() { id = "sr:team:1", name = "Team A", statistics = new teamStatisticsStatistics { yellow_cards = "2", red_cards = "1", yellow_red_cards = "0", corner_kicks = "4" } },
                                             new() { id = "sr:team:2", name = "Team B", statistics = new teamStatisticsStatistics { yellow_cards = "3", red_cards = "2", yellow_red_cards = "1", corner_kicks = "6" } }
                                         }
        };
        var periodStatisticsDto = new List<PeriodStatisticsDto> { new(period, homeAwayCompetitors) };

        // Act
        var matchStatistics = new MatchStatistics(null, periodStatisticsDto);

        // Assert
        matchStatistics.PeriodStatistics.Should().NotBeNull();
        matchStatistics.PeriodStatistics.Should().HaveCount(1);

        var firstPeriod = matchStatistics.PeriodStatistics.First();
        firstPeriod.PeriodName.Should().Be("First Half");
        firstPeriod.TeamStatistics.Should().HaveCount(2);
        var firstTeamStatistics = firstPeriod.TeamStatistics.First();
        firstTeamStatistics.Name.Should().Be("Team A");
        firstTeamStatistics.TeamId.Should().Be(Urn.Parse("sr:team:1"));
        firstTeamStatistics.HomeAway.Should().Be(HomeAway.Home);
        firstTeamStatistics.YellowCards.Should().Be(2);
        firstTeamStatistics.RedCards.Should().Be(1);
        firstTeamStatistics.YellowRedCards.Should().Be(0);
        firstTeamStatistics.CornerKicks.Should().Be(4);
        var secondTeamStatistics = firstPeriod.TeamStatistics.Last();
        secondTeamStatistics.Name.Should().Be("Team B");
        secondTeamStatistics.TeamId.Should().Be(Urn.Parse("sr:team:2"));
        secondTeamStatistics.HomeAway.Should().Be(HomeAway.Away);
        secondTeamStatistics.YellowCards.Should().Be(3);
        secondTeamStatistics.RedCards.Should().Be(2);
        secondTeamStatistics.YellowRedCards.Should().Be(1);
        secondTeamStatistics.CornerKicks.Should().Be(6);
    }

    [Fact]
    public void ConstructorShouldHandleNullInputs()
    {
        // Act
        var matchStatistics = new MatchStatistics(null, null);

        // Assert
        matchStatistics.TotalStatistics.Should().BeNull();
        matchStatistics.PeriodStatistics.Should().BeNull();
    }

    [Fact]
    public void ConstructorShouldHandleEmptyInputs()
    {
        // Arrange
        var totalStatisticsDto = new List<TeamStatisticsDto>();
        var periodStatisticsDto = new List<PeriodStatisticsDto>();

        // Act
        var matchStatistics = new MatchStatistics(totalStatisticsDto, periodStatisticsDto);

        // Assert
        matchStatistics.TotalStatistics.Should().BeEmpty();
        matchStatistics.PeriodStatistics.Should().BeEmpty();
    }
}
