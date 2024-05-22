// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable
{
    /// <summary>
    /// Interface used by cache items to export their properties
    /// </summary>
    interface IExportableBase
    {
        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        Task<ExportableBase> ExportAsync();
    }
}
