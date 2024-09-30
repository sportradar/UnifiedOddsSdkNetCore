// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.OutCompetitor;

public class CompetitorTests : CompetitorSetup
{
    private readonly Mock<ICompetitionCacheItem> _competitionCacheItemMock = new();
    private readonly Mock<IDataRouterManager> _mockDataRouterManager = new();
    private readonly Mock<IProfileCache> _mockProfileCache = new();
    private readonly Mock<ISportEntityFactory> _mockSportEntityFactory = new();

    [Fact]
    public void ConstructorWithCompetitionCacheItemIsNullThrows()
    {
        Assert.Throws<NullReferenceException>(() => new Competitor(null, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetitionProfileCacheIsNullThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, null, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetitionCulturesIsNullThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, _mockProfileCache.Object, null, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetitionSportEntityFactoryIsNullThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, _mockProfileCache.Object, TestData.Cultures, null, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetitionCompetitorCacheItemIsNullDoesNotThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        var competitor = new Competitor(competitorCacheItem, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, (ICompetitionCacheItem)null);

        Assert.NotNull(competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorReferencesCacheItemIsNullThrows()
    {
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<NullReferenceException>(() => new Competitor((CompetitorCacheItem)null, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferencesProfileCacheIsNullThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, null, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferencesCulturesIsNullThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, _mockProfileCache.Object, null, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferencesSportEntityFactoryIsNullThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() =>
                                                 new Competitor(competitorCacheItem,
                                                                _mockProfileCache.Object,
                                                                TestData.Cultures,
                                                                null,
                                                                ExceptionHandlingStrategy.Catch,
                                                                referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferencesCompetitorReferencesIsNullDoesNotThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        var competitor = new Competitor(competitorCacheItem, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, (Dictionary<Urn, ReferenceIdCacheItem>)null);

        Assert.NotNull(competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorIdCacheItemIsNullThrows()
    {
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor((Urn)null, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorIdProfileCacheIsNullThrows()
    {
        var competitorId = GetCompetitorWithDivision().Id;
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorId, null, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorIdCulturesIsNullThrows()
    {
        var competitorId = GetCompetitorWithDivision().Id;
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorId, _mockProfileCache.Object, null, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorIdSportEntityFactoryIsNullThrows()
    {
        var competitorId = GetCompetitorWithDivision().Id;
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorId, _mockProfileCache.Object, TestData.Cultures, null, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorIdCompetitorReferencesIsNullDoesNotThrows()
    {
        var competitorId = GetCompetitorWithDivision().Id;

        var competitor = new Competitor(competitorId, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, null);

        Assert.NotNull(competitor);
    }

    [Fact]
    public void ConstructorWithCompetitionValidBaseParametersInitializeCorrectly()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object);

        ValidateCompetitor(competitorCi, competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorReferencesValidBaseParametersInitializeCorrectly()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        ValidateCompetitor(competitorCi, competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorIdValidBaseParametersInitializeCorrectly()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi.Id, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        ValidateCompetitor(competitorCi, competitor);
    }

    [Fact]
    public void ConstructorWithCompetitionWithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object);

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
    }

    [Fact]
    public void ConstructorWithCompetitorReferencesWithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
    }

    [Fact]
    public void ConstructorWithCompetitorIdWithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi.Id, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
    }

    [Fact]
    public void DivisionGetOrLoadCompetitorReturnNullCi()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);
        _mockProfileCache.Setup(s => s.GetCompetitorProfileAsync(It.IsAny<Urn>(), TestData.Cultures, true)).ReturnsAsync((CompetitorCacheItem)null);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        Assert.NotNull(competitorCi.Division);
        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
    }

    [Fact]
    public void PrintCWithDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);
        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        var print = competitor.ToString("C");

        Assert.NotNull(competitor);
        Assert.NotNull(competitor.Division);
        Assert.DoesNotContain("Division=", print, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void PrintCWithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);
        var competitor = new Competitor(competitorCi.Id, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        var print = competitor.ToString("C");

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
        Assert.DoesNotContain("Division=", print, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void PrintFWithDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);
        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        var print = competitor.ToString("F");

        Assert.NotNull(competitor);
        Assert.NotNull(competitor.Division);
        Assert.Contains("Division=", print, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void PrintFWithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);
        var competitor = new Competitor(competitorCi.Id, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        var print = competitor.ToString("F");

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
        Assert.DoesNotContain("Division=", print, StringComparison.InvariantCultureIgnoreCase);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(null, false)]
    public void ConstructorWithCompetitorCacheItemInitializesVirtualFlag(bool? competitorIsVirtual, bool expectedVirtualFlag)
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithVirtualFlagSet(competitorIsVirtual), TestData.Culture, _mockDataRouterManager.Object);
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object);

        Assert.Equal(expectedVirtualFlag, competitor.IsVirtual);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(null, false)]
    public void ConstructorWithCompetitorCacheItemAndCompetitorsReferencesInitializesVirtualFlag(bool? competitorIsVirtual, bool expectedVirtualFlag)
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithVirtualFlagSet(competitorIsVirtual), TestData.Culture, _mockDataRouterManager.Object);
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, new Dictionary<Urn, ReferenceIdCacheItem>());

        Assert.Equal(expectedVirtualFlag, competitor.IsVirtual);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(null, false)]
    public void ConstructorWithProfileCacheInitializesVirtualFlag(bool? competitorIsVirtual, bool expectedVirtualFlag)
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithVirtualFlagSet(competitorIsVirtual), TestData.Culture, _mockDataRouterManager.Object);
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi.Id, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, new Dictionary<Urn, ReferenceIdCacheItem>());

        Assert.Equal(expectedVirtualFlag, competitor.IsVirtual);
    }

    [Fact]
    public void JerseyNumberWhenNumberPresentThenAvailableInPlayer()
    {
        var dto1 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var competitorCi = new CompetitorCacheItem(dto1, TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        _mockSportEntityFactory.Setup(s => s
                                         .BuildPlayersAsync(competitorCi.AssociatedPlayerIds.ToList(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<ExceptionHandlingStrategy>(), competitorCi.AssociatedPlayersJerseyNumbers))
                               .ReturnsAsync(dto1.Players.Select(p1 => new CompetitorPlayer(new PlayerProfileCacheItem(p1, dto1.Competitor.Id, TestData.Culture, _mockDataRouterManager.Object), TestData.Cultures1, p1.JerseyNumber))
                                                 .ToList());
        _mockProfileCache.Setup(s => s.GetCompetitorProfileAsync(It.IsAny<Urn>(), TestData.Cultures1, true)).ReturnsAsync(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures1, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);
        var competitorPlayer = competitor.AssociatedPlayers.ToList();

        Assert.NotNull(competitor);
        Assert.NotEmpty(competitorPlayer);
        Assert.All(competitorPlayer, p => Assert.True(((ICompetitorPlayer)p).JerseyNumber > 0));
    }

    [Fact]
    public void JerseyNumberWhenNoNumberPresentThenNotAvailableInPlayer()
    {
        var dto1 = GetCompetitorProfileDtoWithPlayerJerseyNumber(false);
        var competitorCi = new CompetitorCacheItem(dto1, TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        _mockSportEntityFactory.Setup(s => s
                                         .BuildPlayersAsync(competitorCi.AssociatedPlayerIds.ToList(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<ExceptionHandlingStrategy>(), competitorCi.AssociatedPlayersJerseyNumbers))
                               .ReturnsAsync(dto1.Players.Select(p1 => new CompetitorPlayer(new PlayerProfileCacheItem(p1, dto1.Competitor.Id, TestData.Culture, _mockDataRouterManager.Object), TestData.Cultures1, p1.JerseyNumber))
                                                 .ToList());
        _mockProfileCache.Setup(s => s.GetCompetitorProfileAsync(It.IsAny<Urn>(), TestData.Cultures1, true)).ReturnsAsync(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures1, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);
        var competitorPlayer = competitor.AssociatedPlayers.ToList();

        Assert.NotNull(competitor);
        Assert.NotEmpty(competitorPlayer);
        Assert.All(competitorPlayer, p => Assert.Null(((ICompetitorPlayer)p).JerseyNumber));
    }

    [Fact]
    public void JerseyNumberWhenValidAndSportEntityFactoryThrowsThenEmptyPlayerList()
    {
        var dto1 = GetCompetitorProfileDtoWithPlayerJerseyNumber(true);
        var competitorCi = new CompetitorCacheItem(dto1, TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        _mockSportEntityFactory.Setup(s => s
                                         .BuildPlayersAsync(competitorCi.AssociatedPlayerIds.ToList(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<ExceptionHandlingStrategy>(), competitorCi.AssociatedPlayersJerseyNumbers))
                               .Throws(new InvalidOperationException("any-message"));
        _mockProfileCache.Setup(s => s.GetCompetitorProfileAsync(It.IsAny<Urn>(), TestData.Cultures1, true)).ReturnsAsync(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures1, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);
        var competitorPlayer = competitor.AssociatedPlayers.ToList();

        Assert.NotNull(competitor);
        Assert.Empty(competitorPlayer);
    }

    private void SetupProfileCacheMockWithCompetitorCi(CompetitorCacheItem competitorCi)
    {
        _mockProfileCache.Setup(s => s.GetCompetitorNamesAsync(It.IsAny<Urn>(), TestData.Cultures, true)).ReturnsAsync(competitorCi.Names as IReadOnlyDictionary<CultureInfo, string>);
        _mockProfileCache.Setup(s => s.GetCompetitorNameAsync(It.IsAny<Urn>(), TestData.Culture, true)).ReturnsAsync(competitorCi.Names[TestData.Culture]);
        _mockProfileCache.Setup(s => s.GetCompetitorProfileAsync(It.IsAny<Urn>(), TestData.Cultures, true)).ReturnsAsync(competitorCi);
    }

    private static void ValidateCompetitor(CompetitorCacheItem competitorCi, Competitor competitor)
    {
        Assert.NotNull(competitor);
        Assert.Equal(competitorCi.Id, competitor.Id);
        Assert.Single(competitor.Names);
        Assert.Equal(competitorCi.GetName(TestData.Culture), competitor.Names[TestData.Culture]);
        Assert.Equal(competitorCi.GetName(TestData.Culture), competitor.GetName(TestData.Culture));
        Assert.Single(competitor.Countries);
        Assert.Equal(competitorCi.GetCountry(TestData.Culture), competitor.Countries[TestData.Culture]);
        Assert.Equal(competitorCi.GetCountry(TestData.Culture), competitor.GetCountry(TestData.Culture));
        Assert.Single(competitor.Abbreviations);
        Assert.Equal(competitorCi.GetAbbreviation(TestData.Culture), competitor.Abbreviations[TestData.Culture]);
        Assert.Equal(competitorCi.GetAbbreviation(TestData.Culture), competitor.GetAbbreviation(TestData.Culture));
        Assert.Equal(competitorCi.CountryCode, competitor.CountryCode);
        Assert.Equal(competitorCi.Gender, competitor.Gender);
        Assert.Equal(competitorCi.AgeGroup, competitor.AgeGroup);
        Assert.Equal(competitorCi.State, competitor.State);
        Assert.Equal(competitorCi.ShortName, competitor.ShortName);
        Assert.Equal(competitorCi.Division.Id, competitor.Division.Id);
        Assert.Equal(competitorCi.Division.Name, competitor.Division.Name);
        if (competitorCi.IsVirtual.HasValue)
        {
            Assert.Equal(competitorCi.IsVirtual.Value, competitor.IsVirtual);
        }
        else
        {
            Assert.False(competitor.IsVirtual);
        }
    }
}
