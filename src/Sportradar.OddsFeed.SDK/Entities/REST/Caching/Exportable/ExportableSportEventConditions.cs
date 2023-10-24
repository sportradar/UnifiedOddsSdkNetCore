/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import sport event conditions cache item properties
    /// </summary>
    [Serializable]
    public class ExportableSportEventConditions
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
        /// A <see cref="ExportableReferee" /> specifying the referee
        /// </summary>
        public ExportableReferee Referee { get; set; }

        /// <summary>
        /// A <see cref="ExportableWeatherInfo" /> specifying the weather info
        /// </summary>
        public ExportableWeatherInfo WeatherInfo { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}" /> specifying the pitchers
        /// </summary>
        public IEnumerable<ExportablePitcher> Pitchers { get; set; }
    }
}
