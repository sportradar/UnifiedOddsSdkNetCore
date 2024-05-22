// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.MockLog;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

public class MarketCacheProviderTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IMarketDescriptionsCache _invariantListCache;
    private readonly IVariantDescriptionsCache _variantListCache;
    private readonly IMarketDescriptionCache _variantSingleCache;
    private readonly IMarketCacheProvider _marketCacheProvider;

    private readonly TestDataRouterManager _dataRouterManager;
    private readonly ICacheStore<string> _invariantListMemoryCache;
    private readonly ICacheStore<string> _variantListMemoryCache;
    private readonly ICacheStore<string> _variantSingleMemoryCache;
    private readonly IReadOnlyList<CultureInfo> _cultures;
    private readonly CacheManager _cacheManager;
    private readonly XunitLoggerFactory _loggerFactory;

    public MarketCacheProviderTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _loggerFactory = new XunitLoggerFactory(outputHelper);
        _cultures = TestData.Cultures.ToList();

        var testCacheStoreManager = new TestCacheStoreManager();
        _invariantListMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache);
        _variantListMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache);
        _variantSingleMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache);

        _cacheManager = new CacheManager();
        _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);

        IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

        var timerInvariantList = new TestTimer(true);
        var timerVariantList = new TestTimer(true);
        _invariantListCache = new InvariantMarketDescriptionCache(_invariantListMemoryCache, _dataRouterManager, mappingValidatorFactory, timerInvariantList, _cultures, _cacheManager, _loggerFactory);
        _variantListCache = new VariantDescriptionListCache(_variantListMemoryCache, _dataRouterManager, mappingValidatorFactory, timerVariantList, _cultures, _cacheManager, _loggerFactory);
        _variantSingleCache = new VariantMarketDescriptionCache(_variantSingleMemoryCache, _dataRouterManager, mappingValidatorFactory, _cacheManager, _loggerFactory);

        _marketCacheProvider = new MarketCacheProvider(_invariantListCache, _variantSingleCache, _variantListCache, _loggerFactory.CreateLogger<MarketCacheProvider>());
    }

    [Fact]
    public void ConstructorWhenAllPresentThenSucceed()
    {
        var marketCacheProvider = new MarketCacheProvider(_invariantListCache, _variantSingleCache, _variantListCache, _loggerFactory.CreateLogger<MarketCacheProvider>());

        Assert.NotNull(marketCacheProvider);
    }

    [Fact]
    public void ConstructorWhenNullInvariantMarketDescriptionCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(null, _variantSingleCache, _variantListCache, _loggerFactory.CreateLogger<MarketCacheProvider>()));
    }

    [Fact]
    public void ConstructorWhenNullSingleVariantMarketDescriptionCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(_invariantListCache, null, _variantListCache, _loggerFactory.CreateLogger<MarketCacheProvider>()));
    }

    [Fact]
    public void ConstructorWhenNullVariantMarketDescriptionListCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(_invariantListCache, _variantSingleCache, null, _loggerFactory.CreateLogger<MarketCacheProvider>()));
    }

    [Fact]
    public void ConstructorWhenNullLoggerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new MarketCacheProvider(_invariantListCache, _variantSingleCache, _variantListCache, null));
    }

    [Fact]
    public void InitialCallsDone()
    {
        Assert.Equal(TestData.InvariantListCacheCount, _invariantListMemoryCache.Count());
        Assert.True(_variantListMemoryCache.Count() > 0);
        Assert.Equal(0, _variantSingleMemoryCache.Count());

        Assert.NotNull(_invariantListCache);
        Assert.NotNull(_variantListCache);
        Assert.NotNull(_variantSingleCache);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));

        Assert.Equal(TestData.InvariantListCacheCount, _invariantListMemoryCache.Count());
        Assert.Equal(TestData.VariantListCacheCount, _variantListMemoryCache.Count());
    }

    [Fact]
    public async Task GetInvariantMarketDescription()
    {
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(282, null, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(282, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Equal(2, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetInvariantMarketDescriptionDoesNotMakeAdditionalApiCalls()
    {
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(282, null, _cultures, true);

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetNonExistingInvariantMarketDescriptionThenReturnNull()
    {
        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(2820, null, _cultures, true);

        Assert.Null(marketDescription);
    }

    [Fact]
    public async Task GetNonExistingInvariantMarketDescriptionThenDoesNotMakeAdditionalApiCall()
    {
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(2820, null, _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromList()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(374, specifiers, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(374, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.Equal(13, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromListThenDoesNotMakeAdditionalApiCalls()
    {
        _dataRouterManager.ResetMethodCall();
        var specifiers = new Dictionary<string, string> { { "variant", "sr:correct_score:bestof:12" } };

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(374, specifiers, _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromListWithMultipleMappings()
    {
        var specifiers = new Dictionary<string, string> { { "variant", "sr:decided_by_extra_points:bestof:5" } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(239, specifiers, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(239, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(6, marketDescription.Outcomes.Count());
        Assert.Equal(4, marketDescription.Mappings.Count());
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromListWithMultipleMappingsThenDoesNotMakeAdditionalApiCalls()
    {
        _dataRouterManager.ResetMethodCall();

        var specifiers = new Dictionary<string, string> { { "variant", "sr:decided_by_extra_points:bestof:5" } };

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(239, specifiers, _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeText()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(9, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextThenMakesSingleVariantApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenMultipleMethodCallsThenMakesOnlyOneVariantApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);
        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextThenCachesNewData()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(1, _variantSingleMemoryCache.Count());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenOnlyInvariantMdIsReturned()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, false);

        Assert.Equal(marketId, marketDescription.Id);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Null(marketDescription.Outcomes);
        _ = Assert.Single(marketDescription.Mappings);
    }

    [Fact]
    public async Task SingleVariantMarketWithFetchVariantDisabledReturnsOnlyInvariantMarketDescriptionThenBaseAttributesTheSame()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescriptionFalse = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, false);
        var marketDescriptionTrue = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(marketDescriptionFalse.Id, marketDescriptionTrue.Id);
        Assert.Contains("variant", marketDescriptionFalse.Specifiers.Select(s => s.Name));
        Assert.Contains("variant", marketDescriptionTrue.Specifiers.Select(s => s.Name));
    }

    [Fact]
    public async Task SingleVariantMarketWithFetchVariantDisabledReturnsOnlyInvariantMarketDescriptionThenOutcomesDiffer()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescriptionFalse = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, false);
        var marketDescriptionTrue = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Null(marketDescriptionFalse.Outcomes);
        Assert.Equal(9, marketDescriptionTrue.Outcomes.Count());
        _ = Assert.Single(marketDescriptionFalse.Mappings);
        _ = Assert.Single(marketDescriptionTrue.Mappings);
    }

    [Fact]
    public async Task SingleVariantMarketWithFetchVariantDisabledReturnsOnlyInvariantMarketDescriptionThenMarketNameDiffer()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        var marketDescriptionFalse = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, false);
        var marketDescriptionTrue = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.True(_cultures.All(a => !marketDescriptionFalse.GetName(a).Equals(marketDescriptionTrue.GetName(a))));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenNothingIsCached()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, false);

        Assert.Equal(0, _variantSingleMemoryCache.Count());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenFetchVariantIsDisabledThenDoesNotMakeApiCall()
    {
        SetupDrmForSingleVariantMarket534(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, false);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerProps()
    {
        SetupDrmForSingleVariantPlayerPropMarket768(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(18, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerPropsThenMakesApiCall()
    {
        SetupDrmForSingleVariantPlayerPropMarket768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePlayerPropsThenCachesNewItem()
    {
        SetupDrmForSingleVariantPlayerPropMarket768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(1, _variantSingleMemoryCache.Count());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorProps()
    {
        SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out var marketId, out var specifiers);

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Contains("variant", marketDescription.Specifiers.Select(s => s.Name));
        Assert.Equal(18, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorPropsThenMakesApiCalls()
    {
        SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSingleUnsupportedCompetitorPropsThenVariantCacheItemIsCached()
    {
        SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out var marketId, out var specifiers);

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, _cultures, true);

        Assert.Equal(1, _variantSingleMemoryCache.Count());
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypePlayer()
    {
        const int marketId = 679;
        const string specifiers = "maxovers=20|type=live|inningnr=1";
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.True(marketDescription.Outcomes.IsNullOrEmpty());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypePlayerThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 679;
        const string specifiers = "maxovers=20|type=live|inningnr=1";
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetMarketDescriptionForOutComeTypeCompetitor()
    {
        const int marketId = 1109;
        const string specifiers = "lapnr=5";
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _cultures, true);

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
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplate()
    {
        const int marketId = 303;
        const string specifiers = "quarternr=4|hcp=-3.5";
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Equal(2, marketDescription.Specifiers.Count());
        Assert.Equal(2, marketDescription.Outcomes.Count());
        Assert.Equal(25, marketDescription.Mappings.Count());
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWithCompetitorInOutcomeTemplateThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 303;
        const string specifiers = "quarternr=4|hcp=-3.5";
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetMarketDescriptionWithScoreInNameTemplate()
    {
        const int marketId = 41;
        const string specifiers = "score=2:0";
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        _ = Assert.Single(marketDescription.Specifiers);
        Assert.Equal(28, marketDescription.Outcomes.Count());
        Assert.Equal(5, marketDescription.Mappings.Count());
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWithScoreInNameTemplateThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 41;
        const string specifiers = "score=2:0";
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenInvalidOutcomeThenMarketDescriptionIsReturned()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_invalid_{marketId}_culture.xml"));
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, TestData.Cultures1, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(9, marketDescription.Outcomes.Count());
    }

    [Fact]
    public async Task GetVariantMarketDescriptionForSinglePreOutcomeTextWhenInvalidOutcomeThenSingleVariantApiCallIsMade()
    {
        const int marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_invalid_{marketId}_culture.xml"));
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        var specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, TestData.Cultures1, true);

        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenReturnJustInvariantMd()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = (MarketDescription)await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _cultures, true);

        Assert.NotNull(marketDescription);
        Assert.Equal(marketId, marketDescription.Id);
        Assert.Null(marketDescription.Outcomes);
        Assert.Equal(_invariantListCache.CacheName, marketDescription.MarketDescriptionCacheItem.SourceCache);
        Assert.Equal(2, marketDescription.Specifiers.Count());
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenExecutionErrorIsLogged()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";
        var marketCacheProviderLogger = _loggerFactory.GetOrCreateLogger(typeof(MarketCacheProvider));

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(1, marketCacheProviderLogger.CountByLevel(LogLevel.Error));
    }

    [Fact]
    public async Task WhenVariantValueEqualsNullThenLog()
    {
        const int marketId = 679;
        const string specifiers = "variant=null";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _cultures, true);

        Assert.NotNull(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionForNonExistingMarketIdThenThrow()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";
        var specifiersCollection = new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers));

        var marketDescription = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiersCollection, _cultures, true);

        Assert.Null(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionForNonExistingMarketIdThenMakesNoAdditionalApiCalls()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.GetMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)), _cultures, true);

        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenLoadInvariantCacheList()
    {
        _dataRouterManager.ResetMethodCall();

        var result = await _marketCacheProvider.ReloadMarketDescriptionAsync(1, null);

        Assert.True(result);
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenFetchInvariantCacheList()
    {
        _dataRouterManager.ResetMethodCall();

        _ = await _marketCacheProvider.ReloadMarketDescriptionAsync(1, null);

        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
    }

    [Fact]
    public async Task ReloadMarketDescriptionsWhenNonVariantMarketThenFetchVariantCacheList()
    {
        const int marketId = 110900;
        const string specifiers = "lapnr=5";

        _ = await _marketCacheProvider.ReloadMarketDescriptionAsync(marketId, new ReadOnlyDictionary<string, string>(SdkInfo.SpecifiersStringToDictionary(specifiers)));

        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
    }

    private void SetupDrmForSingleVariantMarket534(out int marketId, out IReadOnlyDictionary<string, string> specifiers)
    {
        marketId = 534;
        const string variantSpecifier = "pre:markettext:168883";
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        _dataRouterManager.ResetMethodCall();
    }

    private void SetupDrmForSingleVariantPlayerPropMarket768(out int marketId, out IReadOnlyDictionary<string, string> specifiers)
    {
        marketId = 768;
        const string variantSpecifier = "pre:playerprops:35432179:608000";
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_culture.xml"));
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        _dataRouterManager.ResetMethodCall();
    }

    private void SetupDrmForSingleVariantMarketForUnsupportedCompetitorProps1768(out int marketId, out IReadOnlyDictionary<string, string> specifiers)
    {
        marketId = 1768;
        const string variantSpecifier = "pre:competitorprops:35432179:608000";
        ReplaceInvariantMarketDescriptionGroupPlayerPropsWithCompetitorPropsAsync(768, marketId, _cultures).GetAwaiter().GetResult();
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{marketId}_{variantSpecifier}", $"variant_market_description_{marketId}_cp_culture.xml"));
        Assert.Equal(TestData.InvariantListCacheCount + 1, _invariantListMemoryCache.Count());
        Assert.Equal(0, _variantSingleMemoryCache.Count());
        specifiers = new Dictionary<string, string> { { "variant", variantSpecifier } };
        _dataRouterManager.ResetMethodCall();
    }

    private async Task ReplaceInvariantMarketDescriptionGroupPlayerPropsWithCompetitorPropsAsync(int existingId, int newId, IReadOnlyCollection<CultureInfo> cultures)
    {
        foreach (var culture in cultures)
        {
            await LoadNewInvariantMarketDescriptionAsync(existingId, newId, 1, culture).ConfigureAwait(false);
        }
    }

    private async Task LoadNewInvariantMarketDescriptionAsync(int existingId, int newId, int doAction, CultureInfo culture)
    {
        var restDeserializer = new Deserializer<market_descriptions>();
        var mapper = new MarketDescriptionsMapperFactory();

        var resourceName = $"invariant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
        await using var stream = FileHelper.GetResource(resourceName);

        if (stream != null)
        {
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
            if (result?.Items.IsNullOrEmpty() == false)
            {
                var existingMd = result.Items.First(f => f.Id.Equals(existingId));
                existingMd.OverrideId(newId);
                switch (doAction)
                {
                    case 1:
                        {
                            // switch player_props to unknown competitor_props
                            var newGroups = existingMd.Groups.Where(s => !s.Contains("player_props")).ToList();
                            newGroups.Add("competitor_props");
                            existingMd.OverrideGroups(newGroups);
                            break;
                        }
                    default:
                        return;
                }

                var items = new EntityList<MarketDescriptionDto>(new List<MarketDescriptionDto> { existingMd });
                await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:1"), items, culture, DtoType.MarketDescriptionList, null).ConfigureAwait(false);
            }
            else
            {
                _outputHelper.WriteLine($"No results found for {resourceName}");
            }
        }
    }
}
