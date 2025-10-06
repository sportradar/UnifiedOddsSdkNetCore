// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for weather info
    /// </summary>
    internal class WeatherInfoDto
    {
        internal int? TemperatureCelsius { get; }

        internal string Wind { get; }

        internal string WindAdvantage { get; }

        internal string Pitch { get; }

        internal string WeatherConditions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherInfoDto"/> class.
        /// </summary>
        /// <param name="weatherInfo">The <see cref="weatherInfo"/> used for creating instance</param>
        internal WeatherInfoDto(weatherInfo weatherInfo)
        {
            Guard.Argument(weatherInfo, nameof(weatherInfo)).NotNull();

            TemperatureCelsius = weatherInfo.temperature_celsiusSpecified
                                     ? (int?)weatherInfo.temperature_celsius
                                     : null;
            Wind = weatherInfo.wind;
            WindAdvantage = weatherInfo.wind_advantage;
            Pitch = weatherInfo.pitch;
            WeatherConditions = weatherInfo.weather_conditions;
        }
    }
}
