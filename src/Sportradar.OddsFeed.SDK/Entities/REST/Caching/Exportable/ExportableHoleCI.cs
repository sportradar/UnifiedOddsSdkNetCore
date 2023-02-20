/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import hole item properties
    /// </summary>
    [Serializable]
    public class ExportableHoleCI
    {
        /// <summary>
        /// Gets the number of the hole
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets the par
        /// </summary>
        /// <value>The par</value>
        public int Par { get; set; }
    }
}
