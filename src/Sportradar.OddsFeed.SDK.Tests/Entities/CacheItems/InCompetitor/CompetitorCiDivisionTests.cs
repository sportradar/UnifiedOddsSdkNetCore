using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorCiDivisionTests : CompetitorHelper
{
    private readonly Mock<IDataRouterManager> _dataRouterManagerMock = new Mock<IDataRouterManager>();

    [Fact]
    public void ConstructorWithCompetitorDto()
    {
        var dto = new CompetitorDto(GetApiTeamFull(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void ConstructorWithCompetitorDto_NoDivision()
    {
        var apiTeam = GetApiTeamFull(1);
        apiTeam.divisionSpecified = false;
        var dto = new CompetitorDto(apiTeam);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void ConstructorWithCompetitorProfileDto()
    {
        var dto = new CompetitorProfileDto(GetApiCompetitorProfileFull(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void ConstructorWithCompetitorProfileDto_NoDivision()
    {
        var apiCompetitorProfile = GetApiCompetitorProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new CompetitorProfileDto(apiCompetitorProfile);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void ConstructorWithSimpleTeamProfileDto()
    {
        var dto = new SimpleTeamProfileDto(GetApiSimpleTeamProfileFull(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void ConstructorWithSimpleTeamProfileDto_NoDivision()
    {
        var apiCompetitorProfile = GetApiSimpleTeamProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new SimpleTeamProfileDto(apiCompetitorProfile);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void ConstructorWithPlayerCompetitorDto()
    {
        var dto = new PlayerCompetitorDto(GetApiPlayerCompetitor(1));

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void TeamCompetitorConstructor_WithTeamCompetitorDto()
    {
        var dto = new TeamCompetitorDto(GetApiTeamCompetitorFull(1));

        var ci = new TeamCompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void TeamCompetitorConstructor_WithTeamCompetitorDto_NoDivision()
    {
        var apiTeamCompetitorProfile = GetApiTeamCompetitorFull(1);
        apiTeamCompetitorProfile.divisionSpecified = false;
        var dto = new TeamCompetitorDto(apiTeamCompetitorProfile);

        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        Assert.Null(ci.Division);
    }

    [Fact]
    public void TeamCompetitorConstructor_WithTeamCompetitorCacheItem()
    {
        var dto = new TeamCompetitorDto(GetApiTeamCompetitorFull(1));
        var baseTeamCi = new TeamCompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        var ci = new TeamCompetitorCacheItem(baseTeamCi);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto.Division.Id, ci.Division.Id);
        Assert.Equal(dto.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void TeamCompetitorConstructor_WithTeamCompetitorCacheItem_NoDivision()
    {
        var apiTeamCompetitorProfile = GetApiTeamCompetitorFull(1);
        apiTeamCompetitorProfile.divisionSpecified = false;
        var dto = new TeamCompetitorDto(apiTeamCompetitorProfile);
        var baseTeamCi = new TeamCompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        var ci = new TeamCompetitorCacheItem(baseTeamCi);

        Assert.Null(ci.Division);
    }

    [Fact]
    public async Task Export_DivisionExist()
    {
        var dto = new CompetitorDto(GetApiTeamFull(1));
        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await ci.ExportAsync();

        Assert.NotNull(exported.Division);
        Assert.Equal(dto.Division.Id, exported.Division.Id);
        Assert.Equal(dto.Division.Name, exported.Division.Name);
    }

    [Fact]
    public async Task Export_DivisionDoesNotExist()
    {
        var apiCompetitorProfile = GetApiSimpleTeamProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new SimpleTeamProfileDto(apiCompetitorProfile);
        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await ci.ExportAsync();

        Assert.Null(exported.Division);
    }

    [Fact]
    public async Task Import_DivisionExist()
    {
        var dto = new CompetitorDto(GetApiTeamFull(1));
        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);
        var exported = (ExportableCompetitor)await ci.ExportAsync();

        var imported = new CompetitorCacheItem(exported, _dataRouterManagerMock.Object);

        Assert.NotNull(imported.Division);
        Assert.Equal(ci.Division.Id, imported.Division.Id);
        Assert.Equal(ci.Division.Name, imported.Division.Name);
    }

    [Fact]
    public async Task Import_DivisionDoesNotExist()
    {
        var apiCompetitorProfile = GetApiSimpleTeamProfileFull(1);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        var dto = new SimpleTeamProfileDto(apiCompetitorProfile);
        var ci = new CompetitorCacheItem(dto, TestData.Culture, _dataRouterManagerMock.Object);
        var exported = (ExportableCompetitor)await ci.ExportAsync();

        var imported = new CompetitorCacheItem(exported, _dataRouterManagerMock.Object);

        Assert.Null(imported.Division);
    }

    [Fact]
    public void MergeDivisionWithNewDivision_FromCompetitorDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivision_FromCompetitorDto()
    {
        var dto1 = GetCompetitorWithoutDivision();
        var dto2 = GetCompetitorWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivision_FromCompetitorDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorWithoutDivision();
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivision_FromCompetitorProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivision_FromCompetitorProfileDto()
    {
        var dto1 = GetCompetitorWithoutDivision();
        var dto2 = GetCompetitorProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivision_FromCompetitorProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetCompetitorProfileWithoutDivision();
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivision_FromSimpleTeamProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetSimpleTeamProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivision_FromSimpleTeamProfileDto()
    {
        var dto1 = GetCompetitorWithoutDivision();
        var dto2 = GetSimpleTeamProfileWithDivision(222);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Competitor.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Competitor.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivision_FromSimpleTeamProfileDto()
    {
        var dto1 = GetCompetitorWithDivision(111);
        var dto2 = GetSimpleTeamProfileWithoutDivision();
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivision_FromTeamCompetitorDto()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithDivision(222);
        var ci = new TeamCompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivision_FromTeamCompetitorDto()
    {
        var dto1 = GetTeamCompetitorWithDivision();
        var dto2 = GetTeamCompetitorWithDivision(222);
        var ci = new TeamCompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto2.Division.Id, ci.Division.Id);
        Assert.Equal(dto2.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivision_FromTeamCompetitorDto()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithoutDivision();
        var ci = new TeamCompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.NotNull(ci.Division);
        Assert.Equal(dto1.Division.Id, ci.Division.Id);
        Assert.Equal(dto1.Division.Name, ci.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNewDivision_FromTeamCompetitorCacheItem()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithDivision(222);
        var baseCi = new TeamCompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);
        var newCi = new TeamCompetitorCacheItem(dto2, TestData.CultureNl, _dataRouterManagerMock.Object);

        baseCi.Merge(newCi);

        Assert.NotNull(baseCi.Division);
        Assert.Equal(dto2.Division.Id, baseCi.Division.Id);
        Assert.Equal(dto2.Division.Name, baseCi.Division.Name);
    }

    [Fact]
    public void MergeNoDivisionWithNewDivision_FromTeamCompetitorCacheItem()
    {
        var dto1 = GetTeamCompetitorWithDivision();
        var dto2 = GetTeamCompetitorWithDivision(222);
        var baseCi = new TeamCompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);
        var newCi = new TeamCompetitorCacheItem(dto2, TestData.CultureNl, _dataRouterManagerMock.Object);

        baseCi.Merge(newCi);

        Assert.NotNull(baseCi.Division);
        Assert.Equal(dto2.Division.Id, baseCi.Division.Id);
        Assert.Equal(dto2.Division.Name, baseCi.Division.Name);
    }

    [Fact]
    public void MergeDivisionWithNoDivision_FromTeamCompetitorCacheItem()
    {
        var dto1 = GetTeamCompetitorWithDivision(111);
        var dto2 = GetTeamCompetitorWithoutDivision();
        var baseCi = new TeamCompetitorCacheItem(dto1, TestData.Culture, _dataRouterManagerMock.Object);
        var newCi = new TeamCompetitorCacheItem(dto2, TestData.CultureNl, _dataRouterManagerMock.Object);

        baseCi.Merge(newCi);

        Assert.NotNull(baseCi.Division);
        Assert.Equal(dto1.Division.Id, baseCi.Division.Id);
        Assert.Equal(dto1.Division.Name, baseCi.Division.Name);
    }
}
