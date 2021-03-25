/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import tournament info cache item properties
    /// </summary>
    [Serializable]
    public class ExportableTournamentInfoCI : ExportableSportEventCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the category id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// A <see cref="ExportableTournamentCoverageCI"/> representing the coverage
        /// </summary>
        public ExportableTournamentCoverageCI TournamentCoverage { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the competitors ids
        /// </summary>
        public IEnumerable<string> Competitors { get; set; }

        /// <summary>
        /// A <see cref="ExportableCurrentSeasonInfoCI"/> representing the season info
        /// </summary>
        public ExportableCurrentSeasonInfoCI CurrentSeasonInfo { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the groups
        /// </summary>
        public IEnumerable<ExportableGroupCI> Groups { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the schedule urns
        /// </summary>
        public IEnumerable<string> ScheduleUrns { get; set; }

        /// <summary>
        /// A <see cref="ExportableRoundCI"/> representing the round
        /// </summary>
        public ExportableRoundCI Round { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the year
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// A <see cref="ExportableTournamentInfoBasicCI"/> representing the basic info
        /// </summary>
        public ExportableTournamentInfoBasicCI TournamentInfoBasic { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> representing the reference id
        /// </summary>
        public IDictionary<string, string> ReferenceId { get; set; }

        /// <summary>
        /// A <see cref="ExportableSeasonCoverageCI"/> representing the season coverage
        /// </summary>
        public ExportableSeasonCoverageCI SeasonCoverage { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the seasons
        /// </summary>
        public IEnumerable<string> Seasons { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the loaded seasons
        /// </summary>
        public IEnumerable<CultureInfo> LoadedSeasons { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the loaded schedules
        /// </summary>
        public IEnumerable<CultureInfo> LoadedSchedules { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> representing the competitors references
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> CompetitorsReferences { get; set; }

        /// <summary>
        /// A <see cref="bool"/> representing the exhibition games
        /// </summary>
        public bool? ExhibitionGames { get; set; }
    }
}
