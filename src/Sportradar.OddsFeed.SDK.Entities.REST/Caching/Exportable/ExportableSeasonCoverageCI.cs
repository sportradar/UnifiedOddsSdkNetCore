/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import season coverage cache item properties
    /// </summary>
    [Serializable]
    public class ExportableSeasonCoverageCI
    {
        /// <summary>
        /// A <see cref="string"/> representation of the max coverage level
        /// </summary>
        public string MaxCoverageLevel { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the min coverage level
        /// </summary>
        public string MinCoverageLevel { get; set; }

        /// <summary>
        /// A <see cref="int"/> representation of the max covered
        /// </summary>
        public int? MaxCovered { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the year
        /// </summary>
        public int Played { get; set; }

        /// <summary>
        /// A <see cref="int"/> representation of the scheduled
        /// </summary>
        public int Scheduled { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the season id
        /// </summary>
        public string SeasonId { get; set; }
    }
}
