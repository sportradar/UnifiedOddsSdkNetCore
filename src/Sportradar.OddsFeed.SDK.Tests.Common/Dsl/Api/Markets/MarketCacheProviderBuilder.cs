// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public class MarketCacheProviderBuilder
{
    private IDataRouterManager _dataRouterManager;
    private IProfileCache _profileCache;
    private ICacheManager _cacheManager;
    private IReadOnlyCollection<CultureInfo> _languages;
    private ILoggerFactory _loggerFactory = new NullLoggerFactory();
    private readonly IMappingValidatorFactory _mappingValidatorFactory = new MappingValidatorFactory();

    public static MarketCacheProviderBuilder Create()
    {
        return new MarketCacheProviderBuilder();
    }

    internal MarketCacheProviderBuilder WithDataRouterManager(IDataRouterManager dataRouterManager)
    {
        _dataRouterManager = dataRouterManager;
        return this;
    }

    public MarketCacheProviderBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    internal MarketCacheProviderBuilder WithProfileCache(IProfileCache profileCache)
    {
        _profileCache = profileCache;
        return this;
    }

    internal MarketCacheProviderBuilder WithLanguages(IReadOnlyCollection<CultureInfo> languages)
    {
        _languages = languages;
        return this;
    }

    internal MarketCacheProviderBuilder WithCacheManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        return this;
    }

    internal IMarketCacheProvider Build()
    {
        if (_dataRouterManager == null)
        {
            throw new InvalidOperationException("DataRouterManager is not set");
        }
        _profileCache ??= new Mock<IProfileCache>().Object;
        if (_languages.IsNullOrEmpty())
        {
            _languages = TestData.Cultures1;
        }

        return new MarketCacheProvider(ConstructInvariantListCache(), ConstructVariantSingleCache(), ConstructVariantListCache(), _loggerFactory.CreateLogger<MarketCacheProvider>());
    }

    private CacheStore<string> GetCacheStore(string cacheName)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions
        {
            TrackStatistics = true,
            TrackLinkedCacheEntries = true,
            CompactionPercentage = 0.2,
            ExpirationScanFrequency = TimeSpan.FromMinutes(10)
        });

        return new CacheStore<string>(cacheName,
                                      memoryCache,
                                      _loggerFactory.CreateLogger(typeof(Cache)),
                                      null,
                                      TestConfiguration.GetConfig().Cache.VariantMarketDescriptionCacheTimeout,
                                      1);
    }

    private IMarketDescriptionsCache ConstructInvariantListCache()
    {
        var timer = new SdkTimer(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache, TimeSpan.FromSeconds(5), TimeSpan.FromHours(6));

        return new InvariantMarketDescriptionCache(GetCacheStore(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache), _dataRouterManager, _mappingValidatorFactory, timer, _languages, _cacheManager, _loggerFactory);
    }

    private IVariantDescriptionsCache ConstructVariantListCache()
    {
        var timer = new SdkTimer(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache, TimeSpan.FromSeconds(5), TimeSpan.FromHours(6));

        return new VariantDescriptionListCache(GetCacheStore(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache), _dataRouterManager, _mappingValidatorFactory, timer, _languages, _cacheManager, _loggerFactory);
    }

    private IMarketDescriptionCache ConstructVariantSingleCache()
    {
        return new VariantMarketDescriptionCache(GetCacheStore(UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache), _dataRouterManager, _mappingValidatorFactory, _cacheManager, _loggerFactory);
    }
}
