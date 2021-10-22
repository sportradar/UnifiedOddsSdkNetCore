/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Defines a contract for classes implementing caching functionally
    /// </summary>
    internal interface ISdkCache
    {
        /// <summary>
        /// Gets the registered dto types
        /// </summary>
        /// <value>The registered dto types</value>
        IEnumerable<DtoType> RegisteredDtoTypes { get; }

        /// <summary>
        /// Gets the name of the cache instance
        /// </summary>
        /// <value>The name</value>
        string CacheName { get; }

        /// <summary>
        /// Registers the cache in the <see cref="CacheManager"/>
        /// </summary>
        void RegisterCache();

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        void SetDtoTypes();

        /// <summary>
        /// Adds the item to the cache
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <param name="item">The item to be added</param>
        /// <param name="culture">The culture of the data-transfer-object</param>
        /// <param name="dtoType">Type of the dto item</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns><c>true</c> if is added/updated, <c>false</c> otherwise</returns>
        Task<bool> CacheAddDtoAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester);

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        void CacheDeleteItem(URN id, CacheItemType cacheItemType);

        /// <summary>
        /// Deletes the item from cache
        /// </summary>
        /// <param name="id">A string representing the id of the item in the cache to be deleted</param>
        /// <param name="cacheItemType">A cache item type</param>
        void CacheDeleteItem(string id, CacheItemType cacheItemType);

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="URN"/> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        bool CacheHasItem(URN id, CacheItemType cacheItemType);

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A string representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        bool CacheHasItem(string id, CacheItemType cacheItemType);
    }
}
