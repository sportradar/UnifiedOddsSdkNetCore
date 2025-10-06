// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about weather
    /// </summary>
    internal class WeatherInfoCacheItem
    {
        /// <summary>
        /// Gets the temperature in degrees celsius or a null reference if the temperature is not known
        /// </summary>
        internal int? TemperatureCelsius { get; }

        internal string Wind { get; }

        internal string WindAdvantage { get; }

        internal string Pitch { get; }

        internal string WeatherConditions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherInfoCacheItem"/> class.
        /// </summary>
        /// <param name="dto">The <see cref="WeatherInfoDto"/> used to create new instance</param>
        internal WeatherInfoCacheItem(WeatherInfoDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            TemperatureCelsius = dto.TemperatureCelsius;
            Wind = dto.Wind;
            WindAdvantage = dto.WindAdvantage;
            Pitch = dto.Pitch;
            WeatherConditions = dto.WeatherConditions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherInfoCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableWeatherInfo"/> used to create new instance</param>
        internal WeatherInfoCacheItem(ExportableWeatherInfo exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            TemperatureCelsius = exportable.TemperatureCelsius;
            Wind = exportable.Wind;
            WindAdvantage = exportable.WindAdvantage;
            Pitch = exportable.Pitch;
            WeatherConditions = exportable.WeatherConditions;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableWeatherInfo> ExportAsync()
        {
            return Task.FromResult(new ExportableWeatherInfo
            {
                TemperatureCelsius = TemperatureCelsius,
                Wind = Wind,
                WindAdvantage = WindAdvantage,
                Pitch = Pitch,
                WeatherConditions = WeatherConditions
            });
        }
    }
}
