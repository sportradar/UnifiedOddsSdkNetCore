// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    internal interface ICacheStore<T> : IDisposable
    {
        string StoreName { get; }

        /// <summary>
        /// Adds object value to the cache with key identifier. Also adds key to list of keys. Sets absolute and/or sliding expiration based on CacheStore settings.
        /// </summary>
        /// <param name="key">Key identifier for new cache item</param>
        /// <param name="value">Value of the cache item</param>
        /// <param name="cacheItemPriority">Priority of the cache item</param>
        void Add(T key, object value, CacheItemPriority cacheItemPriority = CacheItemPriority.Normal);

        /// <summary>
        /// Get cache item for specified key if exists
        /// </summary>
        /// <param name="key">The key to search cache item</param>
        /// <returns>Returns the cache item for specified key if exists</returns>
        object Get(T key);

        /// <summary>
        /// Remove cache item from cache if exists
        /// </summary>
        /// <param name="key">The key to remove from cache</param>
        void Remove(T key);

        /// <summary>
        /// Get the list of all the keys
        /// </summary>
        /// <returns>The list of all the keys</returns>
        IReadOnlyCollection<T> GetKeys();

        /// <summary>
        /// Get all the cache items in the cache
        /// </summary>
        /// <returns>All the cache items in the cache</returns>
        IReadOnlyCollection<object> GetValues();

        /// <summary>
        /// Check if the key exists in the cache store
        /// </summary>
        /// <param name="key">The key to be checked</param>
        /// <returns>True if cache item with specified key exists, otherwise false</returns>
        bool Contains(T key);

        /// <summary>
        /// Get the count of stored keys
        /// </summary>
        /// <returns>The count of stored keys</returns>
        int Count();

        /// <summary>
        /// Returns estimated cache size if tracking is enabled and sizes are specified, otherwise it will return store key count
        /// </summary>
        /// <returns>Estimated cache size if tracking is enabled and sizes are specified, otherwise it will return store key count</returns>
        long Size();
    }
}
