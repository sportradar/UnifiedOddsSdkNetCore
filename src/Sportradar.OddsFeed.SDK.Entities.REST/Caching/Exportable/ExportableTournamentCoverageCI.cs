/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import tournament coverage cache item properties
    /// </summary>
    public class ExportableTournamentCoverageCI
    {
        /// <summary>
        /// A <see cref="bool"/> indicating if the tournament has live coverage
        /// </summary>
        public bool LiveCoverage { get; set; }
    }
}
