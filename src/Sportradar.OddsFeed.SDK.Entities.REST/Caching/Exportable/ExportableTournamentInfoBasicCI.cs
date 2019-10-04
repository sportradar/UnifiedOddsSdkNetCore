/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import tournament info basic cache item properties
    /// </summary>
    public class ExportableTournamentInfoBasicCI : ExportableCI
    {
        /// <summary>
        /// A <see cref="string"/> representation of the category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// A <see cref="ExportableCurrentSeasonInfoCI"/> representation of the current season
        /// </summary>
        public ExportableCurrentSeasonInfoCI CurrentSeason { get; set; }
    }
}
