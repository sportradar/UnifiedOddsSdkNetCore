// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable
{
    /// <summary>
    /// Defines a contract for classes implementing cache export/import functionally
    /// </summary>
    internal interface IExportableSdkCache
    {
        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <returns>Collection of <see cref="ExportableBase"/> containing all the items currently in the cache</returns>
        Task<IEnumerable<ExportableBase>> ExportAsync();

        /// <summary>
        /// Imports provided items into the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableBase"/> to be inserted into the cache</param>
        Task ImportAsync(IEnumerable<ExportableBase> items);

        /// <summary>
        /// Returns current cache status
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{K, V}"/> containing all cache item types in the cache and their counts</returns>
        IReadOnlyDictionary<string, int> CacheStatus();
    }
}
