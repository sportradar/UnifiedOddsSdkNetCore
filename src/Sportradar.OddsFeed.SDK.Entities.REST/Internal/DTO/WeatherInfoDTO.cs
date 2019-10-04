/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for weather info
    /// </summary>
    internal class WeatherInfoDTO
    {
        internal int? TemperatureCelsius { get; }

        internal string Wind { get; }

        internal string WindAdvantage { get; }

        internal string Pitch { get; }

        internal string WeatherConditions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherInfoDTO"/> class.
        /// </summary>
        /// <param name="weatherInfo">The <see cref="weatherInfo"/> used for creating instance</param>
        internal WeatherInfoDTO(weatherInfo weatherInfo)
        {
            Contract.Requires(weatherInfo != null);

            TemperatureCelsius = weatherInfo.temperature_celsiusSpecified
                ? (int?) weatherInfo.temperature_celsius
                : null;
            Wind = weatherInfo.wind;
            WindAdvantage = weatherInfo.wind_advantage;
            Pitch = weatherInfo.pitch;
            WeatherConditions = weatherInfo.weather_conditions;
        }
    }
}
