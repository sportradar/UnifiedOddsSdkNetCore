/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import group cache item properties
    /// </summary>
    public class ExportableGroupCI
    {
        /// <summary>
        /// A <see cref="string"/> representation of the id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representation of the competitors
        /// </summary>
        public IEnumerable<ExportableCompetitorCI> Competitors { get; set; }
    }
}
