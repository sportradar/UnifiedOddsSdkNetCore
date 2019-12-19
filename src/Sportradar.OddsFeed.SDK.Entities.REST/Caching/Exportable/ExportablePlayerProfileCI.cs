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
    /// Class used to export/import player profile cache item properties
    /// </summary>
    [Serializable]
    public class ExportablePlayerProfileCI : ExportableCI
    {
        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> containing nationalities in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Nationalities { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A <see cref="DateTime"/> representing the date of birth
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// A <see cref="int"/> representing the height
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// A <see cref="int"/> representing the weight
        /// </summary>
        public int? Weight { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the abbreviation
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The competitor id this player belongs to
        /// </summary>
        public URN CompetitorId { get; set; }
    }
}
