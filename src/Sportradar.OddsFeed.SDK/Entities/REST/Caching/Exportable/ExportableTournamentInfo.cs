/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import tournament info cache item properties
    /// </summary>
    [Serializable]
    public class ExportableTournamentInfo : ExportableSportEvent
    {
        /// <summary>
        /// A <see cref="string"/> representing the category id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// A <see cref="ExportableTournamentCoverage"/> representing the coverage
        /// </summary>
        public ExportableTournamentCoverage TournamentCoverage { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the competitors ids
        /// </summary>
        public IEnumerable<string> Competitors { get; set; }

        /// <summary>
        /// A <see cref="ExportableCurrentSeasonInfo"/> representing the season info
        /// </summary>
        public ExportableCurrentSeasonInfo CurrentSeasonInfo { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the groups
        /// </summary>
        public IEnumerable<ExportableGroup> Groups { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the schedule urns
        /// </summary>
        public IEnumerable<string> ScheduleUrns { get; set; }

        /// <summary>
        /// A <see cref="ExportableRound"/> representing the round
        /// </summary>
        public ExportableRound Round { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the year
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// A <see cref="ExportableTournamentInfoBasic"/> representing the basic info
        /// </summary>
        public ExportableTournamentInfoBasic TournamentInfoBasic { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> representing the reference id
        /// </summary>
        public IDictionary<string, string> ReferenceId { get; set; }

        /// <summary>
        /// A <see cref="ExportableSeasonCoverage"/> representing the season coverage
        /// </summary>
        public ExportableSeasonCoverage SeasonCoverage { get; set; }

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
