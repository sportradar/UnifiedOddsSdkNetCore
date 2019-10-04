/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    internal class TimelineEvent : ITimelineEventV1
    {
        private readonly int _x;
        private readonly int _y;
        public int Id { get; }
        public decimal? HomeScore { get; }
        public decimal? AwayScore { get; }
        public int? MatchTime { get; }
        public string Period { get; }
        public string PeriodName { get; }
        public string Points { get; }
        public string StoppageTime { get; }
        public HomeAway? Team { get; }
        public string Type { get; }
        public string Value { get; }
        int ITimelineEvent.X => _x;
        int ITimelineEvent.Y => _y;
        public int? X { get; }
        public int? Y { get; }
        public DateTime Time { get; }
        public IEnumerable<IAssist> Assists { get; }
        public IGoalScorer GoalScorer { get; }
        public IPlayer Player { get; }
        public int? MatchStatusCode { get; }
        public string MatchClock { get; }

        internal TimelineEvent(TimelineEventCI ci)
        {
            Contract.Requires(ci != null);

            Id = ci.Id;
            HomeScore = ci.HomeScore;
            AwayScore = ci.AwayScore;
            MatchTime = ci.MatchTime;
            Period = ci.Period;
            PeriodName = ci.PeriodName;
            Points = ci.Points;
            StoppageTime = ci.StoppageTime;
            Team = ci.Team;
            Type = ci.Type;
            Value = ci.Value;
            X = ci.X;
            Y = ci.Y;
            _x = ci.X ?? 0;
            _y = ci.Y ?? 0;
            Time = ci.Time;
            if (ci.Assists != null && ci.Assists.Any())
            {
                Assists = ci.Assists.Select(s => new Assist(s.Id, s.Name, s.Type));
            }
            if (ci.GoalScorer != null)
            {
                GoalScorer = new GoalScorer(ci.GoalScorer.Id, ci.GoalScorer.Name);
            }
            if (ci.Player != null)
            {
                Player = new Player(ci.Player.Id, ci.Player.Name);
            }
            MatchStatusCode = ci.MatchStatusCode;
            MatchClock = ci.MatchClock;
        }
    }
}
