// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for sport event conditions
    /// </summary>
    internal class SportEventConditionsDto
    {
        internal string Attendance { get; }

        internal string EventMode { get; }

        internal RefereeDto Referee { get; }

        internal WeatherInfoDto WeatherInfo { get; }

        internal IEnumerable<PitcherDto> Pitchers { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventConditionsDto"/> class
        /// </summary>
        /// <param name="conditions">The <see cref="sportEventConditions"/> used for creating instance</param>
        internal SportEventConditionsDto(sportEventConditions conditions)
        {
            Guard.Argument(conditions, nameof(conditions)).NotNull();

            Attendance = conditions.attendance;
            EventMode = conditions.match_mode;
            Referee = conditions.referee == null
                ? null
                : new RefereeDto(conditions.referee);
            WeatherInfo = conditions.weather_info == null
                ? null
                : new WeatherInfoDto(conditions.weather_info);
            Pitchers = conditions.pitchers == null || !conditions.pitchers.Any()
                ? null
                : conditions.pitchers.Select(s => new PitcherDto(s));
        }
    }
}
