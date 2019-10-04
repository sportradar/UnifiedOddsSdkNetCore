/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides information about sport event conditions
    /// </summary>
    /// <seealso cref="ISportEventConditions" />
    internal class SportEventConditions : EntityPrinter, ISportEventConditionsV1
    {
        /// <summary>
        /// Gets a <see cref="string" /> specifying the attendance of the associated sport event
        /// </summary>
        public string Attendance { get; }

        /// <summary>
        /// Gets the event mode property
        /// </summary>
        public string EventMode { get; }

        /// <summary>
        /// Gets the <see cref="IReferee" /> instance representing the referee presiding over the associated sport event
        /// </summary>
        public IReferee Referee { get; }

        /// <summary>
        /// Gets a <see cref="IWeatherInfo" /> instance representing the expected weather on the associated sport event
        /// </summary>
        public IWeatherInfo WeatherInfo { get; }

        /// <summary>
        /// Gets the pitchers
        /// </summary>
        /// <value>The pitchers</value>
        public IEnumerable<IPitcher> Pitchers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventConditions"/> class
        /// </summary>
        /// <param name="ci">A <see cref="SportEventConditionsCI"/> used to create new instance</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the supported languages of the constructed instance</param>
        public SportEventConditions(SportEventConditionsCI ci, IEnumerable<CultureInfo> cultures)
        {
            Contract.Requires(ci != null);
            Contract.Requires(cultures != null && cultures.Any());
            Attendance = ci.Attendance;
            EventMode = ci.EventMode;
            if (ci.Referee != null)
            {
                Referee = new Referee(ci.Referee, cultures);
            }
            if (ci.WeatherInfo != null)
            {
                WeatherInfo = new WeatherInfo(ci.WeatherInfo);
            }

            if (ci.Pitchers != null)
            {
                Pitchers = ci.Pitchers.Select(s => new Pitcher(s));
            }
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"Attendance={Attendance}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var refereeStr = Referee == null
                ? string.Empty
                : ((Referee) Referee).ToString("c");
            var weatherStr = WeatherInfo == null
                ? string.Empty
                : ((WeatherInfo) WeatherInfo).ToString("c");
            var pitcherStr = Pitchers == null
                ? string.Empty
                : string.Join(",", Pitchers.Select(s=>s.Id));
            return $"Attendance={Attendance}, EventMode={EventMode}, Referee={refereeStr}, WeatherInfo={weatherStr}, Pitchers=[{pitcherStr}]";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var refereeStr = Referee == null
                ? string.Empty
                : ((Referee)Referee).ToString("f");
            var weatherStr = WeatherInfo == null
                ? string.Empty
                : ((WeatherInfo)WeatherInfo).ToString("f");
            var pitcherStr = Pitchers == null
                ? string.Empty
                : string.Join(",", Pitchers.Select(s => s.Id));
            return $"Attendance={Attendance}, EventMode={EventMode}, Referee={refereeStr}, WeatherInfo={weatherStr}, Pitchers=[{pitcherStr}]";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
