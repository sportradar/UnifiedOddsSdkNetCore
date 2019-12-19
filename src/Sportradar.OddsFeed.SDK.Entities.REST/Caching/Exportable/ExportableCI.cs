/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Abstract class used to export/import cache item properties
    /// </summary>
    [Serializable]
    public class ExportableCI
    {
        /// <summary>
        /// A <see cref="string"/> representing id of the related entity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{TKey,TValue}"/> containing translated name of the item
        /// </summary>
        public IDictionary<CultureInfo, string> Name { get; set; }
    }
}
