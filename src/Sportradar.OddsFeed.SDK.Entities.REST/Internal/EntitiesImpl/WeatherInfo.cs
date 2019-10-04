/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides information about weather conditions
    /// </summary>
    /// <seealso cref="IWeatherInfo" />
    internal class WeatherInfo : EntityPrinter, IWeatherInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherInfo"/> class
        /// </summary>
        /// <param name="cacheItem">A <see cref="WeatherInfoCI"/> used to create new instance</param>
        internal WeatherInfo(WeatherInfoCI cacheItem)
        {
            Pitch = cacheItem.Pitch;
            Temperature = cacheItem.TemperatureCelsius;
            WeatherConditions = cacheItem.WeatherConditions;
            WindAdvantage = cacheItem.WindAdvantage;
            Wind = cacheItem.Wind;
        }

        /// <summary>
        /// Gets the pitch value
        /// </summary>
        public string Pitch { get; }

        /// <summary>
        /// Gets the temperature in degrees celsius or a null reference if the expected temperature is not known
        /// </summary>
        public int? Temperature { get; }

        /// <summary>
        /// Gets a <see cref="string" /> specifying the weather conditions (cloudy, sunny, ...)
        /// </summary>
        public string WeatherConditions { get; }

        /// <summary>
        /// Gets a <see cref="string" /> specifying the wind conditions
        /// </summary>
        public string Wind { get; }

        /// <summary>
        /// Gets the wind advantage value
        /// </summary>
        public string WindAdvantage { get; }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Pitch={Pitch}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"Pitch={Pitch}, Temparature={Temperature}, WeatherConditions={WeatherConditions}, Wind={Wind}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
