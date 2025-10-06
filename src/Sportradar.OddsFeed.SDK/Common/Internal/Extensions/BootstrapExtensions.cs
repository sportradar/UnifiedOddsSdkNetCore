// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Extensions
{
    internal static class BootstrapExtensions
    {
        public static void AddSdkCacheStore<T>(this IServiceCollection services, string cacheStoreName, TimeSpan? absoluteExpiration = null, TimeSpan? cacheItemSlidingExpiration = null, int expirationVariance = 0)
        {
            var memoryCacheOptions = new MemoryCacheOptions
            {
                TrackStatistics = true,
                TrackLinkedCacheEntries = true,
                CompactionPercentage = 0.2,
                ExpirationScanFrequency = TimeSpan.FromMinutes(10)
            };

            services.AddSingleton<ICacheStore<T>>(serviceProvider =>
                                                  {
                                                      var storeMemoryCache = new MemoryCache(memoryCacheOptions);
                                                      var logger = serviceProvider.GetRequiredService<ILogger<Cache>>();
                                                      return new CacheStore<T>(cacheStoreName, storeMemoryCache, logger, absoluteExpiration, cacheItemSlidingExpiration, expirationVariance);
                                                  });
        }

        public static ICacheStore<T> GetSdkCacheStore<T>(this IServiceProvider serviceProvider, string cacheStoreName)
        {
            return serviceProvider.GetServices<ICacheStore<T>>().First(w => w.StoreName.Equals(cacheStoreName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static void AddSdkTimer(this IServiceCollection services, string timerName, TimeSpan dueTime, TimeSpan period)
        {
            var sdkTimer = new SdkTimer(timerName, dueTime, period);
            services.AddSingleton<ISdkTimer>(sdkTimer);
        }

        public static ISdkTimer GetSdkTimer(this IServiceProvider serviceProvider, string timerName)
        {
            return serviceProvider.GetServices<ISdkTimer>().First(w => w.TimerName.Equals(timerName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static IDataProviderNamed<T> GetDataProviderNamed<T>(this IServiceProvider serviceProvider, string dataProviderName) where T : class
        {
            var services = serviceProvider.GetServices<IDataProviderNamed<T>>().ToList();
            if (services.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"No registered service found for {dataProviderName}");
            }
            return services.First(w => w.DataProviderName.Equals(dataProviderName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static INamedValueCache GetNamedValueCache(this IServiceProvider serviceProvider, string cacheName)
        {
            var services = serviceProvider.GetServices<INamedValueCache>().ToList();
            if (services.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"No registered service found for {cacheName}");
            }
            return services.First(w => w.CacheName.Equals(cacheName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static ILocalizedNamedValueCache GetLocalizedNamedValueCache(this IServiceProvider serviceProvider, string cacheName)
        {
            var services = serviceProvider.GetServices<ILocalizedNamedValueCache>().ToList();
            if (services.IsNullOrEmpty())
            {
                throw new InvalidOperationException($"No registered service found for {cacheName}");
            }
            return services.First(w => w.CacheName.Equals(cacheName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
