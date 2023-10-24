/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import lottery cache item properties
    /// </summary>
    [Serializable]
    public class ExportableLottery : ExportableSportEvent
    {
        /// <summary>
        /// A <see cref="string"/> representing the category id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// A <see cref="ExportableBonusInfo"/> representing the bonus info
        /// </summary>
        public ExportableBonusInfo BonusInfo { get; set; }

        /// <summary>
        /// A <see cref="ExportableDrawInfo"/> representing the draw info
        /// </summary>
        public ExportableDrawInfo DrawInfo { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the scheduled draws
        /// </summary>
        public IEnumerable<string> ScheduledDraws { get; set; }
    }
}
