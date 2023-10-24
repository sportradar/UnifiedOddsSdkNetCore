/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import manager item properties
    /// </summary>
    [Serializable]
    public class ExportableManager : ExportableBase
    {
        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing translated nationality of the item
        /// </summary>
        public IDictionary<CultureInfo, string> Nationality { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the country code of the manager
        /// </summary>
        public string CountryCode { get; set; }
    }
}
