// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import course item properties
    /// </summary>
    [Serializable]
    public class ExportableCourse
    {
        /// <summary>
        /// The id value
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing translated name of the item
        /// </summary>
        public IDictionary<CultureInfo, string> Names { get; set; }

        /// <summary>
        /// List of associated holes
        /// </summary>
        public ICollection<ExportableHole> Holes { get; set; }
    }
}
