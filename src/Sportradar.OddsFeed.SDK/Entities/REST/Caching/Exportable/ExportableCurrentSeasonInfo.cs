/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import current season info cache item properties
    /// </summary>
    [Serializable]
    public class ExportableCurrentSeasonInfo : ExportableBase
    {
        /// <summary>
        /// A <see cref="string"/> representation of the year
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/> representation of the start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/> representation of the end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// A <see cref="ExportableSeasonCoverage"/> representation of the coverage
        /// </summary>
        public ExportableSeasonCoverage SeasonCoverage { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the groups
        /// </summary>
        public IEnumerable<ExportableGroup> Groups { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the current round
        /// </summary>
        public ExportableRound CurrentRound { get; set; }

        /// <summary>
        /// A list representation of the competitors ids
        /// </summary>
        public IEnumerable<string> Competitors { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the schedule
        /// </summary>
        public IEnumerable<string> Schedule { get; set; }
    }
}
