/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import event timeline cache item properties
    /// </summary>
    [Serializable]
    public class ExportableEventTimelineCI
    {
        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the timeline
        /// </summary>
        public IEnumerable<ExportableTimelineEventCI> Timeline { get; set; }

        /// <summary>
        /// A <see cref="bool"/> indicating if the time line is finished
        /// </summary>
        public bool IsFinalized { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the fetched cultures
        /// </summary>
        public IEnumerable<CultureInfo> FetchedCultures { get; set; }
    }
}
