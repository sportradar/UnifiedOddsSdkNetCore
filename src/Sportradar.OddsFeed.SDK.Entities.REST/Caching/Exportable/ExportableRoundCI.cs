/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import round cache item properties
    /// </summary>
    public class ExportableRoundCI
    {
        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> representation of the round names in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Names { get; set; }

        /// <summary>
        /// A <see cref="IDictionary{K, V}"/> representation of the phase or group long name in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> PhaseOrGroupLongName { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the group id
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the other match id
        /// </summary>
        public string OtherMatchId { get; set; }

        /// <summary>
        /// A <see cref="int"/> representation of the number
        /// </summary>
        public int? Number { get; set; }

        /// <summary>
        /// A <see cref="int"/> representation of the cup round matches
        /// </summary>
        public int? CupRoundMatches { get; set; }

        /// <summary>
        /// A <see cref="int"/> representation of the cup round match number
        /// </summary>
        public int? CupRoundMatchNumber { get; set; }

        /// <summary>
        /// A <see cref="int"/> representation of the betradar id
        /// </summary>
        public int? BetradarId { get; set; }

        /// <summary>
        /// A <see cref="string"/> representation of the phase
        /// </summary>
        public string Phase { get; set; }
    }
}
