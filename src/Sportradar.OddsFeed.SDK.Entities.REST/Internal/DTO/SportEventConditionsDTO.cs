/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for sport event conditions
    /// </summary>
    public class SportEventConditionsDTO
    {
        internal string Attendance { get; }

        internal string EventMode { get; }

        internal RefereeDTO Referee { get; }

        internal WeatherInfoDTO WeatherInfo { get; }

        internal IEnumerable<PitcherDTO> Pitchers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventConditionsDTO"/> class
        /// </summary>
        /// <param name="conditions">The <see cref="sportEventConditions"/> used for creating instance</param>
        internal SportEventConditionsDTO(sportEventConditions conditions)
        {
            Guard.Argument(conditions).NotNull();

            Attendance = conditions.attendance;
            EventMode = conditions.match_mode;
            Referee = conditions.referee == null
                ? null
                : new RefereeDTO(conditions.referee);
            WeatherInfo = conditions.weather_info == null
                ? null
                : new WeatherInfoDTO(conditions.weather_info);
            Pitchers = conditions.pitchers == null || !conditions.pitchers.Any()
                ? null
                : conditions.pitchers.Select(s => new PitcherDTO(s));
        }
    }
}
