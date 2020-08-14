/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import season cache item properties
    /// </summary>
    [Serializable]
    public class ExportableSeasonCI
    {
        /// <summary>
        /// Gets a <see cref="URN"/> representing the ID of the represented sport entity
        /// </summary>
        /// <value>The identifier</value>
        public URN Id { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing round names in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Names { get; set; }

        /// <summary>
        /// Gets the start date of the season represented by the current instance
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets the end date of the season represented by the current instance
        /// </summary>
        /// <value>The end time.</value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Gets the associated tournament identifier.
        /// </summary>
        /// <value>The associated tournament identifier.</value>
        public URN TournamentId { get; set; }
    }
}
