/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import race driver profile item properties
    /// </summary>
    public class ExportableRaceDriverProfileCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the race driver id
        /// </summary>
        public string RaceDriverId { get; set; }

        /// <summary>
        /// A <see cref="string"/> representing the race team id
        /// </summary>
        public string RaceTeamId { get; set; }

        /// <summary>
        /// A <see cref="ExportableCarCI"/> representing the car
        /// </summary>
        public ExportableCarCI Car { get; set; }
    }
}