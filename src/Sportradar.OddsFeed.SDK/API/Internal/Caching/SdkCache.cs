/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Class SdkCache
    /// </summary>
    /// <seealso cref="ISdkCache" />
    internal abstract class SdkCache : ISdkCache
    {
        public string CacheName { get; }

        public IEnumerable<DtoType> RegisteredDtoTypes { get; protected set; }

        protected readonly ILogger ExecutionLog;

        protected readonly ILogger CacheLog;

        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SdkCache"/> class
        /// </summary>
        protected SdkCache(ICacheManager cacheManager)
        {
            Guard.Argument(cacheManager, nameof(cacheManager)).NotNull();

            _cacheManager = cacheManager;

            CacheName = GetType().Name;

            RegisterCache();

            ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(GetType());
            CacheLog = SdkLoggerFactory.GetLoggerForCache(GetType());
        }

        /// <summary>
        /// Registers the cache in <see cref="CacheManager" />
        /// </summary>
        public void RegisterCache()
        {
            SetDtoTypes();
            if (RegisteredDtoTypes == null)
            {
                throw new InvalidOperationException($"{CacheName} cache has no registered dto types");
            }
            _cacheManager.RegisterCache(CacheName, this);
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public abstract void SetDtoTypes();

        /// <summary>
        /// Adds the item to the cache
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be added</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        public async Task<bool> CacheAddDtoAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(item, nameof(item)).NotNull();

            var metricTags = new Dictionary<string, string> { { "cache_name", CacheName }, { "dto_type", dtoType.ToString() } };
            using (new TelemetryTracker(UofSdkTelemetry.CacheDistribution, metricTags))
            {
                var result = await CacheAddDtoItemAsync(id, item, culture, dtoType, requester).ConfigureAwait(false);
                return result;
            }
        }

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        public abstract void CacheDeleteItem(Urn id, CacheItemType cacheItemType);

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public abstract bool CacheHasItem(Urn id, CacheItemType cacheItemType);

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A string representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void CacheDeleteItem(string id, CacheItemType cacheItemType)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();

            try
            {
                var urn = Urn.Parse(id);
                CacheDeleteItem(urn, cacheItemType);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A string representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool CacheHasItem(string id, CacheItemType cacheItemType)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();

            try
            {
                var urn = Urn.Parse(id);
                return CacheHasItem(urn, cacheItemType);
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        /// <summary>
        /// Asynchronously adds the dto item to cache
        /// </summary>
        /// <param name="id">The identifier of the object</param>
        /// <param name="item">The item to be added</param>
        /// <param name="culture">The culture of the item</param>
        /// <param name="dtoType">Type of the dto</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if added, <c>false</c> otherwise</returns>
        protected abstract Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester);

        /// <summary>
        /// Logs the conflict during saving the Dto instance
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="receivedType">Type of the received.</param>
        /// <param name="logger">The logger.</param>
        protected virtual void LogSavingDtoConflict(Urn id, Type expectedType, Type receivedType, ILogger logger = null)
        {
            if (logger == null)
            {
                ExecutionLog.LogWarning("Invalid data for item id={CacheItemId}. Expecting: {ExpectedType}, received: {ReceivedType}", id, expectedType?.Name, receivedType?.Name);
            }
            else
            {
                logger.LogWarning("Invalid data for item id={CacheItemId}. Expecting: {ExpectedType}, received: {ReceivedType}", id, expectedType?.Name, receivedType?.Name);
            }
        }
    }
}
