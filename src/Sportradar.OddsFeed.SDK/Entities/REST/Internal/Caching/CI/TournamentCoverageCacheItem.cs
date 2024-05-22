// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for TournamentCoverage
    /// </summary>
    internal class TournamentCoverageCacheItem
    {
        /// <summary>
        /// Gets a value indicating whether [live coverage].
        /// </summary>
        /// <value><c>true</c> if [live coverage]; otherwise, <c>false</c>.</value>
        public bool LiveCoverage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentCoverageCacheItem"/> class.
        /// </summary>
        /// <param name="tournamentCoverage">The tournament coverage.</param>
        internal TournamentCoverageCacheItem(TournamentCoverageDto tournamentCoverage)
        {
            Guard.Argument(tournamentCoverage, nameof(tournamentCoverage)).NotNull();

            LiveCoverage = tournamentCoverage.LiveCoverage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentCoverageCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">The tournament coverage.</param>
        internal TournamentCoverageCacheItem(ExportableTournamentCoverage exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            LiveCoverage = exportable.LiveCoverage;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableTournamentCoverage> ExportAsync()
        {
            return Task.FromResult(new ExportableTournamentCoverage
            {
                LiveCoverage = LiveCoverage
            });
        }
    }
}
