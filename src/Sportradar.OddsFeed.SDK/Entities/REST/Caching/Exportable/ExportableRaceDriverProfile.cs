// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import race driver profile item properties
    /// </summary>
    [Serializable]
    public class ExportableRaceDriverProfile
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
        /// A <see cref="ExportableCar"/> representing the car
        /// </summary>
        public ExportableCar Car { get; set; }
    }
}
