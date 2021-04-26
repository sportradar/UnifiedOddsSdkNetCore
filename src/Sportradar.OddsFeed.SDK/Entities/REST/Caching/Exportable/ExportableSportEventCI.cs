/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import sport event cache item properties
    /// </summary>
    [Serializable]
    public class ExportableSportEventCI : ExportableCI
    {
        /// <summary>
        /// Gets the <see cref="string"/> specifying the id of the parent sport
        /// </summary>
        public string SportId { get; set; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the start time
        /// </summary>
        public DateTime? Scheduled { get; set; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the end time
        /// </summary>
        public DateTime? ScheduledEnd { get; set; }

        /// <summary>
        /// Gets the <see cref="bool"/> indicating if the start time is to be determined
        /// </summary>
        public bool? StartTimeTbd { get; set; }

        /// <summary>
        /// Gets the <see cref="string"/> specifying the replacement sport event id
        /// </summary>
        public string ReplacedBy { get; set; }

        /// <summary>
        /// Gets the <see cref="List{T}"/> specifying the loaded fixtures
        /// </summary>
        public List<CultureInfo> LoadedFixtures { get; set; }

        /// <summary>
        /// Gets the <see cref="List{T}"/> specifying the loaded summaries
        /// </summary>
        public List<CultureInfo> LoadedSummaries { get; set; }
    }
}