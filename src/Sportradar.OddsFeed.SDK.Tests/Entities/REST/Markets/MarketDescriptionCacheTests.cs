// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

public class MarketDescriptionCacheTests
{
    private readonly ICacheStore<string> _variantMemoryCache;
    private readonly ICacheStore<string> _invariantMemoryCache;
    private readonly IMarketDescriptionCache _variantMdCache;
    private readonly IMarketDescriptionsCache _inVariantMdCache;
    private readonly IMappingValidatorFactory _mappingValidatorFactory;
    private readonly TestDataRouterManager _dataRouterManager;

    public MarketDescriptionCacheTests(ITestOutputHelper outputHelper)
    {
        var loggerFactory = new XunitLoggerFactory(outputHelper);
        var testCacheStoreManager = new TestCacheStoreManager();
        _variantMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache);
        _invariantMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache);

        var cacheManager = new CacheManager();
        _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

        _mappingValidatorFactory = new MappingValidatorFactory();

        var timer = new TestTimer(true);
        _variantMdCache = new VariantMarketDescriptionCache(_variantMemoryCache, _dataRouterManager, _mappingValidatorFactory, cacheManager, loggerFactory);
        _inVariantMdCache = new InvariantMarketDescriptionCache(_invariantMemoryCache, _dataRouterManager, _mappingValidatorFactory, timer, TestData.Cultures, cacheManager, loggerFactory);
    }

    [Fact]
    public async Task VariantMarketDescriptionCacheIsCaching()
    {
        const string callType = "GetVariantMarketDescriptionAsync";
        Assert.Equal(0, _variantMemoryCache.Count());
        Assert.Equal(0, _dataRouterManager.GetCallCount(callType));

        var market = await _variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", TestData.Cultures);
        Assert.NotNull(market);
        Assert.Equal(1, _variantMemoryCache.Count());
        Assert.Equal(3, _dataRouterManager.GetCallCount(callType));

        market = await _variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", TestData.Cultures);
        Assert.NotNull(market);
        Assert.Equal(1, _variantMemoryCache.Count());
        Assert.Equal(3, _dataRouterManager.GetCallCount(callType));
    }

    [Fact]
    public async Task InVariantMarketDescriptionCacheIsCaching()
    {
        const string callType = "GetMarketDescriptionsAsync";
        var market = await _inVariantMdCache.GetMarketDescriptionAsync(178, "lcoo:markettext:1", TestData.Cultures);
        Assert.NotNull(market);
        Assert.Equal(178, market.Id);
        Assert.Equal(TestData.InvariantListCacheCount, _invariantMemoryCache.Count());
        Assert.Equal(3, _dataRouterManager.GetCallCount(callType));

        market = await _inVariantMdCache.GetMarketDescriptionAsync(178, "lcoo:markettext:1", TestData.Cultures);
        Assert.NotNull(market);
        Assert.Equal(TestData.InvariantListCacheCount, _invariantMemoryCache.Count());
        Assert.Equal(3, _dataRouterManager.GetCallCount(callType));
    }

    [Fact]
    public void InVariantMarketDescriptionCacheIsAutoCaching()
    {
        const string callType = "GetMarketDescriptionsAsync";
        //The data is automatically fetched when the cache is constructed and timer is started in the cache constructor
        Assert.Equal(TestData.InvariantListCacheCount, _invariantMemoryCache.Count());
        Assert.Equal(3, _dataRouterManager.GetCallCount(callType));
    }
}
