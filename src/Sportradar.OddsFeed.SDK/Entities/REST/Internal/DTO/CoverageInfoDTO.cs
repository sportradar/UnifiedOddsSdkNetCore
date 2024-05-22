// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    internal class CoverageInfoDto
    {
        internal string Level { get; }

        internal bool IsLive { get; }

        internal IEnumerable<string> Includes { get; }

        internal CoveredFrom? CoveredFrom { get; }

        internal CoverageInfoDto(coverageInfo coverageInfo)
        {
            Guard.Argument(coverageInfo, nameof(coverageInfo)).NotNull();
            Guard.Argument(coverageInfo.level, nameof(coverageInfo.level)).NotNull().NotEmpty();

            Level = coverageInfo.level;
            IsLive = coverageInfo.live_coverage;
            Includes = coverageInfo.coverage != null && coverageInfo.coverage.Any()
                ? new ReadOnlyCollection<string>(coverageInfo.coverage.Select(c => c.includes).ToList())
                : null;
            RestMapperHelper.TryGetCoveredFrom(coverageInfo.covered_from, out var coveredFrom);
            CoveredFrom = coveredFrom;
        }
    }
}
