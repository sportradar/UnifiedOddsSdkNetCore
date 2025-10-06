// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import weather info cache item properties
    /// </summary>
    [Serializable]
    public class ExportableWeatherInfo
    {
        /// <summary>
        /// A <see cref="int" /> specifying the temperature
        /// </summary>
        public int? TemperatureCelsius { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the wind
        /// </summary>
        public string Wind { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the wind advantage
        /// </summary>
        public string WindAdvantage { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the pitch
        /// </summary>
        public string Pitch { get; set; }

        /// <summary>
        /// A <see cref="string" /> specifying the weather conditions
        /// </summary>
        public string WeatherConditions { get; set; }
    }
}
