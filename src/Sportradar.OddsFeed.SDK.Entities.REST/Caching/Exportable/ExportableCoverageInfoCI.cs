/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import coverage info cache item properties
    /// </summary>
    [Serializable]
    public class ExportableCoverageInfoCI
    {
        /// <summary>
        /// A <see cref="string"/> representation of the level
        /// </summary>
        public string Level;

        /// <summary>
        /// A <see cref="bool"/> indicating if the coverage is live
        /// </summary>
        public bool IsLive;

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the includes
        /// </summary>
        public IEnumerable<string> Includes;

        /// <summary>
        /// A <see cref="CoveredFrom"/> representation of the covered from
        /// </summary>
        public CoveredFrom? CoveredFrom;
    }
}
