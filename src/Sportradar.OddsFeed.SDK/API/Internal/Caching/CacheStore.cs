// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    internal class CacheStore<T> : ICacheStore<T>
    {
        private readonly ILogger _logCache;
        private readonly HashSet<T> _cacheStoreKeys;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _absoluteExpiration;
        private readonly TimeSpan _slidingExpiration;
        private readonly int _slidingExpirationVariance;
        private readonly object _storeKeysLock = new object();

        public string StoreName { get; }

        public CacheStore(string cacheStoreName, IMemoryCache memoryCache, ILogger logger, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, int slidingExpirationVariance = 0)
        {
            if (slidingExpiration != null && slidingExpiration < TimeSpan.Zero)
            {
                throw new ArgumentException("Not valid expiration exception. Must be 0 or greater.", nameof(slidingExpiration));
            }
            if (slidingExpirationVariance < 0)
            {
                throw new ArgumentException("Expiration variance must be 0 or greater", nameof(slidingExpirationVariance));
            }

            _cacheStoreKeys = new HashSet<T>();
            StoreName = cacheStoreName;
            _logCache = logger;
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _absoluteExpiration = absoluteExpiration ?? TimeSpan.Zero;
            _slidingExpiration = slidingExpiration ?? TimeSpan.Zero;
            _slidingExpirationVariance = slidingExpirationVariance;
        }

        /// <summary>
        /// Adds object value to the cache with key identifier. Also adds key to list of keys. Sets absolute and/or sliding expiration based on CacheStore settings.
        /// </summary>
        /// <param name="key">Key identifier for new cache item</param>
        /// <param name="value">Value of the cache item</param>
        /// <param name="cacheItemPriority">Priority of the cache item</param>
        public void Add(T key, object value, CacheItemPriority cacheItemPriority = CacheItemPriority.Normal)
        {
            if (Equals(key, default(T)) || value == null)
            {
                return;
            }

            var memoryCacheEntryOptions = GetMemoryCacheEntryOptions(SdkInfo.TryGetObjectSize(value), cacheItemPriority);

            lock (_storeKeysLock)
            {
                _memoryCache.Set(key, value, memoryCacheEntryOptions);
                if (_cacheStoreKeys.Contains(key))
                {
                    return;
                }

                _cacheStoreKeys.Add(key);
                if (!_cacheStoreKeys.Contains(key))
                {
                    // ignore
                }
            }
        }

        public object Get(T key)
        {
            if (Equals(key, default(T)))
            {
                return null;
            }

            return _memoryCache.TryGetValue(key, out var cacheItem) ? cacheItem : null;
        }

        public void Remove(T key)
        {
            lock (_storeKeysLock)
            {
                _cacheStoreKeys.Remove(key);
            }
            _memoryCache.Remove(key);
        }

        public IReadOnlyCollection<T> GetKeys()
        {
            lock (_storeKeysLock)
            {
                return _cacheStoreKeys.ToList();
            }
        }

        public IReadOnlyCollection<object> GetValues()
        {
            List<T> keys;
            lock (_storeKeysLock)
            {
                keys = _cacheStoreKeys.ToList();
            }

            var resultValues = new List<object>();
            foreach (var key in keys)
            {
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    resultValues.Add(value);
                }
            }
            return resultValues;
        }

        public bool Contains(T key)
        {
            lock (_storeKeysLock)
            {
                return _cacheStoreKeys.Contains(key);
            }
        }

        public int Count()
        {
            lock (_storeKeysLock)
            {
                return _cacheStoreKeys.Count;
            }
        }

        public long Size()
        {
            if (!(_memoryCache is MemoryCache memoryCache))
            {
                return Count();
            }

            var statistics = memoryCache.GetCurrentStatistics();

            return statistics?.CurrentEstimatedSize ?? Count();
        }

        [SuppressMessage("CodeQuality", "IDE0058:Expression value is never used", Justification = "Allowed here")]
        private MemoryCacheEntryOptions GetMemoryCacheEntryOptions(long? cacheItemSize, CacheItemPriority cacheItemPriority = CacheItemPriority.Normal)
        {
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions();
            if (_absoluteExpiration > TimeSpan.Zero)
            {
                memoryCacheEntryOptions.SetAbsoluteExpiration(_absoluteExpiration);
                //memoryCacheEntryOptions.AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(_absoluteExpiration.Value).Token)); // enabling this would expire item regardless of Get
            }
            if (_slidingExpiration > TimeSpan.Zero)
            {
                var expiration = _slidingExpirationVariance == 0 ? _slidingExpiration : SdkInfo.AddVariableNumber(_slidingExpiration, _slidingExpirationVariance);
                memoryCacheEntryOptions.SetSlidingExpiration(expiration);
                //memoryCacheEntryOptions.AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(expiration).Token)); // enabling this would expire item regardless of Get
            }
            memoryCacheEntryOptions.Size = cacheItemSize;
            memoryCacheEntryOptions.SetPriority(cacheItemPriority);
            memoryCacheEntryOptions.RegisterPostEvictionCallback(PostEvictionDelegate, this);

            return memoryCacheEntryOptions;
        }

        private void PostEvictionDelegate(object key, object value, EvictionReason reason, object state)
        {
            if (key == null || reason == EvictionReason.Replaced)
            {
                return;
            }

            using (new TelemetryTracker(UofSdkTelemetry.CacheStoreEviction, "cache_name", StoreName))
            {
                LogCacheItemPostEviction(_logCache, StoreName, key.ToString(), reason, null);
                lock (_storeKeysLock)
                {
                    _cacheStoreKeys.Remove((T)key);
                }
            }
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
            GC.SuppressFinalize(this);
        }

        private static readonly Action<ILogger, string, string, EvictionReason, Exception> LogCacheItemPostEviction =
            LoggerMessage.Define<string, string, EvictionReason>(LogLevel.Debug,
                                                                 new EventId(1, nameof(CacheStore<T>)),
                                                                 "{CacheName}: evicted cache item {CacheItem} with reason: {EvictionReason}");
    }
}
