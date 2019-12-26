/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import tv channel cache item properties
    /// </summary>
    [Serializable]
    public class ExportableTvChannelCI
    {
        /// <summary>
        /// A <see cref="string"/> representation of the name
        /// </summary>
        public string Name;

        /// <summary>
        /// A <see cref="DateTime"/> representation of the start time
        /// </summary>
        public DateTime? StartTime;

        /// <summary>
        /// A <see cref="string"/> representation of the stream url
        /// </summary>
        public string StreamUrl;
    }
}
