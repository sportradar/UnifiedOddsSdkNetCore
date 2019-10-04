/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing weather conditions
    /// </summary>
    public interface IWeatherInfo : IEntityPrinter
    {
        /// <summary>
        /// TODO: add comments
        /// </summary>
        string Pitch { get; }

        /// <summary>
        /// Gets the temperature in degrees celsius or a null reference if the expected temperature is not known
        /// </summary>
        int? Temperature { get; }

        /// <summary>
        /// Gets a <see cref="string"/> specifying the weather conditions (cloudy, sunny, ...)
        /// </summary>
        string WeatherConditions { get; }

        /// <summary>
        /// Gets a <see cref="string"/> specifying the wind conditions
        /// </summary>
        string Wind { get; }

        /// <summary>
        /// TOOD: add comments
        /// </summary>
        string WindAdvantage { get; }
    }
}