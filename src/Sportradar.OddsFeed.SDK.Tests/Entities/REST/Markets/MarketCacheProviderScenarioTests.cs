// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using xRetry;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class MarketCacheProviderScenarioTests
{
    private readonly IMarketCacheProvider _marketCacheProvider;
    private readonly List<CultureInfo> _languages;
    private readonly XunitLoggerFactory _loggerFactory;

    private readonly Mock<IDataProvider<EntityList<MarketDescriptionDto>>> _invariantMdProviderMock;
    private readonly Mock<IDataProvider<EntityList<VariantDescriptionDto>>> _variantMdProviderMock;
    private readonly Mock<IDataProvider<MarketDescriptionDto>> _singleVariantMdProviderMock;

    public MarketCacheProviderScenarioTests(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new XunitLoggerFactory(outputHelper);
        _languages = TestData.Cultures.ToList();

        _invariantMdProviderMock = new Mock<IDataProvider<EntityList<MarketDescriptionDto>>>();
        _variantMdProviderMock = new Mock<IDataProvider<EntityList<VariantDescriptionDto>>>();
        _singleVariantMdProviderMock = new Mock<IDataProvider<MarketDescriptionDto>>();

        var profileCache = new Mock<IProfileCache>().Object;

        var cacheManager = new CacheManager();
        var dataRouterManager = new DataRouterManagerBuilder()
                               .AddMockedDependencies()
                               .WithCacheManager(cacheManager)
                               .WithInvariantMarketDescriptionsProvider(_invariantMdProviderMock.Object)
                               .WithVariantDescriptionsProvider(_variantMdProviderMock.Object)
                               .WithVariantMarketDescriptionProvider(_singleVariantMdProviderMock.Object)
                               .Build();

        _marketCacheProvider = MarketCacheProviderBuilder.Create()
                                                         .WithCacheManager(cacheManager)
                                                         .WithDataRouterManager(dataRouterManager)
                                                         .WithLanguages(_languages)
                                                         .WithLoggerFactory(_loggerFactory)
                                                         .WithProfileCache(profileCache)
                                                         .Build();

        SetupInvariantListEndpointForAllLanguages();
        SetupVariantListEndpointForAllLanguages();
    }

    [Fact]
    public async Task GetInvariantMarketDescription()
    {
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(282, null, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(282, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Equal(2, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetInvariantMarketDescriptionDoesNotMakeAdditionalApiCalls()
    {
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(282, null, _languages, true);

        VerifyInvariantMarketDescriptionApiCalls(Times.Once());
        VerifyVariantMarketDescriptionApiCalls(Times.Never());
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetNonExistingInvariantMarketDescriptionThenReturnNull()
    {
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(2820, null, _languages, true);

        marketDescription.ShouldBeNull();
    }

    [Fact]
    public async Task GetNonExistingInvariantMarketDescriptionThenDoesNotMakeAdditionalApiCall()
    {
        await InitializeMarketDescriptionLists();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(2820, null, _languages, true);

        VerifyInvariantMarketDescriptionApiCalls(Times.Never());
        VerifyVariantMarketDescriptionApiCalls(Times.Never());
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromList()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(374, specifiers, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(374, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.Equal(13, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromListThenDoesNotMakeAdditionalApiCalls()
    {
        await InitializeMarketDescriptionLists();
        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(374, specifiers, _languages, true);

        VerifyInvariantMarketDescriptionApiCalls(Times.Never());
        VerifyVariantMarketDescriptionApiCalls(Times.Never());
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromListWithMultipleMappings()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:decided_by_extra_points:bestof:5" } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(239, specifiers, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(239, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(6, marketDescription.Outcomes.Count());
        Assert.Equal(4, marketDescription.Mappings.Count());
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromListWithMultipleMappingsThenDoesNotMakeAdditionalApiCalls()
    {
        await InitializeMarketDescriptionLists();
        var specifiers = new Dictionary<string, string> { { "variant", "sr:decided_by_extra_points:bestof:5" } };

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(239, specifiers, _languages, true);

        VerifyInvariantMarketDescriptionApiCalls(Times.Never());
        VerifyVariantMarketDescriptionApiCalls(Times.Never());
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeText()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(9, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextThenMakesSingleVariantApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Once());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenMultipleMethodCallsThenMakesOnlyOneVariantApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Once());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextThenCachesNewData()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        marketDescription.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenOnlyInvariantMdIsReturned()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);

        Assert.Equal(marketId, marketDescription.Id);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Null(marketDescription.Outcomes);
        _ = Assert.Single(marketDescription.Mappings);
    }

    [Fact]
    public async Task SingleVariantMarketWithFetchVariantDisabledReturnsOnlyInvariantMarketDescriptionThenBaseAttributesTheSame()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescriptionFalse = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);
        var marketDescriptionTrue = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        Assert.Equal(marketDescriptionFalse.Id, marketDescriptionTrue.Id);
        Assert.Contains("variant", marketDescriptionFalse.Specifiers.Select(s => s.Name));
        Assert.Contains("variant", marketDescriptionTrue.Specifiers.Select(s => s.Name));
    }

    [Fact]
    public async Task SingleVariantMarketWithFetchVariantDisabledReturnsOnlyInvariantMarketDescriptionThenOutcomesDiffer()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescriptionFalse = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);
        var marketDescriptionTrue = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        Assert.Null(marketDescriptionFalse.Outcomes);
        Assert.Equal(9, marketDescriptionTrue.Outcomes.Count());
        _ = Assert.Single(marketDescriptionFalse.Mappings);
        _ = Assert.Single(marketDescriptionTrue.Mappings);
    }

    [Fact]
    public async Task SingleVariantMarketWithFetchVariantDisabledReturnsOnlyInvariantMarketDescriptionThenMarketNameDiffer()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescriptionFalse = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);
        var marketDescriptionTrue = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        Assert.True(_languages.All(a => !marketDescriptionFalse.GetName(a).Equals(marketDescriptionTrue.GetName(a), StringComparison.Ordinal)));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenNothingIsCached()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenDoesNotMakeApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerProps()
    {
        SetupDrmForSingleVariantPlayerPropMarket768(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(18, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerPropsThenMakesApiCall()
    {
        SetupDrmForSingleVariantPlayerPropMarket768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Once());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerPropsThenCachesNewItem()
    {
        SetupDrmForSingleVariantPlayerPropMarket768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Once());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorProps()
    {
        SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(18, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorPropsThenMakesApiCalls()
    {
        SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Once());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorPropsThenVariantCacheItemIsCached()
    {
        SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Once());
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypePlayer()
    {
        const int marketId = 679;
        const string specifiers = "maxovers=20|type=live|inningnr=1";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        var marketDescription =
            await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.True(marketDescription.Outcomes.IsNullOrEmpty());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypePlayerThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 679;
        const string specifiers = "maxovers=20|type=live|inningnr=1";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypeCompetitor()
    {
        const int marketId = 1109;
        const string specifiers = "lapnr=5";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        var marketDescription =
            await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.True(marketDescription.Outcomes.IsNullOrEmpty());
        _ = Assert.Single(marketDescription.Mappings);
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypeCompetitorThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 1109;
        const string specifiers = "lapnr=5";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [RetryFact(3, 5000)]
    public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplate()
    {
        const int marketId = 303;
        const string specifiers = "quarternr=4|hcp=-3.5";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        var marketDescription =
            await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.Equal(2, marketDescription.Outcomes.Count());
        marketDescription.Mappings.ShouldBeNull();
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplateThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 303;
        const string specifiers = "quarternr=4|hcp=-3.5";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [RetryFact(3, 5000)]
    public async Task GetMarketDescriptionWithScoreInNameTemplate()
    {
        const int marketId = 41;
        const string specifiers = "score=2:0";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        var marketDescription =
            await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Equal(28, marketDescription.Outcomes.Count());
        marketDescription.Mappings.ShouldBeNull();
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWithScoreInNameTemplateThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 41;
        const string specifiers = "score=2:0";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenInvalidOutcomeThenMarketDescriptionIsReturned()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };

        var marketDescriptionXmlBody = FileHelper.GetFileContent("variant_market_description_invalid_534_en.xml");
        var apiMd = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(marketDescriptionXmlBody).market[0];
        foreach (var culture in _languages)
        {
            _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(marketId.ToString(),
                                                                   It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal)),
                                                                   variantSpecifier))
                                        .ReturnsAsync(new MarketDescriptionDto(apiMd));
        }

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, TestData.Cultures1, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(9, marketDescription.Outcomes.Count());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenInvalidOutcomeThenSingleVariantApiCallIsMade()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };

        var marketDescriptionXmlBody = FileHelper.GetFileContent("variant_market_description_invalid_534_culture.xml");
        foreach (var culture in _languages)
        {
            _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(marketId.ToString(),
                                                                   It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal)),
                                                                   variantSpecifier))
                                        .ThrowsAsync(new DeserializationException("Error", marketDescriptionXmlBody, "market_description_invalid_534_culture.xml", null));
        }

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, TestData.Cultures1, true);

        _singleVariantMdProviderMock.Verify(v => v.GetDataAsync(It.IsAny<string>(), TestData.Cultures1.First().TwoLetterISOLanguageName, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenReturnJustInvariantMd()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";

        var marketDescription =
            (MarketDescription)await _marketCacheProvider.GetMarketDescriptionAsync(marketId,
                Utilities.SpecifiersStringToReadOnlyDictionary(specifiers),
                                                                                    _languages,
                                                                                    true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Null(marketDescription.Outcomes);
        Assert.Equal(2, marketDescription.Specifiers.Count());
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenExecutionErrorIsLogged()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";
        var marketCacheProviderLogger = _loggerFactory.GetOrCreateLogger(typeof(MarketCacheProvider));

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        Assert.Equal(1, marketCacheProviderLogger.CountByLevel(LogLevel.Error));
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenLog()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";

        var marketDescription =
            await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        Assert.NotNull(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionForNonExistingMarketIdThenThrow()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";

        var marketDescription =
            await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        Assert.Null(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionForNonExistingMarketIdThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers), _languages, true);

        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenLoadInvariantCacheList()
    {
        VerifySingleVariantMarketDescriptionApiCalls(Times.Never());

        var result = await _marketCacheProvider.ReloadMarketDescriptionAsync(1, null);

        Assert.True(result);
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenFetchInvariantCacheList()
    {
        VerifyInvariantMarketDescriptionApiCalls(Times.Never());

        _ = await _marketCacheProvider.ReloadMarketDescriptionAsync(1, null);

        VerifyInvariantMarketDescriptionApiCalls(Times.Once());
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenFetchVariantCacheList()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";

        _ = await _marketCacheProvider.ReloadMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers));

        VerifyInvariantMarketDescriptionApiCalls(Times.Once());
        VerifyVariantMarketDescriptionApiCalls(Times.Never());
    }

    private void SetupDrmForSingleVariantMarket534(out int marketId, out IReadOnlyDictionary<string, string> specifiers)
    {
        marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };

        var mId = marketId;
        foreach (var culture in _languages)
        {
            _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(mId.ToString(),
                                                                   It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal)),
                                                                   variantSpecifier))
                                        .ReturnsAsync(new MarketDescriptionDto(MdForVariantSingle.GetPreOutcomeMarket534().AddSuffix(culture)));
        }
    }

    private void SetupDrmForSingleVariantPlayerPropMarket768(out int marketId, out IReadOnlyDictionary<string, string> specifiers)
    {
        marketId = 768;
        const string variantSpecifier = "pre:playerprops:35432179:608000";
        specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };

        var mId = marketId;
        foreach (var culture in _languages)
        {
            _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(mId.ToString(),
                                                                   It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal)),
                                                                   variantSpecifier))
                                        .ReturnsAsync(new MarketDescriptionDto(MdForVariantSingle.GetPlayerPropsMarket768().AddSuffix(culture)));
        }
    }

    private void SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out int marketId, out IReadOnlyDictionary<string, string> specifiers)
    {
        marketId = 1768;
        const string variantSpecifier = "pre:competitorprops:35432179:608000";
        specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };

        var mId = marketId;
        foreach (var culture in _languages)
        {
            var md = MdForVariantSingle.GetCompetitorPropsMarket1768().AddSuffix(culture);
            _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(mId.ToString(),
                                                                   It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal)),
                                                                   variantSpecifier))
                                        .ReturnsAsync(new MarketDescriptionDto(md));
        }
    }

    private void SetupInvariantListEndpointForAllLanguages()
    {
        foreach (var culture in _languages)
        {
            var marketDescriptions = MarketDescriptionEndpoint.GetDefaultInvariantList(culture);
            _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal))))
                                    .ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(marketDescriptions.market));
        }
    }

    private void SetupVariantListEndpointForAllLanguages()
    {
        foreach (var culture in _languages)
        {
            var marketDescriptions = MarketDescriptionEndpoint.GetDefaultVariantList(culture);
            _variantMdProviderMock.Setup(s => s.GetDataAsync(It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal))))
                                  .ReturnsAsync(MarketDescriptionEndpoint.GetVariantDto(marketDescriptions.variant));
        }
    }

    private async Task InitializeMarketDescriptionLists()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(1, specifiers, _languages, true);
        _invariantMdProviderMock.Invocations.Clear();
        _variantMdProviderMock.Invocations.Clear();
        _singleVariantMdProviderMock.Invocations.Clear();
    }

    private void VerifyInvariantMarketDescriptionApiCalls(Times expectedTimes)
    {
        foreach (var culture in _languages)
        {
            _invariantMdProviderMock.Verify(v => v.GetDataAsync(culture.TwoLetterISOLanguageName), expectedTimes);
        }
    }

    private void VerifyVariantMarketDescriptionApiCalls(Times expectedTimes)
    {
        foreach (var culture in _languages)
        {
            _variantMdProviderMock.Verify(v => v.GetDataAsync(culture.TwoLetterISOLanguageName), expectedTimes);
        }
    }

    private void VerifySingleVariantMarketDescriptionApiCalls(Times expectedTimes)
    {
        foreach (var language in _languages)
        {
            _singleVariantMdProviderMock.Verify(v => v.GetDataAsync(It.IsAny<string>(), language.TwoLetterISOLanguageName, It.IsAny<string>()), expectedTimes);
        }
    }
}
