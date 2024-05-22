// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    internal class TournamentCoverageDto
    {
        public bool LiveCoverage { get; }

        internal TournamentCoverageDto(tournamentLiveCoverageInfo tournamentCoverage)
        {
            LiveCoverage = tournamentCoverage.live_coverage.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
