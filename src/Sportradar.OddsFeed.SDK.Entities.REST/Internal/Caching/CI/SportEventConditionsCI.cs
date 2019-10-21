/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A implementation of cache item for SportEventConditions
    /// </summary>
    public class SportEventConditionsCI
    {
        /// <summary>
        /// Gets a <see cref="string" /> specifying the attendance of the associated sport event
        /// </summary>
        public string Attendance { get; private set; }

        /// <summary>
        /// The mode of the event
        /// </summary>
        public string EventMode { get; private set; }

        /// <summary>
        /// Gets the <see cref="IReferee" /> instance representing the referee presiding over the associated sport event
        /// </summary>
        public RefereeCI Referee { get; private set; }

        /// <summary>
        /// Gets a <see cref="IWeatherInfo" /> instance representing the expected weather on the associated sport event
        /// </summary>
        public WeatherInfoCI WeatherInfo { get; private set; }

        /// <summary>
        /// Gets the pitchers
        /// </summary>
        /// <value>The pitchers</value>
        public IEnumerable<PitcherCI> Pitchers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventConditions"/> class
        /// </summary>
        /// <param name="dto">A <see cref="SportEventConditionsDTO"/> used to create new instance</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the sport event conditions</param>
        internal SportEventConditionsCI(SportEventConditionsDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto).NotNull();
            Guard.Argument(culture).NotNull();

            Merge(dto, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventConditions"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSportEventConditionsCI"/> used to create new instance</param>
        internal SportEventConditionsCI(ExportableSportEventConditionsCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            Attendance = exportable.Attendance;
            EventMode = exportable.EventMode;
            Referee = exportable.Referee != null ? new RefereeCI(exportable.Referee) : null;
            WeatherInfo = exportable.WeatherInfo != null ? new WeatherInfoCI(exportable.WeatherInfo) : null;
            Pitchers = exportable.Pitchers?.Select(p => new PitcherCI(p)).ToList();
        }

        /// <summary>
        /// Merges the specified <see cref="SportEventConditionsDTO"/> into instance
        /// </summary>
        /// <param name="dto">A <see cref="SportEventConditionsDTO"/> containing information about the sport event conditions</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the sport event conditions</param>
        internal void Merge(SportEventConditionsDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto).NotNull();
            Guard.Argument(culture).NotNull();

            Attendance = dto.Attendance;
            EventMode = dto.EventMode;
            if (dto.Referee != null)
            {
                if (Referee == null)
                {
                    Referee = new RefereeCI(dto.Referee, culture);
                }
                else
                {
                    Referee.Merge(dto.Referee, culture);
                }

            }
            if (dto.WeatherInfo != null)
            {
                WeatherInfo = new WeatherInfoCI(dto.WeatherInfo);
            }

            if (dto.Pitchers != null)
            {
                Pitchers = dto.Pitchers.Select(s => new PitcherCI(s, culture));
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableSportEventConditionsCI> ExportAsync()
        {
            var pitcherTasks = Pitchers?.Select(async p => await p.ExportAsync().ConfigureAwait(false));

            return new ExportableSportEventConditionsCI
            {
                Attendance = Attendance,
                EventMode = EventMode,
                Referee = Referee != null ? await Referee.ExportAsync().ConfigureAwait(false) : null,
                WeatherInfo = WeatherInfo != null ? await WeatherInfo.ExportAsync().ConfigureAwait(false) as ExportableWeatherInfoCI : null,
                Pitchers = pitcherTasks != null ? await Task.WhenAll(pitcherTasks) : null
            };
        }
    }
}
