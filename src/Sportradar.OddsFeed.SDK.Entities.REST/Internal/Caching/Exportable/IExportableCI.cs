/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable
{
    /// <summary>
    /// Interface used by cache items to export their properties
    /// </summary>
    interface IExportableCI
    {
        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        Task<ExportableCI> ExportAsync();
    }
}
