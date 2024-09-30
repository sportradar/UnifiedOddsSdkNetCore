// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorCiJerseyNumberTests : CompetitorSetup
{
    [Fact]
    public void ConstructorWithCompetitorDtoThenNoPlayerHasJerseyNumber()
    {
        var dto = GetCompetitorDtoWhichCanNotHavePlayerJerseyNumber();

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(competitorCi.AssociatedPlayerIds);
        Assert.Empty(competitorCi.GetAssociatedPlayersJerseyNumber());
    }

    [Fact]
    public void ConstructorWithCompetitorProfileDtoWhenNoJerseyNumberThenNoJerseyNumberSaved()
    {
        var dto = GetCompetitorProfileDtoWithPlayerJerseyNumber(false);

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(competitorCi.AssociatedPlayerIds);
        Assert.NotEmpty(competitorCi.AssociatedPlayerIds);
        Assert.Empty(competitorCi.GetAssociatedPlayersJerseyNumber());
    }

    [Fact]
    public void ConstructorWithCompetitorProfileDtoWhenJerseyNumberDefinedThenSaved()
    {
        var dto = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(competitorCi.AssociatedPlayerIds);
        Assert.NotEmpty(competitorCi.AssociatedPlayerIds);
        Assert.NotEmpty(competitorCi.GetAssociatedPlayersJerseyNumber());
        foreach (var playerId in competitorCi.GetAssociatedPlayerIds())
        {
            var playerJerseyNumberPair = competitorCi.GetAssociatedPlayersJerseyNumber().FirstOrDefault(f => Equals(f.Key, playerId));
            Assert.True(playerJerseyNumberPair.Value > 0);
        }
    }

    [Fact]
    public void ConstructorWithCompetitorDtoWhenNoPlayersThenNoJerseyNumbers()
    {
        var dto = new CompetitorDto(GetApiTeamFull(1, 0));

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(competitorCi.AssociatedPlayerIds);
        Assert.Empty(competitorCi.AssociatedPlayerIds);
        Assert.Empty(competitorCi.GetAssociatedPlayersJerseyNumber());
    }

    [Fact]
    public void ConstructorWithCompetitorProfileDtoWhenNoPlayersThenNoJerseyNumbers()
    {
        var dto = new CompetitorProfileDto(GetApiCompetitorProfileFull(1, 0));

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(competitorCi.AssociatedPlayerIds);
        Assert.Empty(competitorCi.AssociatedPlayerIds);
        Assert.Empty(competitorCi.GetAssociatedPlayersJerseyNumber());
    }

    [Fact]
    public void ConstructorWithCompetitorCiWhenJerseyNumberDefinedThenSaved()
    {
        var dto = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var orgCompetitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);
        var competitorCi = new TeamCompetitorCacheItem(orgCompetitorCi);

        Assert.NotEmpty(competitorCi.GetAssociatedPlayersJerseyNumber());
        foreach (var playerId in competitorCi.GetAssociatedPlayerIds())
        {
            var playerJerseyNumberPair = competitorCi.GetAssociatedPlayersJerseyNumber().FirstOrDefault(f => Equals(f.Key, playerId));
            Assert.True(playerJerseyNumberPair.Value > 0);
        }
    }

    [Fact]
    public async Task ExportWithCompetitorDtoThenNoPlayerHasJerseyNumber()
    {
        var dto = GetCompetitorDtoWhichCanNotHavePlayerJerseyNumber();
        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await competitorCi.ExportAsync();

        Assert.NotNull(exported.AssociatedPlayerIds);
        Assert.Empty(exported.AssociatedPlayersJerseyNumbers);
    }

    [Fact]
    public async Task ExportWithPlayerJerseyNumbersThenJerseyNumberAreExported()
    {
        var dto = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await competitorCi.ExportAsync();

        Assert.NotNull(exported.AssociatedPlayerIds);
        Assert.NotEmpty(exported.AssociatedPlayersJerseyNumbers);
        foreach (var playerId in exported.AssociatedPlayerIds)
        {
            var playerJerseyNumberPair = exported.AssociatedPlayersJerseyNumbers.FirstOrDefault(f => Equals(f.Key, playerId));
            Assert.True(playerJerseyNumberPair.Value > 0);
        }
    }

    [Fact]
    public async Task ImportThenVirtualIsCorrectlySet()
    {
        var dto = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);
        var exported = (ExportableCompetitor)await competitorCi.ExportAsync();

        var importedCi = new CompetitorCacheItem(exported, DataRouterManagerMock.Object);

        Assert.NotNull(importedCi.AssociatedPlayerIds);
        Assert.NotEmpty(importedCi.AssociatedPlayersJerseyNumbers);
        foreach (var playerId in importedCi.AssociatedPlayerIds)
        {
            var playerJerseyNumberPair = importedCi.AssociatedPlayersJerseyNumbers.FirstOrDefault(f => Equals(f.Key, playerId));
            Assert.True(playerJerseyNumberPair.Value > 0);
        }
    }

    [Fact]
    public void MergeFromCompetitorProfileDtoWhenOverridingWithNewValueThenOnlyNewResultAreSaved()
    {
        var dto1 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var dto2 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.AssociatedPlayerIds);
        Assert.NotEmpty(ci.AssociatedPlayersJerseyNumbers);
        Assert.Equal(ci.AssociatedPlayerIds.Count, ci.AssociatedPlayersJerseyNumbers.Count);
    }

    [Fact]
    public void MergeFromCompetitorProfileDtoWhenOverridingWithNewValueThenCorrectIsResult()
    {
        var dto1 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var dto2 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.AssociatedPlayerIds);
        Assert.NotEmpty(ci.AssociatedPlayersJerseyNumbers);
        foreach (var playerId in ci.AssociatedPlayerIds)
        {
            Assert.Contains(playerId, dto2.Players.Select(s => s.Id));
            var playerJerseyNumberPair = ci.AssociatedPlayersJerseyNumbers.FirstOrDefault(f => Equals(f.Key, playerId));
            Assert.True(playerJerseyNumberPair.Value > 0);
        }
    }

    [Fact]
    public void MergeFromCompetitorProfileDtoWhenOverridingWithNoValueThenOldResultArePreserved()
    {
        var dto1 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var dto2 = GetCompetitorProfileDtoWithPlayerJerseyNumber(false);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.AssociatedPlayerIds);
        Assert.NotEmpty(ci.AssociatedPlayersJerseyNumbers);
        Assert.Equal(ci.AssociatedPlayerIds.Count, ci.AssociatedPlayersJerseyNumbers.Count);
    }

    [Fact]
    public void MergeFromCompetitorProfileDtoWhenOverridingWithNoValueThenJerseyNumbersArePreserved()
    {
        var dto1 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var dto2 = GetCompetitorProfileDtoWithPlayerJerseyNumber(false);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.AssociatedPlayerIds);
        Assert.NotEmpty(ci.AssociatedPlayersJerseyNumbers);
        foreach (var playerId in ci.AssociatedPlayerIds)
        {
            var playerJerseyNumberPair = ci.AssociatedPlayersJerseyNumbers.FirstOrDefault(f => Equals(f.Key, playerId));
            Assert.True(playerJerseyNumberPair.Value > 0);
        }
    }

    private static CompetitorDto GetCompetitorDtoWhichCanNotHavePlayerJerseyNumber()
    {
        var apiTeam = GetApiTeamFull(1, 10);
        return new CompetitorDto(apiTeam);
    }
}
