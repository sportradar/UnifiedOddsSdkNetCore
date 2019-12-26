/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Defines a contract for classes implementing manager for caches
    /// </summary>
    internal interface ICacheManager
    {
        /// <summary>
        /// Registers the cache in the CacheManager
        /// </summary>
        /// <param name="name">The name of the instance</param>
        /// <param name="cache">The cache to be registered</param>
        void RegisterCache(string name, ISdkCache cache);

        /// <summary>
        /// Adds the item to the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be add</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        void SaveDto(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester);

        /// <summary>
        /// Adds the item to the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be add</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        Task SaveDtoAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester);

        /// <summary>
        /// Remove the cache item in the all registered caches
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="cacheItemType">Type of the cache item</param>
        /// <param name="sender">The name of the cache or class that is initiating request</param>
        void RemoveCacheItem(URN id, CacheItemType cacheItemType, string sender);
    }
}