/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Class TournamentCoverage
    /// </summary>
    /// <seealso cref="ITournamentCoverage" />
    internal class TournamentCoverage : ITournamentCoverage
    {
        /// <summary>
        /// Gets a value indicating whether live coverage is available
        /// </summary>
        /// <value><c>true</c> if [live coverage]; otherwise, <c>false</c></value>
        public bool LiveCoverage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentCoverage"/> class
        /// </summary>
        /// <param name="isLiveCoverage">if set to <c>true</c> [is live coverage]</param>
        internal TournamentCoverage(bool isLiveCoverage)
        {
            LiveCoverage = isLiveCoverage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentCoverage"/> class
        /// </summary>
        /// <param name="tournamentCoverageCacheItem">The tournament coverage ci</param>
        internal TournamentCoverage(TournamentCoverageCacheItem tournamentCoverageCacheItem)
        {
            LiveCoverage = tournamentCoverageCacheItem.LiveCoverage;
        }
    }
}
