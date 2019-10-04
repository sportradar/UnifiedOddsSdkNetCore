/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    public class CoverageInfoDTO
    {
        internal string Level { get; }

        internal bool IsLive { get; }

        internal IEnumerable<string> Includes { get; }

        internal CoveredFrom? CoveredFrom { get; }

        internal CoverageInfoDTO(coverageInfo coverageInfo)
        {
            Contract.Requires(coverageInfo != null);
            Contract.Requires(!string.IsNullOrEmpty(coverageInfo.level));

            Level = coverageInfo.level;
            IsLive = coverageInfo.live_coverage;
            Includes = coverageInfo.coverage != null && coverageInfo.coverage.Any()
                ? new ReadOnlyCollection<string>(coverageInfo.coverage.Select(c => c.includes).ToList())
                : null;
            CoveredFrom? coveredFrom;
            RestMapperHelper.TryGetCoveredFrom(coverageInfo.covered_from, out coveredFrom);
            CoveredFrom = coveredFrom;
        }
    }
}