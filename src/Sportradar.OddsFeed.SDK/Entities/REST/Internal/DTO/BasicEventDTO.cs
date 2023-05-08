/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for <see cref="basicEvent"/> used in <see cref="MatchTimelineDTO"/>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "DTO is allowed solution wide")]
    internal class BasicEventDTO
    {
        public readonly int Id;
        public decimal? HomeScore;
        public decimal? AwayScore;
        public int? MatchTime;
        public readonly string Period;
        public readonly string PeriodName;
        public readonly string Points;
        public readonly string StoppageTime;
        public HomeAway? Team;
        public readonly string Type;
        public readonly string Value;
        public int? X;
        public int? Y;
        public DateTime Time;
        public readonly IEnumerable<EventPlayerAssistDTO> Assists;
        public readonly EventPlayerDTO GoalScorer;
        public readonly EventPlayerDTO Player;
        public int? MatchStatusCode;
        public readonly string MatchClock;

        internal BasicEventDTO(basicEvent item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Id = item.id;
            HomeScore = string.IsNullOrEmpty(item.home_score) ? (decimal?)null : decimal.Parse(item.home_score, NumberStyles.Any, CultureInfo.InvariantCulture);
            AwayScore = string.IsNullOrEmpty(item.away_score) ? (decimal?)null : decimal.Parse(item.away_score, NumberStyles.Any, CultureInfo.InvariantCulture);
            MatchTime = item.match_timeSpecified ? (int?)item.match_time : null;
            Period = item.period;
            PeriodName = item.period_name;
            Points = item.points;
            StoppageTime = item.stoppage_time;
            Team = item.team == null
                ? (HomeAway?)null
                : item.team.Equals("home", StringComparison.InvariantCultureIgnoreCase)
                    ? HomeAway.Home
                    : HomeAway.Away;
            Type = item.type;
            Value = item.value;
            X = item.xSpecified ? (int?)item.x : null;
            Y = item.ySpecified ? (int?)item.y : null;
            Time = item.time;
            if (item.assist != null && item.assist.Length > 0)
            {
                Assists = item.assist.Select(s => new EventPlayerAssistDTO(s));
            }
            if (item.goal_scorer != null)
            {
                GoalScorer = new EventPlayerDTO(item.goal_scorer);
            }
            if (item.player != null)
            {
                Player = new EventPlayerDTO(item.player);
            }
            MatchStatusCode = item.match_status_codeSpecified ? (int?)item.match_status_code : null;
            MatchClock = item.match_clock;
        }
    }
}
