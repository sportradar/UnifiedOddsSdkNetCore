/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import scheduled start time change cache item properties
    /// </summary>
    public class ExportableScheduledStartTimeChangeCI
    {
        /// <summary>
        /// A <see cref="DateTime"/> representation of the old time
        /// </summary>
        public DateTime OldTime { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/> representation of the new time
        /// </summary>
        public DateTime NewTime { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/> representation of the changed at
        /// </summary>
        public DateTime ChangedAt { get; set; }

    }
}
