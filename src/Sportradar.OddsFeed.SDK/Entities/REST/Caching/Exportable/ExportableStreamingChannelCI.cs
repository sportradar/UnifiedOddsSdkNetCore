/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import product info cache item properties
    /// </summary>
    [Serializable]
    public class ExportableStreamingChannelCI
    {
        /// <summary>
        /// A <see cref="int"/> representing the id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the name
        /// </summary>
        public string Name { get; set; }
    }
}
