// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Common.Extensions
{
    /// <summary>
    /// Class defining extension methods for IEnumerable
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Check if IEnumerable is null or empty
        /// </summary>
        /// <param name="list">The list to be checked for null or empty</param>
        /// <returns>True is yes, otherwise false</returns>
        public static bool IsNullOrEmpty(this IEnumerable list)
        {
            return list == null || !list.GetEnumerator().MoveNext();
        }

        /// <summary>
        /// Add new item to collection only if not present already
        /// </summary>
        /// <typeparam name="T">The type saved in the collection</typeparam>
        /// <param name="collection">The original collection</param>
        /// <param name="item">The item to be added to the collection</param>
        public static void AddUnique<T>(this ICollection<T> collection, T item)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection), "Collection is missing");
            }

            if (Equals(item, default(T)))
            {
                return;
            }

            if (collection.Contains(item))
            {
                return;
            }

            collection.Add(item);
        }
    }
}
