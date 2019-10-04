/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable
{
    /// <summary>
    /// Defines a contract for classes implementing cache export/import functionally
    /// </summary>
    interface IExportableSdkCache
    {
        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableCI"/> containing all the items currently in the cache</returns>
        Task<IEnumerable<ExportableCI>> ExportAsync();

        /// <summary>
        /// Imports provided items into the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableCI"/> to be inserted into the cache</param>
        Task ImportAsync(IEnumerable<ExportableCI> items);

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{K, V}"/> containing all cache item types in the cache and their counts</returns>
        IReadOnlyDictionary<string, int> CacheStatus();
    }
}