/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Dawn;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for TournamentCoverage
    /// </summary>
    public class TournamentCoverageCI
    {
        /// <summary>
        /// Gets a value indicating whether [live coverage].
        /// </summary>
        /// <value><c>true</c> if [live coverage]; otherwise, <c>false</c>.</value>
        public bool LiveCoverage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentCoverageCI"/> class.
        /// </summary>
        /// <param name="tournamentCoverage">The tournament coverage.</param>
        internal TournamentCoverageCI(TournamentCoverageDTO tournamentCoverage)
        {
            Guard.Argument(tournamentCoverage).NotNull();

            LiveCoverage = tournamentCoverage.LiveCoverage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentCoverageCI"/> class.
        /// </summary>
        /// <param name="exportable">The tournament coverage.</param>
        internal TournamentCoverageCI(ExportableTournamentCoverageCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            LiveCoverage = exportable.LiveCoverage;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableTournamentCoverageCI> ExportAsync()
        {
            return Task.FromResult(new ExportableTournamentCoverageCI
            {
                LiveCoverage = LiveCoverage
            });
        }
    }
}
