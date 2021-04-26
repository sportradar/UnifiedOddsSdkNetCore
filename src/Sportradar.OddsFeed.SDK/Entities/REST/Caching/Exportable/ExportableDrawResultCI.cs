/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import draw result cache item properties
    /// </summary>
    [Serializable]
    public class ExportableDrawResultCI
    {
        /// <summary>
        /// A <see cref="int"/> representing the value
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing translated names
        /// </summary>
        public IDictionary<CultureInfo, string> Names { get; set; }
    }
}
