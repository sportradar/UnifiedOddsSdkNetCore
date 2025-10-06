// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A implementation of cache item for SportEventConditions
    /// </summary>
    internal class SportEventConditionsCacheItem
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
        public RefereeCacheItem Referee { get; private set; }

        /// <summary>
        /// Gets a <see cref="IWeatherInfo" /> instance representing the expected weather on the associated sport event
        /// </summary>
        public WeatherInfoCacheItem WeatherInfo { get; private set; }

        /// <summary>
        /// Gets the pitchers
        /// </summary>
        /// <value>The pitchers</value>
        public IEnumerable<PitcherCacheItem> Pitchers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventConditions"/> class
        /// </summary>
        /// <param name="dto">A <see cref="SportEventConditionsDto"/> used to create new instance</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the sport event conditions</param>
        internal SportEventConditionsCacheItem(SportEventConditionsDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Merge(dto, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventConditions"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSportEventConditions"/> used to create new instance</param>
        internal SportEventConditionsCacheItem(ExportableSportEventConditions exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Attendance = exportable.Attendance;
            EventMode = exportable.EventMode;
            Referee = exportable.Referee != null ? new RefereeCacheItem(exportable.Referee) : null;
            WeatherInfo = exportable.WeatherInfo != null ? new WeatherInfoCacheItem(exportable.WeatherInfo) : null;
            Pitchers = exportable.Pitchers?.Select(p => new PitcherCacheItem(p)).ToList();
        }

        /// <summary>
        /// Merges the specified <see cref="SportEventConditionsDto"/> into instance
        /// </summary>
        /// <param name="dto">A <see cref="SportEventConditionsDto"/> containing information about the sport event conditions</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the sport event conditions</param>
        internal void Merge(SportEventConditionsDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Attendance = dto.Attendance;
            EventMode = dto.EventMode;
            if (dto.Referee != null)
            {
                if (Referee == null)
                {
                    Referee = new RefereeCacheItem(dto.Referee, culture);
                }
                else
                {
                    Referee.Merge(dto.Referee, culture);
                }
            }
            if (dto.WeatherInfo != null)
            {
                WeatherInfo = new WeatherInfoCacheItem(dto.WeatherInfo);
            }

            if (dto.Pitchers != null)
            {
                Pitchers = dto.Pitchers.Select(s => new PitcherCacheItem(s, culture));
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public async Task<ExportableSportEventConditions> ExportAsync()
        {
            var pitcherTasks = Pitchers?.Select(async p => await p.ExportAsync().ConfigureAwait(false));

            return new ExportableSportEventConditions
            {
                Attendance = Attendance,
                EventMode = EventMode,
                Referee = Referee != null ? await Referee.ExportAsync().ConfigureAwait(false) : null,
                WeatherInfo = WeatherInfo != null ? await WeatherInfo.ExportAsync().ConfigureAwait(false) : null,
                Pitchers = pitcherTasks != null ? await Task.WhenAll(pitcherTasks) : null
            };
        }
    }
}
