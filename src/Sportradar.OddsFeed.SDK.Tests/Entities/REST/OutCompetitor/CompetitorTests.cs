using System;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.OutCompetitor;

public class CompetitorTests : CompetitorHelper
{
    private readonly Mock<IProfileCache> _mockProfileCache = new();
    private readonly Mock<ISportEntityFactory> _mockSportEntityFactory = new();
    private readonly Mock<IDataRouterManager> _mockDataRouterManager = new();
    private readonly Mock<ICompetitionCacheItem> _competitionCacheItemMock = new();

    [Fact]
    public void ConstructorWithCompetition_CacheItemIsNull_Throws()
    {
        Assert.Throws<NullReferenceException>(() => new Competitor(null, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetition_ProfileCacheIsNull_Throws()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, null, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetition_CulturesIsNull_Throws()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, _mockProfileCache.Object, null, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetition_SportEntityFactoryIsNull_Throws()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, _mockProfileCache.Object, TestData.Cultures, null, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object));
    }

    [Fact]
    public void ConstructorWithCompetition_CompetitorCacheItemIsNull_DoesNotThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        var competitor = new Competitor(competitorCacheItem, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, (ICompetitionCacheItem)null);

        Assert.NotNull(competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorReferences_CacheItemIsNull_Throws()
    {
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<NullReferenceException>(() => new Competitor((CompetitorCacheItem)null, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferences_ProfileCacheIsNull_Throws()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, null, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferences_CulturesIsNull_Throws()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorCacheItem, _mockProfileCache.Object, null, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferences_SportEntityFactoryIsNull_Throws()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() =>
            new Competitor(
                competitorCacheItem,
                _mockProfileCache.Object,
                TestData.Cultures,
                null,
                ExceptionHandlingStrategy.Catch,
                referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorReferences_CompetitorReferencesIsNull_DoesNotThrows()
    {
        var competitorCacheItem = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);

        var competitor = new Competitor(competitorCacheItem, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, (Dictionary<Urn, ReferenceIdCacheItem>)null);

        Assert.NotNull(competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorId_CacheItemIsNull_Throws()
    {
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor((Urn)null, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorId_ProfileCacheIsNull_Throws()
    {
        var competitorId = GetCompetitorWithDivision().Id;
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorId, null, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorId_CulturesIsNull_Throws()
    {
        var competitorId = GetCompetitorWithDivision().Id;
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorId, _mockProfileCache.Object, null, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorId_SportEntityFactoryIsNull_Throws()
    {
        var competitorId = GetCompetitorWithDivision().Id;
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();

        Assert.Throws<ArgumentNullException>(() => new Competitor(competitorId, _mockProfileCache.Object, TestData.Cultures, null, ExceptionHandlingStrategy.Catch, referenceDictionary));
    }

    [Fact]
    public void ConstructorWithCompetitorId_CompetitorReferencesIsNull_DoesNotThrows()
    {
        var competitorId = GetCompetitorWithDivision().Id;

        var competitor = new Competitor(competitorId, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, null);

        Assert.NotNull(competitor);
    }

    [Fact]
    public void ConstructorWithCompetition_ValidBaseParameters_InitializeCorrectly()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object);

        ValidateCompetitor(competitorCi, competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorReferences_ValidBaseParameters_InitializeCorrectly()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        ValidateCompetitor(competitorCi, competitor);
    }

    [Fact]
    public void ConstructorWithCompetitorId_ValidBaseParameters_InitializeCorrectly()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi.Id, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        ValidateCompetitor(competitorCi, competitor);
    }

    [Fact]
    public void ConstructorWithCompetition_WithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, _competitionCacheItemMock.Object);

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
    }

    [Fact]
    public void ConstructorWithCompetitorReferences_WithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
    }

    [Fact]
    public void ConstructorWithCompetitorId_WithoutDivision()
    {
        var competitorCi = new CompetitorCacheItem(GetCompetitorWithoutDivision(), TestData.Culture, _mockDataRouterManager.Object);
        var referenceDictionary = new Dictionary<Urn, ReferenceIdCacheItem>();
        SetupProfileCacheMockWithCompetitorCi(competitorCi);

        var competitor = new Competitor(competitorCi.Id, _mockProfileCache.Object, TestData.Cultures, _mockSportEntityFactory.Object, ExceptionHandlingStrategy.Catch, referenceDictionary);

        Assert.NotNull(competitor);
        Assert.Null(competitor.Division);
    }

    [Fact]
    public void Division_GetOrLoadCompetitorReturnNullCi()
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
    public void PrintC_WithDivision()
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
    public void PrintC_WithoutDivision()
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
    public void PrintF_WithDivision()
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
    public void PrintF_WithoutDivision()
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

    private void SetupProfileCacheMockWithCompetitorCi(CompetitorCacheItem competitorCi)
    {
        _mockProfileCache.Setup(s => s.GetCompetitorNamesAsync(It.IsAny<Urn>(), TestData.Cultures, true)).ReturnsAsync(competitorCi.Names as IReadOnlyDictionary<CultureInfo, string>);
        _mockProfileCache.Setup(s => s.GetCompetitorNameAsync(It.IsAny<Urn>(), TestData.Culture, true)).ReturnsAsync(competitorCi.Names[TestData.Culture]);
        _mockProfileCache.Setup(s => s.GetCompetitorProfileAsync(It.IsAny<Urn>(), TestData.Cultures, true)).ReturnsAsync(competitorCi);
    }

    private void ValidateCompetitor(CompetitorCacheItem competitorCi, ICompetitor competitor)
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
        Assert.Equal(competitorCi.IsVirtual, competitor.IsVirtual);
        Assert.Equal(competitorCi.CountryCode, competitor.CountryCode);
        Assert.Equal(competitorCi.Gender, competitor.Gender);
        Assert.Equal(competitorCi.AgeGroup, competitor.AgeGroup);
        Assert.Equal(competitorCi.State, competitor.State);
        Assert.Equal(competitorCi.ShortName, competitor.ShortName);
        Assert.Equal(competitorCi.Division.Id, competitor.Division.Id);
        Assert.Equal(competitorCi.Division.Name, competitor.Division.Name);
    }
}
