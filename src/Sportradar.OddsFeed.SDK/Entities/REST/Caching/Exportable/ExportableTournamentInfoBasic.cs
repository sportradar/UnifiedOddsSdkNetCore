/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import tournament info basic cache item properties
    /// </summary>
    [Serializable]
    public class ExportableTournamentInfoBasic : ExportableBase
    {
        /// <summary>
        /// A <see cref="string"/> representation of the category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// A <see cref="ExportableCurrentSeasonInfo"/> representation of the current season
        /// </summary>
        public ExportableCurrentSeasonInfo CurrentSeason { get; set; }
    }
}
