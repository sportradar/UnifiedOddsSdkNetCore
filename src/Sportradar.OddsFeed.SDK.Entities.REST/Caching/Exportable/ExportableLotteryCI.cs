/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import lottery cache item properties
    /// </summary>
    [Serializable]
    public class ExportableLotteryCI : ExportableSportEventCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the category id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// A <see cref="ExportableBonusInfoCI"/> representing the bonus info
        /// </summary>
        public ExportableBonusInfoCI BonusInfo { get; set; }

        /// <summary>
        /// A <see cref="ExportableDrawInfoCI"/> representing the draw info
        /// </summary>
        public ExportableDrawInfoCI DrawInfo { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the scheduled draws
        /// </summary>
        public IEnumerable<string> ScheduledDraws { get; set; }
    }
}
