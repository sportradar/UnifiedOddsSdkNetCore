// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for coverage info
    /// </summary>
    internal class CoverageInfoCacheItem
    {
        internal string Level { get; }

        internal bool IsLive { get; }

        internal IEnumerable<string> Includes { get; }

        internal CoveredFrom? CoveredFrom { get; }

        internal CoverageInfoCacheItem(CoverageInfoDto coverageInfo)
        {
            Guard.Argument(coverageInfo, nameof(coverageInfo)).NotNull();

            Level = coverageInfo.Level;
            IsLive = coverageInfo.IsLive;
            Includes = coverageInfo.Includes;
            CoveredFrom = coverageInfo.CoveredFrom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageInfoCacheItem"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableCoverageInfo"/> used to create new instance</param>
        internal CoverageInfoCacheItem(ExportableCoverageInfo exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Level = exportable.Level;
            IsLive = exportable.IsLive;
            Includes = exportable.Includes;
            CoveredFrom = exportable.CoveredFrom;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableCoverageInfo> ExportAsync()
        {
            return Task.FromResult(new ExportableCoverageInfo
            {
                Level = Level,
                IsLive = IsLive,
                Includes = Includes,
                CoveredFrom = CoveredFrom
            });
        }
    }
}
