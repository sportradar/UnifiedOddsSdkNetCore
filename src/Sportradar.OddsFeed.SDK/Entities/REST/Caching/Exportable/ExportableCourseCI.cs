/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import course item properties
    /// </summary>
    [Serializable]
    public class ExportableCourseCI
    {
        /// <summary>
        /// The id value
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name value
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of associated holes
        /// </summary>
        public ICollection<ExportableHoleCI> Holes { get; set; }
    }
}
