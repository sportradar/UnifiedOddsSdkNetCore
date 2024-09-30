// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class SportEntityFactoryTests
{
    private readonly TestSportEntityFactoryBuilder _testSportEntityFactoryBuilder;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SportEntityFactoryTests(ITestOutputHelper outputHelper)
    {
        _testSportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(outputHelper, TestData.Cultures1);
    }

    [Fact]
    public void BuildPlayersWhenAssociatedPlayersHavePlayerIdThenReturnCompetitorPlayers()
    {
        var apiCompetitorProfile = CompetitorSetup.GetApiCompetitorProfileFull(1, 10);
        apiCompetitorProfile.competitor.players = null;

        var competitor = GetCompetitorFromApiCompetitorProfile(apiCompetitorProfile);
        var competitorPlayer = competitor.AssociatedPlayers.ToList();

        competitor.Should().NotBeNull();
        competitorPlayer.Should().HaveCount(10);
        competitorPlayer.Should().AllBeAssignableTo<CompetitorPlayer>();
    }

    [Fact]
    public void BuildPlayersWhenAssociatedPlayersHaveCompetitorIdThenReturnCompetitors()
    {
        var apiCompetitorProfile = CompetitorSetup.GetApiCompetitorProfileFull(1, 10);
        apiCompetitorProfile.players = null;
        foreach (var player in apiCompetitorProfile.competitor.players)
        {
            player.id = player.id.Replace("player", "competitor");
        }

        var competitor = GetCompetitorFromApiCompetitorProfile(apiCompetitorProfile);
        var competitorPlayer = competitor.AssociatedPlayers.ToList();

        competitor.Should().NotBeNull();
        competitorPlayer.Should().HaveCount(10);
        competitorPlayer.Should().AllBeAssignableTo<Competitor>();
    }

    private Competitor GetCompetitorFromApiCompetitorProfile(competitorProfileEndpoint apiCompetitorProfile)
    {
        var dto1 = new CompetitorProfileDto(apiCompetitorProfile);
        _testSportEntityFactoryBuilder.CacheManager.SaveDto(dto1.Competitor.Id, dto1, TestData.Culture, DtoType.CompetitorProfile, null);
        var competitorCi = new CompetitorCacheItem(dto1, TestData.Culture, _testSportEntityFactoryBuilder.DataRouterManager);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        return new Competitor(competitorCi, _testSportEntityFactoryBuilder.ProfileCache, TestData.Cultures1, _testSportEntityFactoryBuilder.SportEntityFactory, ExceptionHandlingStrategy.Catch, referenceDictionary);
    }
}
