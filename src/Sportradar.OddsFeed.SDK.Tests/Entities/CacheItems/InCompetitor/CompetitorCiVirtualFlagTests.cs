// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorCiVirtualFlagTests : CompetitorSetup
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ConstructorWithCompetitorDtoThenVirtualIsCorrectlySet(bool? actualVirtual, bool? expectedVirtual)
    {
        var dto = GetCompetitorDtoWithVirtualFlag(actualVirtual);

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Equal(expectedVirtual, competitorCi.IsVirtual);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ConstructorWithCompetitorProfileDtoThenVirtualIsCorrectlySet(bool? actualVirtual, bool? expectedVirtual)
    {
        var dto = GetCompetitorProfileDtoWithVirtualFlag(actualVirtual);

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Equal(expectedVirtual, competitorCi.IsVirtual);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ConstructorWithSimpleTeamProfileDtoThenVirtualIsCorrectlySet(bool? actualVirtual, bool? expectedVirtual)
    {
        var dto = GetSimpleTeamProfileDtoWithVirtualFlag(actualVirtual);

        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Equal(expectedVirtual, competitorCi.IsVirtual);
    }

    [Fact]
    public void ConstructorWithPlayerCompetitorDtoThenNullIsVirtual()
    {
        var apiPlayerCompetitor = GetApiPlayerCompetitor(1);
        var dtoPlayerCompetitor = new PlayerCompetitorDto(apiPlayerCompetitor);

        var competitorCi = new CompetitorCacheItem(dtoPlayerCompetitor, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Null(competitorCi.IsVirtual);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ConstructorWithCompetitorCiThenVirtualIsCorrectlySet(bool? actualVirtual, bool? expectedVirtual)
    {
        var dto = GetCompetitorProfileDtoWithVirtualFlag(actualVirtual);
        var orgCompetitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var competitorCi = new TeamCompetitorCacheItem(orgCompetitorCi);

        Assert.Equal(expectedVirtual, competitorCi.IsVirtual);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ConstructorWithTeamCompetitorDtoThenVirtualIsCorrectlySet(bool? actualVirtual, bool? expectedVirtual)
    {
        var dto = GetTeamCompetitorProfileDtoWithVirtualFlag(actualVirtual);

        var competitorCi = new TeamCompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Equal(expectedVirtual, competitorCi.IsVirtual);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task ExportThenVirtualIsCorrectlySet(bool? actualVirtual, bool? expectedVirtual)
    {
        var dto = GetTeamCompetitorProfileDtoWithVirtualFlag(actualVirtual);
        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await competitorCi.ExportAsync();

        Assert.Equal(expectedVirtual, exported.IsVirtual);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task ImportThenVirtualIsCorrectlySet(bool? initialVirtual, bool? expectedVirtual)
    {
        var dto = GetCompetitorDtoWithVirtualFlag(initialVirtual);
        var competitorCi = new CompetitorCacheItem(dto, TestData.Culture, DataRouterManagerMock.Object);
        var exported = (ExportableCompetitor)await competitorCi.ExportAsync();

        var importedCi = new CompetitorCacheItem(exported, DataRouterManagerMock.Object);

        Assert.Equal(expectedVirtual, importedCi.IsVirtual);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(true, null, true)]
    [InlineData(false, null, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(null, false, false)]
    [InlineData(null, true, true)]
    public void MergeFromCompetitorDtoWhenOverridingValueThenCorrectIsResult(bool? firstVirtual, bool? secondVirtual, bool? expectedVirtual)
    {
        var dto1 = GetCompetitorDtoWithVirtualFlag(firstVirtual);
        var dto2 = GetCompetitorDtoWithVirtualFlag(secondVirtual);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.Equal(expectedVirtual, ci.IsVirtual);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(true, null, true)]
    [InlineData(false, null, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(null, false, false)]
    [InlineData(null, true, true)]
    public void MergeFromCompetitorProfileDtoWhenOverridingValueThenCorrectIsResult(bool? firstVirtual, bool? secondVirtual, bool? expectedVirtual)
    {
        var dto1 = GetCompetitorProfileDtoWithVirtualFlag(firstVirtual);
        var dto2 = GetCompetitorProfileDtoWithVirtualFlag(secondVirtual);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.Equal(expectedVirtual, ci.IsVirtual);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(true, null, true)]
    [InlineData(false, null, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(null, false, false)]
    [InlineData(null, true, true)]
    public void MergeFromTeamCompetitorDtoWhenOverridingValueThenCorrectIsResult(bool? firstVirtual, bool? secondVirtual, bool? expectedVirtual)
    {
        var dto1 = GetTeamCompetitorProfileDtoWithVirtualFlag(firstVirtual);
        var dto2 = GetTeamCompetitorProfileDtoWithVirtualFlag(secondVirtual);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.Equal(expectedVirtual, ci.IsVirtual);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(true, null, true)]
    [InlineData(false, null, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(null, false, false)]
    [InlineData(null, true, true)]
    public void MergeFromCompetitorCiWhenOverridingValueThenCorrectIsResult(bool? firstVirtual, bool? secondVirtual, bool? expectedVirtual)
    {
        var dto1 = GetTeamCompetitorProfileDtoWithVirtualFlag(firstVirtual);
        var dto2 = GetTeamCompetitorProfileDtoWithVirtualFlag(secondVirtual);
        var orgCompetitorCi1 = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);
        var orgCompetitorCi2 = new CompetitorCacheItem(dto2, TestData.CultureNl, DataRouterManagerMock.Object);

        orgCompetitorCi1.Merge(orgCompetitorCi2);

        Assert.Equal(expectedVirtual, orgCompetitorCi1.IsVirtual);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(true, null, true)]
    [InlineData(false, null, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(null, false, false)]
    [InlineData(null, true, true)]
    public void MergeFromSimpleTeamProfileDtoWhenOverridingValueThenCorrectIsResult(bool? firstVirtual, bool? secondVirtual, bool? expectedVirtual)
    {
        var dto1 = GetSimpleTeamProfileDtoWithVirtualFlag(firstVirtual);
        var dto2 = GetSimpleTeamProfileDtoWithVirtualFlag(secondVirtual);
        var ci = new CompetitorCacheItem(dto1, TestData.Culture, DataRouterManagerMock.Object);

        ci.Merge(dto2, TestData.CultureNl);

        Assert.Equal(expectedVirtual, ci.IsVirtual);
    }

    private CompetitorDto GetCompetitorDtoWithVirtualFlag(bool? isVirtual)
    {
        var apiTeam = GetApiTeamFull(1);
        apiTeam.@virtual = isVirtual ?? true;
        apiTeam.virtualSpecified = isVirtual.HasValue;
        return new CompetitorDto(apiTeam);
    }

    private CompetitorProfileDto GetCompetitorProfileDtoWithVirtualFlag(bool? isVirtual)
    {
        var apiCompetitorProfile = GetApiCompetitorProfileFull(1);
        apiCompetitorProfile.competitor.@virtual = isVirtual ?? true;
        apiCompetitorProfile.competitor.virtualSpecified = isVirtual.HasValue;
        return new CompetitorProfileDto(apiCompetitorProfile);
    }

    private SimpleTeamProfileDto GetSimpleTeamProfileDtoWithVirtualFlag(bool? isVirtual)
    {
        var apiSimpleTeamProfile = GetApiSimpleTeamProfileFull(1);
        apiSimpleTeamProfile.competitor.@virtual = isVirtual ?? true;
        apiSimpleTeamProfile.competitor.virtualSpecified = isVirtual.HasValue;
        return new SimpleTeamProfileDto(apiSimpleTeamProfile);
    }

    private TeamCompetitorDto GetTeamCompetitorProfileDtoWithVirtualFlag(bool? isVirtual)
    {
        var apiTeam = GetApiTeamCompetitorFull(1);
        apiTeam.@virtual = isVirtual ?? true;
        apiTeam.virtualSpecified = isVirtual.HasValue;
        return new TeamCompetitorDto(apiTeam);
    }
}
