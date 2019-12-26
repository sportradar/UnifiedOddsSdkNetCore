/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import fixture cache item properties
    /// </summary>
    [Serializable]
    public class ExportableFixtureCI
    {
        /// <summary>
        /// A <see cref="DateTime"/> representation of the start time
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/> representation of the next live time
        /// </summary>
        public DateTime? NextLiveTime { get; set; }

        /// <summary>
        /// A <see cref="bool"/> indicating if the start time is confirmed
        /// </summary>
        public bool? StartTimeConfirmed { get; set; }

        /// <summary>
        /// A <see cref="bool"/> indicating if the start time is TBD
        /// </summary>
        public bool? StartTimeTBD { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the replaced by
        /// </summary>
        public string ReplacedBy { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> representation of the extra info
        /// </summary>
        public IDictionary<string, string> ExtraInfo { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the tv channels
        /// </summary>
        public IEnumerable<ExportableTvChannelCI> TvChannels { get; set; }

        /// <summary>
        /// A <see cref="ExportableCoverageInfoCI"/> representation of the coverage info
        /// </summary>
        public ExportableCoverageInfoCI CoverageInfo { get; set; }

        /// <summary>
        /// A <see cref="ExportableProductInfoCI"/> representation of the product info
        /// </summary>
        public ExportableProductInfoCI ProductInfo { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> representation of the references
        /// </summary>
        public IDictionary<string, string> References { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the scheduled start time changes
        /// </summary>
        public IEnumerable<ExportableScheduledStartTimeChangeCI> ScheduledStartTimeChanges { get; set; }
    }
}
