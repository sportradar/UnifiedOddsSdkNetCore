/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import category cache item properties
    /// </summary>
    [Serializable]
    public class ExportableCategoryCI : ExportableCI
    {
        /// <summary>
        /// Gets the <see cref="string"/> specifying the id of the parent sport
        /// </summary>
        public string SportId { get; set; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> containing the ids of child tournaments
        /// </summary>
        public IEnumerable<string> TournamentIds { get; set; }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; set; }
    }
}