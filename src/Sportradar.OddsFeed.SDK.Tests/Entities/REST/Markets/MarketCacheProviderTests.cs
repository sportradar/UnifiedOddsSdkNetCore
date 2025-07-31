// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

public class MarketCacheProviderTests
{
    private readonly IMarketCacheProvider _marketCacheProvider;

    private readonly IReadOnlyList<CultureInfo> _languages;
    private readonly XunitLoggerFactory _loggerFactory;
    private readonly Mock<IDataProvider<EntityList<MarketDescriptionDto>>> _invariantMdProviderMock;
    private readonly Mock<IDataProvider<EntityList<VariantDescriptionDto>>> _variantMdProviderMock;
    private readonly Mock<IDataProvider<MarketDescriptionDto>> _singleVariantMdProviderMock;

    public MarketCacheProviderTests(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new XunitLoggerFactory(outputHelper);
        _languages = TestData.Cultures3.ToList();

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

        foreach (var culture in _languages)
        {
            var invariantList = MarketDescriptionEndpoint.GetDefaultInvariantList();
            invariantList.market = invariantList.market.Select(m => m.AddSuffix(culture.TwoLetterISOLanguageName)).ToArray();
            var variantList = MarketDescriptionEndpoint.GetDefaultVariantList();
            variantList.variant = variantList.variant.Select(m => m.AddSuffix(culture.TwoLetterISOLanguageName)).ToArray();
            _invariantMdProviderMock.Setup(s => s.GetDataAsync(culture.TwoLetterISOLanguageName))
                .ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(invariantList.market));
            _variantMdProviderMock.Setup(s => s.GetDataAsync(culture.TwoLetterISOLanguageName))
                .ReturnsAsync(MarketDescriptionEndpoint.GetVariantDto(variantList.variant));
        }
    }

    [Fact]
    public void ConstructorWhenAllPresentThenSucceed()
    {
        var marketCacheProvider = new MarketCacheProvider(new Mock<IMarketDescriptionsCache>().Object, new Mock<IMarketDescriptionCache>().Object, new Mock<IVariantDescriptionsCache>().Object, _loggerFactory.CreateLogger<MarketCacheProvider>());

        Assert.NotNull(marketCacheProvider);
    }

    [Fact]
    public void ConstructorWhenNullInvariantMarketDescriptionCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(null, new Mock<IMarketDescriptionCache>().Object, new Mock<IVariantDescriptionsCache>().Object, _loggerFactory.CreateLogger<MarketCacheProvider>()));
    }

    [Fact]
    public void ConstructorWhenNullSingleVariantMarketDescriptionCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(new Mock<IMarketDescriptionsCache>().Object, null, new Mock<IVariantDescriptionsCache>().Object, _loggerFactory.CreateLogger<MarketCacheProvider>()));
    }

    [Fact]
    public void ConstructorWhenNullVariantMarketDescriptionListCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(new Mock<IMarketDescriptionsCache>().Object, new Mock<IMarketDescriptionCache>().Object, null, _loggerFactory.CreateLogger<MarketCacheProvider>()));
    }

    [Fact]
    public void ConstructorWhenNullLoggerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(new Mock<IMarketDescriptionsCache>().Object, new Mock<IMarketDescriptionCache>().Object, new Mock<IVariantDescriptionsCache>().Object, null));
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
        await PreloadMarketListAndClearInvocations();

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(282, null, _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyInvariantListApiCalls(Times.Never());
        VerifyVariantListApiCalls(Times.Never());
        VerifyVariantSingleApiCalls(Times.Never());
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
        await PreloadMarketListAndClearInvocations();

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(2820, null, _languages, true);

        marketDescription.ShouldBeNull();
        VerifyInvariantListApiCalls(Times.Never());
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
        await PreloadMarketListAndClearInvocations();

        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(374, specifiers, _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyNoAdditionalMarkerApiCallsWasMade();
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
        await PreloadMarketListAndClearInvocations();

        var specifiers = new Dictionary<string, string> { { "variant", "sr:decided_by_extra_points:bestof:5" } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(239, specifiers, _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyNoAdditionalMarkerApiCallsWasMade();
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

        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenMultipleMethodCallsThenMakesOnlyOneVariantApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextThenCachesNewData()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
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

        Assert.True(_languages.All(a => !marketDescriptionFalse.GetName(a).Equals(marketDescriptionTrue.GetName(a))));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenNothingIsCached()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);

        VerifyVariantSingleApiCalls(Times.Never());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenDoesNotMakeApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);
        await PreloadMarketListAndClearInvocations();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, false);

        VerifyNoAdditionalMarkerApiCallsWasMade();
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

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerPropsThenCachesNewItem()
    {
        SetupDrmForSingleVariantPlayerPropMarket768(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
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

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorPropsThenVariantCacheItemIsCached()
    {
        SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypePlayer()
    {
        const int marketId = 679;
        const string specifiers = "maxovers=20|type=live|inningnr=1";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _languages, true);

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
        await PreloadMarketListAndClearInvocations();

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyNoAdditionalMarkerApiCallsWasMade();
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypeCompetitor()
    {
        const int marketId = 1109;
        const string specifiers = "lapnr=5";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _languages, true);

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
        await PreloadMarketListAndClearInvocations();

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyNoAdditionalMarkerApiCallsWasMade();
    }

    [Fact]
    public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplate()
    {
        const int marketId = 303;
        const string specifiers = "quarternr=4|hcp=-3.5";
        var specifiersCollection = Utilities.SpecifiersStringToReadOnlyDictionary(specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.Equal(2, marketDescription.Outcomes.Count());
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplateThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 303;
        const string specifiers = "quarternr=4|hcp=-3.5";
        await PreloadMarketListAndClearInvocations();

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyNoAdditionalMarkerApiCallsWasMade();
    }

    [Fact]
    public async Task GetMarketDescriptionWithScoreInNameTemplate()
    {
        const int marketId = 41;
        const string specifiers = "score=2:0";
        var specifiersCollection = Utilities.SpecifiersStringToReadOnlyDictionary(specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Equal(28, marketDescription.Outcomes.Count());
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWithScoreInNameTemplateThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 41;
        const string specifiers = "score=2:0";
        await PreloadMarketListAndClearInvocations();

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _languages, true);

        marketDescription.ShouldNotBeNull();
        VerifyNoAdditionalMarkerApiCallsWasMade();
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenInvalidOutcomeThenMarketDescriptionIsReturned()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        var invalidBody = FileHelper.GetFileContent($"variant_market_description_invalid_{marketId}_en.xml");

        var restDeserializer = new Deserializer<market_descriptions>();
        var mapper = new MarketDescriptionMapperFactory();

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidBody));
        var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

        _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(marketId.ToString(),
                It.Is<string>(x => x.Equals(_languages.First().TwoLetterISOLanguageName, StringComparison.Ordinal)),
                variantSpecifier))
            .ReturnsAsync(result);
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(9, marketDescription.Outcomes.Count());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenInvalidOutcomeThenSingleVariantApiCallIsMade()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        var invalidBody = FileHelper.GetFileContent($"variant_market_description_invalid_{marketId}_en.xml");

        var restDeserializer = new Deserializer<market_descriptions>();
        var mapper = new MarketDescriptionMapperFactory();

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidBody));
        var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

        _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(marketId.ToString(),
                It.Is<string>(x => x.Equals(_languages.First().TwoLetterISOLanguageName, StringComparison.Ordinal)),
                variantSpecifier))
            .ReturnsAsync(result);
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        ResetMarketApiCalls();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _languages, true);

        VerifyVariantSingleApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenReturnJustInvariantMd()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = (MarketDescription)await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _languages, true);

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

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _languages, true);

        Assert.Equal(1, marketCacheProviderLogger.CountByLevel(LogLevel.Error));
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenLog()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _languages, true);

        Assert.NotNull(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionForNonExistingMarketIdThenThrow()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _languages, true);

        Assert.Null(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionForNonExistingMarketIdThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";
        await PreloadMarketListAndClearInvocations();

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _languages, true);

        marketDescription.ShouldBeNull();
        VerifyNoAdditionalMarkerApiCallsWasMade();
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenLoadInvariantCacheList()
    {
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(303, null, _languages, true);
        marketDescription.ShouldNotBeNull();
        ResetMarketApiCalls();

        var result = await _marketCacheProvider.ReloadMarketDescriptionAsync(1, null);

        Assert.True(result);
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenFetchInvariantCacheList()
    {
        await PreloadMarketListAndClearInvocations();

        var reloaded = await _marketCacheProvider.ReloadMarketDescriptionAsync(1, null);

        reloaded.ShouldBeTrue();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenFetchVariantCacheList()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";

        var reloaded = await _marketCacheProvider.ReloadMarketDescriptionAsync(marketId, Utilities.SpecifiersStringToReadOnlyDictionary(specifiers));

        reloaded.ShouldBeTrue();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
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
        ReplaceInvariantMarketDescriptionGroupPlayerPropsWithCompetitorProps(768, marketId, _languages);
        var mId = marketId;
        foreach (var culture in _languages)
        {
            var apiMarket = MdForVariantSingle.GetPlayerPropsMarket768().AddSuffix(culture);
            apiMarket.id = marketId;
            apiMarket.variant = variantSpecifier;
            _singleVariantMdProviderMock.Setup(s => s.GetDataAsync(mId.ToString(),
                    It.Is<string>(x => x.Equals(culture.TwoLetterISOLanguageName, StringComparison.Ordinal)),
                    variantSpecifier))
                .ReturnsAsync(new MarketDescriptionDto(apiMarket));
        }
        specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        ResetMarketApiCalls();
    }

    private void ReplaceInvariantMarketDescriptionGroupPlayerPropsWithCompetitorProps(int existingId, int newId, IReadOnlyCollection<CultureInfo> cultures)
    {
        _invariantMdProviderMock.Reset();
        foreach (var culture in cultures)
        {
            var invariantList = MarketDescriptionEndpoint.GetDefaultInvariantList();
            var existingMd = invariantList.market.First(f => f.id.Equals(existingId));
            existingMd.id = newId;
            existingMd.groups = existingMd.groups.Replace("player_props", "competitor_props");
            invariantList.market = invariantList.market.Select(m => m.AddSuffix(culture.TwoLetterISOLanguageName)).ToArray();
            _invariantMdProviderMock.Setup(s => s.GetDataAsync(culture.TwoLetterISOLanguageName))
                .ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(invariantList.market));
        }
    }

    private async Task PreloadMarketListAndClearInvocations()
    {
        var marketDescription1 = await _marketCacheProvider.GetMarketDescriptionAsync(303, null, _languages, true);
        marketDescription1.ShouldNotBeNull();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
        var marketDescription2 = await _marketCacheProvider.GetMarketDescriptionAsync(303, Utilities.SpecifiersStringToReadOnlyDictionary("variant=sr:decided_by_extra_points:bestof:5"), _languages, true);
        marketDescription2.ShouldNotBeNull();
        VerifyVariantListApiCalls(Times.Exactly(_languages.Count));
        ResetMarketApiCalls();
    }

    private void VerifyInvariantListApiCalls(Times times)
    {
        _invariantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>()), times);
    }

    private void VerifyVariantListApiCalls(Times times)
    {
        _variantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>()), times);
    }

    private void VerifyVariantSingleApiCalls(Times times)
    {
        _singleVariantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), times);
    }

    private void ResetMarketApiCalls()
    {
        _invariantMdProviderMock.Invocations.Clear();
        _variantMdProviderMock.Invocations.Clear();
        _singleVariantMdProviderMock.Invocations.Clear();
    }

    private void VerifyNoAdditionalMarkerApiCallsWasMade()
    {
        _invariantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>()), Times.Never);
        _variantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>()), Times.Never);
        _singleVariantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>()), Times.Never);
    }
}
