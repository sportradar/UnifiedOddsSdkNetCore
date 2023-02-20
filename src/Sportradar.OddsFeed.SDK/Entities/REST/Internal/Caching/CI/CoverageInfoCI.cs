/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for coverage info
    /// </summary>
    internal class CoverageInfoCI
    {
        internal string Level { get; }

        internal bool IsLive { get; }

        internal IEnumerable<string> Includes { get; }

        internal CoveredFrom? CoveredFrom { get; }

        internal CoverageInfoCI(CoverageInfoDTO coverageInfo)
        {
            Guard.Argument(coverageInfo, nameof(coverageInfo)).NotNull();

            Level = coverageInfo.Level;
            IsLive = coverageInfo.IsLive;
            Includes = coverageInfo.Includes;
            CoveredFrom = coverageInfo.CoveredFrom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageInfoCI"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableCoverageInfoCI"/> used to create new instance</param>
        internal CoverageInfoCI(ExportableCoverageInfoCI exportable)
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
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableCoverageInfoCI> ExportAsync()
        {
            return Task.FromResult(new ExportableCoverageInfoCI
            {
                Level = Level,
                IsLive = IsLive,
                Includes = Includes,
                CoveredFrom = CoveredFrom
            });
        }
    }
}
