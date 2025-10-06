// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import referee cache item properties
    /// </summary>
    [Serializable]
    public class ExportableDelayedInfo
    {
        /// <summary>
        /// A <see cref="string" /> specifying the id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K,V}"/> containing descriptions in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Descriptions { get; set; }
    }
}
