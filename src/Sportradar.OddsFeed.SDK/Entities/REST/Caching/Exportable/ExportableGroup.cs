// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import group cache item properties
    /// </summary>
    [Serializable]
    public class ExportableGroup
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
        /// A list of competitor id Urn as string representation of the competitors ids
        /// </summary>
        public IEnumerable<string> Competitors { get; set; }

        /// <summary>
        /// A dictionary of competitor ids and references representation of the competitor references
        /// </summary>
        public IDictionary<string, Dictionary<string, string>> CompetitorsReferences { get; set; }
    }
}
