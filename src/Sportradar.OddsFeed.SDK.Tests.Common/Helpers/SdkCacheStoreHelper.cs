// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

internal static class SdkCacheStoreHelper
{
    public static CacheStore<T> CreateSdkCacheStore<T>(string cacheStoreName, ILoggerFactory loggerFactory)
    {
        var memoryCacheOptions = new MemoryCacheOptions
        {
            TrackStatistics = true,
            TrackLinkedCacheEntries = true,
            CompactionPercentage = 0.2,
            ExpirationScanFrequency = TimeSpan.FromMinutes(10)
        };

        var storeMemoryCache = new MemoryCache(memoryCacheOptions);
        var logger = loggerFactory.CreateLogger<ILogger<Cache>>();

        return new CacheStore<T>(cacheStoreName, storeMemoryCache, logger);
    }
}
