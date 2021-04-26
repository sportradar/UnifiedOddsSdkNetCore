/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import sport event conditions cache item properties
    /// </summary>
    [Serializable]
    public class ExportableSportEventConditionsCI
    {
        /// <summary>
        /// A <see cref="string" /> specifying the attendance of the associated sport event
        /// </summary>
        public string Attendance { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the event mode
        /// </summary>
        public string EventMode { get; set; }

        /// <summary>
        /// A <see cref="ExportableRefereeCI" /> specifying the referee
        /// </summary>
        public ExportableRefereeCI Referee { get; set; }

        /// <summary>
        /// A <see cref="ExportableWeatherInfoCI" /> specifying the weather info
        /// </summary>
        public ExportableWeatherInfoCI WeatherInfo { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}" /> specifying the pitchers
        /// </summary>
        public IEnumerable<ExportablePitcherCI> Pitchers { get; set; }
    }
}
