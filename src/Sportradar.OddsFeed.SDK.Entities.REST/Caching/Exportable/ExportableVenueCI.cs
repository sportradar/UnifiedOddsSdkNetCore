/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import venue item properties
    /// </summary>
    public class ExportableVenueCI : ExportableCI
    {
        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing city of the venue in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> CityNames { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing country of the venue in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> CountryNames { get; set; }

        /// <summary>
        /// Gets the capacity of the venue
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets a map coordinates specifying the exact location of the venue
        /// </summary>
        public string Coordinates { get; set; }

        /// <summary>
        /// Gets a country code of the venue
        /// </summary>
        public string CountryCode { get; set; }
    }
}