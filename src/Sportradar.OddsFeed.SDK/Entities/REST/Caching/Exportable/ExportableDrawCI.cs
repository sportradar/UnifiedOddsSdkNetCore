/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import draw cache item properties
    /// </summary>
    [Serializable]
    public class ExportableDrawCI : ExportableSportEventCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the lottery id
        /// </summary>
        public string LotteryId { get; set; }

        /// <summary>
        /// A <see cref="DrawStatus"/> representing the draw status
        /// </summary>
        public DrawStatus DrawStatus { get; set; }

        /// <summary>
        /// A <see cref="bool"/> indicating if the results are chronological
        /// </summary>
        public bool ResultsChronological { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the results
        /// </summary>
        public IEnumerable<ExportableDrawResultCI> Results { get; set; }

        /// <summary>
        /// A <see cref="int"/> representing the display id
        /// </summary>
        public int? DisplayId { get; set; }
    }
}
