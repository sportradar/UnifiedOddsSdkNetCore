/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about weather
    /// </summary>
    public class WeatherInfoCI
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
        /// Initializes a new instance of the <see cref="WeatherInfoCI"/> class.
        /// </summary>
        /// <param name="dto">The <see cref="WeatherInfoDTO"/> used to create new instance</param>
        internal WeatherInfoCI(WeatherInfoDTO dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            TemperatureCelsius = dto.TemperatureCelsius;
            Wind = dto.Wind;
            WindAdvantage = dto.WindAdvantage;
            Pitch = dto.Pitch;
            WeatherConditions = dto.WeatherConditions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherInfoCI"/> class.
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableWeatherInfoCI"/> used to create new instance</param>
        internal WeatherInfoCI(ExportableWeatherInfoCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            TemperatureCelsius = exportable.TemperatureCelsius;
            Wind = exportable.Wind;
            WindAdvantage = exportable.WindAdvantage;
            Pitch = exportable.Pitch;
            WeatherConditions = exportable.WeatherConditions;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableWeatherInfoCI> ExportAsync()
        {
            return Task.FromResult(new ExportableWeatherInfoCI
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
