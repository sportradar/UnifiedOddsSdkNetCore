// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorCiTests : CompetitorSetup
{
    [Fact]
    public void ConstructorWithCompetitorDto()
    {
        var competitorDto = new CompetitorDto(GetApiTeamFull(1));

        var competitorCi = new CompetitorCacheItem(competitorDto, TestData.Culture, DataRouterManagerMock.Object);

        ValidateCompetitorDtoWithCi(competitorDto, competitorCi, TestData.Culture);
        Assert.Equal("CompetitorDto", competitorCi.RootSource);
    }

    [Fact]
    public void ConstructorWithCompetitorDtoWhenNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new CompetitorCacheItem((CompetitorDto)null, TestData.Culture, DataRouterManagerMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetitorDtoWhenNullCultureThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new CompetitorCacheItem(new CompetitorDto(GetApiTeamFull(1)), null, DataRouterManagerMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetitorDtoWhenNullDataRouterManager()
    {
        var competitorDto = new CompetitorDto(GetApiTeamFull(1));

        var competitorCi = new CompetitorCacheItem(competitorDto, TestData.Culture, null);

        ValidateCompetitorDtoWithCi(competitorDto, competitorCi, TestData.Culture);
    }

    [Fact]
    public void ConstructorWithCompetitorDtoWhenNullExportableThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new CompetitorCacheItem(null, DataRouterManagerMock.Object));
    }

    [Fact]
    public async Task WithCompetitorDtoWhenExport()
    {
        var competitorCi = new CompetitorCacheItem(new CompetitorDto(GetApiTeamFull(1)), TestData.Culture, DataRouterManagerMock.Object);

        var exported = (ExportableCompetitor)await competitorCi.ExportAsync();

        Assert.NotNull(exported);
        Assert.Equal(competitorCi.Id.ToString(), exported.Id);
        Assert.Equal(competitorCi.AgeGroup, exported.AgeGroup);
        //Assert.Equal(competitorCi.CategoryId.ToString(), exported.CategoryId);
        Assert.Equal(competitorCi.CountryCode, exported.CountryCode);
        Assert.Equal(competitorCi.Gender, exported.Gender);
        Assert.Null(competitorCi.IsVirtual);
        Assert.False(exported.IsVirtual);
        Assert.Equal(competitorCi.ShortName, exported.ShortName);
        //Assert.Equal(competitorCi.SportId.ToString(), exported.SportId);
        Assert.Equal(competitorCi.State, exported.State);
        if (competitorCi.Division != null)
        {
            Assert.Equal(competitorCi.Division.Id, exported.Division.Id);
            Assert.Equal(competitorCi.Division.Name, exported.Division.Name);
        }
        foreach (var associatedPlayerId in competitorCi.AssociatedPlayerIds)
        {
            Assert.Contains(associatedPlayerId.ToString(), exported.AssociatedPlayerIds);
        }
        Assert.Equal(competitorCi.ReferenceId.ReferenceIds.Count, exported.ReferenceIds.Count);
    }

    [Fact]
    public async Task WithCompetitorDtoWhenImport()
    {
        var competitorCi = new CompetitorCacheItem(new CompetitorDto(GetApiTeamFull(1)), TestData.Culture, DataRouterManagerMock.Object);
        var exported = (ExportableCompetitor)await competitorCi.ExportAsync();

        var imported = new CompetitorCacheItem(exported, DataRouterManagerMock.Object);

        Assert.NotNull(imported);
        Assert.Equal(competitorCi.Id, imported.Id);
        Assert.Equal(competitorCi.AgeGroup, imported.AgeGroup);
        Assert.Equal(competitorCi.CategoryId, imported.CategoryId);
        Assert.Equal(competitorCi.CountryCode, imported.CountryCode);
        Assert.Equal(competitorCi.Gender, imported.Gender);
        Assert.Null(competitorCi.IsVirtual);
        Assert.False(imported.IsVirtual);
        Assert.Equal(competitorCi.ShortName, imported.ShortName);
        Assert.Equal(competitorCi.SportId, imported.SportId);
        Assert.Equal(competitorCi.State, imported.State);
        Assert.Equal(competitorCi.Division.Id, imported.Division.Id);
        Assert.Equal(competitorCi.Division.Name, imported.Division.Name);
        foreach (var associatedPlayerId in imported.AssociatedPlayerIds)
        {
            Assert.Contains(associatedPlayerId, imported.AssociatedPlayerIds);
        }
        Assert.Equal(competitorCi.ReferenceId.ReferenceIds.Count, imported.ReferenceId.ReferenceIds.Count);
    }

    [Fact]
    public void NamesHasCulture()
    {
        var competitorDto = new CompetitorDto(GetApiTeamFull(1));

        var competitorCi = new CompetitorCacheItem(competitorDto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Equal(competitorDto.Name, competitorCi.Names[TestData.Culture]);
    }

    [Fact]
    public void NamesDoesNotHasCulture()
    {
        var competitorDto = new CompetitorDto(GetApiTeamFull(1));

        var competitorCi = new CompetitorCacheItem(competitorDto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.False(competitorCi.Names.ContainsKey(TestData.CultureNl));
    }

    [Fact]
    public void GetNameForExistingCulture()
    {
        var competitorDto = new CompetitorDto(GetApiTeamFull(1));

        var competitorCi = new CompetitorCacheItem(competitorDto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Equal(competitorDto.Name, competitorCi.GetName(TestData.Culture));
    }

    [Fact]
    public void GetNameForNonExistingCulture()
    {
        var competitorDto = new CompetitorDto(GetApiTeamFull(1));

        var competitorCi = new CompetitorCacheItem(competitorDto, TestData.Culture, DataRouterManagerMock.Object);

        Assert.Null(competitorCi.GetName(TestData.CultureNl));
    }
}
