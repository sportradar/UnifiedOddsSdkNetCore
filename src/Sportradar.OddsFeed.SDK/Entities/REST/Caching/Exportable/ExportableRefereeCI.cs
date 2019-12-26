/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import referee cache item properties
    /// </summary>
    [Serializable]
    public class ExportableRefereeCI
    {
        /// <summary>
        /// A <see cref="string"/> representing id of the related entity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing referee nationality in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Nationality { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the name
        /// </summary>
        public string Name { get; set; }
    }
}
