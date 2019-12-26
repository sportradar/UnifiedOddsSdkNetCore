/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    public class TournamentCoverageDTO
    {
        public  bool LiveCoverage { get; }

        internal TournamentCoverageDTO(tournamentLiveCoverageInfo tournamentCoverage)
        {
            LiveCoverage = tournamentCoverage.live_coverage.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
