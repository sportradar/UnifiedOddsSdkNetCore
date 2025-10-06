// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import division cache item properties
    /// </summary>
    [Serializable]
    public class ExportableDivision
    {
        /// <summary>
        /// The id of the division
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The name of the division
        /// </summary>
        public string Name { get; set; }
    }
}
