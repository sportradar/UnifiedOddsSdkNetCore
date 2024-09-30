// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorCiDivisionTests : CompetitorSetup
{
    [Fact]
    public void ConstructorWithCompetitorDto()
    {
        var dto = new CompetitorDto(GetApiTeamFull(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void ConstructorWithCompetitorDtoWhenNoDivision()
    {
        var apiTeam = GetApiTeamFull(1);
        apiTeam.divisionSpecified = false;
        var dto = new CompetitorDto(apiTeam);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void ConstructorWithCompetitorProfileDto()
    {
        var dto = new CompetitorProfileDto(GetApiCompetitorProfileFull(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void ConstructorWithCompetitorProfileDtoWhenNoDivision()
    {
        var apiCompetitorProfile = GetApiCompetitorProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new CompetitorProfileDto(apiCompetitorProfile);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void ConstructorWithSimpleTeamProfileDto()
    {
        var dto = new SimpleTeamProfileDto(GetApiSimpleTeamProfileFull(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void ConstructorWithSimpleTeamProfileDtoWhenNoDivision()
    {
        var apiCompetitorProfile = GetApiSimpleTeamProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new SimpleTeamProfileDto(apiCompetitorProfile);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void ConstructorWithPlayerCompetitorDto()
    {
        var dto = new PlayerCompetitorDto(GetApiPlayerCompetitor(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void TeamCompetitorConstructorWhenWithTeamCompetitorDto()
    {
        var dto = new TeamCompetitorDto(GetApiTeamCompetitorFull(1));

        var ci = new TeamCompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void TeamCompetitorConstructorWhenWithTeamCompetitorDtoThenNoDivision()
    {
        var apiTeamCompetitorProfile = GetApiTeamCompetitorFull(1);
        apiTeamCompetitorProfile.divisionSpecified = false;
        var dto = new TeamCompetitorDto(apiTeamCompetitorProfile);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void TeamCompetitorConstructorWhenWithTeamCompetitorCacheItem()
    {
        var dto = new TeamCompetitorDto(GetApiTeamCompetitorFull(1));
        var baseTeamCi = new TeamCompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var ci = new TeamCompetitorCacheItem(baseTeamCi);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void TeamCompetitorConstructorWhenWithTeamCompetitorCacheItemThenNoDivision()
    {
        var apiTeamCompetitorProfile = GetApiTeamCompetitorFull(1);
        apiTeamCompetitorProfile.divisionSpecified = false;
        var dto = new TeamCompetitorDto(apiTeamCompetitorProfile);
        var baseTeamCi = new TeamCompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var ci = new TeamCompetitorCacheItem(baseTeamCi);

        Assert.Null(ci.Division);
    }

    [Fact]
    public async Task ExportWhenDivisionExist()
    {
        var dto = new CompetitorDto(GetApiTeamFull(1));
        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await ci.ExportAsync();

        Assert.NotNull(exported.Division);
        Assert.Equal(dto.Division.Id, exported.Division.Id);
        Assert.Equal(dto.Division.Name, exported.Division.Name);
    }

    [Fact]
    public async Task ExportWhenDivisionDoesNotExist()
    {
        var apiCompetitorProfile = GetApiSimpleTeamProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new SimpleTeamProfileDto(apiCompetitorProfile);
        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await ci.ExportAsync();

        Assert.Null(exported.Division);
    }

    [Fact]
    public async Task ImportWhenDivisionExist()
    {
        var dto = new CompetitorDto(GetApiTeamFull(1));
        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);
        var exported = (ExportableCompetitor)await ci.ExportAsync();

        var imported = new CompetitorCacheItem(exported, DataRouterManagerMock.Object);

        Assert.NotNull(imported.Division);
        Assert.Equal(ci.Division.Id, imported.Division.Id);
        Assert.Equal(ci.Division.Name, imported.Division.Name);
    }

    [Fact]
    public async Task ImportWhenDivisionDoesNotExist()
    {
        var apiCompetitorProfile = GetApiSimpleTeamProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new SimpleTeamProfileDto(apiCompetitorProfile);
        var ci = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);
        var exported = (ExportableCompetitor)await ci.ExportAsync();

        var imported = new CompetitorCacheItem(exported, DataRouterManagerMock.Object);

        Assert.Null(imported.Division);
    }

    [Fact]
    public void MergeDivisionWithNewDivisionWhenFromCompetitorDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivisionWhenFromCompetitorDto()
    {
        var dto1 = GetCompetitorWithoutDivision();
        var dto2 = GetCompetitorWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivisionWhenFromCompetitorDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorWithoutDivision();
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivisionWhenFromCompetitorProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivisionWhenFromCompetitorProfileDto()
    {
        var dto1 = GetCompetitorWithoutDivision();
        var dto2 = GetCompetitorProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivisionWhenFromCompetitorProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorProfileWithoutDivision();
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivisionWhenFromSimpleTeamProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetSimpleTeamProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivisionWhenFromSimpleTeamProfileDto()
    {
        var dto1 = GetCompetitorWithoutDivision();
        var dto2 = GetSimpleTeamProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivisionWhenFromSimpleTeamProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetSimpleTeamProfileWithoutDivision();
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivisionWhenFromTeamCompetitorDto()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithDivision(222);
        var ci = new TeamCompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivisionWhenFromTeamCompetitorDto()
    {
        var dto1 = GetTeamCompetitorWithDivision();
        var dto2 = GetTeamCompetitorWithDivision(222);
        var ci = new TeamCompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivisionWhenFromTeamCompetitorDto()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithoutDivision();
        var ci = new TeamCompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivisionWhenFromTeamCompetitorCacheItem()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithDivision(222);
        var baseCi = new TeamCompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);
        var newCi = new TeamCompetitorCacheItem(dto2, TestData.CultureNl, DataRouterManagerMock.Object);

        baseCi.Merge(newCi);

        Assert.NotNull(baseCi.Division);
        Assert.Equal(dto2.Division.Id, baseCi.Division.Id);
        Assert.Equal(dto2.Division.Name, baseCi.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivisionWhenFromTeamCompetitorCacheItem()
    {
        var dto1 = GetTeamCompetitorWithDivision();
        var dto2 = GetTeamCompetitorWithDivision(222);
        var baseCi = new TeamCompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);
        var newCi = new TeamCompetitorCacheItem(dto2, TestData.CultureNl, DataRouterManagerMock.Object);

        baseCi.Merge(newCi);

        Assert.NotNull(baseCi.Division);
        Assert.Equal(dto2.Division.Id, baseCi.Division.Id);
        Assert.Equal(dto2.Division.Name, baseCi.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivisionWhenFromTeamCompetitorCacheItem()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithoutDivision();
        var baseCi = new TeamCompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);
        var newCi = new TeamCompetitorCacheItem(dto2, TestData.CultureNl, DataRouterManagerMock.Object);

        baseCi.Merge(newCi);

        Assert.NotNull(baseCi.Division);
        Assert.Equal(dto1.Division.Id, baseCi.Division.Id);
        Assert.Equal(dto1.Division.Name, baseCi.Division.Name);
    }
}
