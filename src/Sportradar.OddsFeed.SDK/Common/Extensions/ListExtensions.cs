/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections;

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
        /// <returns>True is yes, otherwise false</returns>
        public static bool IsNullOrEmpty(this IEnumerable list)
        {
            return list == null || !list.GetEnumerator().MoveNext();
        }
    }
}
