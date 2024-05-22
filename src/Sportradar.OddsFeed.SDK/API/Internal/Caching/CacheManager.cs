// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Class CacheManager
    /// </summary>
    /// <seealso cref="ICacheManager" />
    internal class CacheManager : ICacheManager
    {
        private static readonly ILogger ExecLog = SdkLoggerFactory.GetLoggerForExecution(typeof(CacheManager));

        private readonly Dictionary<string, ISdkCache> _caches;

        public long MaxSaveTime { get; private set; }

        public long TotalSaveTime { get; private set; }

        public CacheManager()
        {
            _caches = new Dictionary<string, ISdkCache>();
        }

        /// <summary>
        /// Registers the cache in the CacheManager
        /// </summary>
        /// <param name="name">The name of the instance</param>
        /// <param name="cache">The cache to be registered</param>
        public void RegisterCache(string name, ISdkCache cache)
        {
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
            Guard.Argument(cache, nameof(cache)).NotNull();

            if (cache.RegisteredDtoTypes?.Any() != true)
            {
                throw new InvalidOperationException($"Missing registered dto types in {cache.CacheName}");
            }
            if (_caches.ContainsKey(name))
            {
                ExecLog.LogWarning("Cache with the name={CacheName} already added (removing it)", name);
                _caches.Remove(name);
            }
            ExecLog.LogDebug("Registering cache with the name={CacheName} to the CacheManager", name);
            _caches.Add(name, cache);
        }

        /// <summary>
        /// Adds the item to the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be add</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        public void SaveDto(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            SaveDtoAsync(id, item, culture, dtoType, requester).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds the item to the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be add</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        public async Task SaveDtoAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(item, nameof(item)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (_caches == null || !_caches.Any())
            {
                return;
            }

            var appropriateCaches = _caches.Where(s => s.Value.RegisteredDtoTypes.Contains(dtoType)).ToList();

            if (!appropriateCaches.Any())
            {
                ExecLog.LogDebug("No cache with registered type:{DtoType} and lang:[{Language}] to save data", dtoType, culture.TwoLetterISOLanguageName);
                return;
            }

            var stopWatch = Stopwatch.StartNew();

            var tasks = appropriateCaches.Select(c => c.Value.CacheAddDtoAsync(id, item, culture, dtoType, requester)).ToArray();
            if (tasks.Any())
            {
                try
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    ExecLog.LogError(e, "Error saving dto data for id={ItemId}, lang=[{Language}], type={DtoType}", id, culture.TwoLetterISOLanguageName, dtoType);
                }
            }
            else
            {
                ExecLog.LogDebug("Cannot save data, because there is no cache registered");
            }

            stopWatch.Stop();

            TotalSaveTime += stopWatch.ElapsedMilliseconds;
            if (stopWatch.ElapsedMilliseconds > MaxSaveTime)
            {
                MaxSaveTime = stopWatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Remove the cache item in the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="cacheItemType">Type of the cache item</param>
        /// <param name="sender">The name of the cache or class that is initiating request</param>
        public void RemoveCacheItem(Urn id, CacheItemType cacheItemType, string sender)
        {
            Guard.Argument(id, nameof(id)).NotNull();

            if (_caches?.Any() != true)
            {
                ExecLog.LogWarning("Cannot remove item from cache, because there is no cache registered");
                return;
            }

            var caches = _caches.Where(c => !c.Value.CacheName.Equals(sender));
            foreach (var cache in caches)
            {
                cache.Value.CacheDeleteItem(id, cacheItemType);
            }
        }
    }
}
