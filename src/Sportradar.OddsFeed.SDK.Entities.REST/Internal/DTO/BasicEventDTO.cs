/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for <see cref="basicEvent"/> used in <see cref="MatchTimelineDTO"/>
    /// </summary>
    public class BasicEventDTO
    {
        public int Id;
        public decimal? HomeScore;
        public decimal? AwayScore;
        public int? MatchTime;
        public string Period;
        public string PeriodName;
        public string Points;
        public string StoppageTime;
        public HomeAway? Team;
        public string Type;
        public string Value;
        public int? X;
        public int? Y;
        public DateTime Time;
        public IEnumerable<EventPlayerAssistDTO> Assists;
        public SportEntityDTO GoalScorer;
        public SportEntityDTO Player;
        public int? MatchStatusCode;
        public string MatchClock;

        internal BasicEventDTO(basicEvent item)
        {
            Contract.Requires(item != null);

            Id = item.id;
            HomeScore = item.home_scoreSpecified ? (decimal?) item.home_score : null;
            AwayScore = item.away_scoreSpecified ? (decimal?) item.away_score : null;
            MatchTime = item.match_timeSpecified ? (int?) item.match_time : null;
            Period = item.period;
            PeriodName = item.period_name;
            Points = item.points;
            StoppageTime = item.stoppage_time;
            Team = item.team == null
                ? (HomeAway?) null
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
                GoalScorer = new SportEntityDTO(item.goal_scorer.id, item.goal_scorer.name);
            }
            if (item.player != null)
            {
                Player = new SportEntityDTO(item.player.id, item.player.name);
            }
            MatchStatusCode = item.match_status_codeSpecified ? (int?) item.match_status_code : null;
            MatchClock = item.match_clock;
        }
    }
}
