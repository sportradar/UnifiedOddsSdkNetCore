// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class TimelineEvent : ITimelineEvent
    {
        private readonly int _x;
        private readonly int _y;
        public long Id { get; }
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
        int? ITimelineEvent.X => _x;
        int? ITimelineEvent.Y => _y;
        public int? X { get; }
        public int? Y { get; }
        public DateTime Time { get; }
        public IEnumerable<IAssist> Assists { get; }
        public IGoalScorer GoalScorer { get; }
        public IEventPlayer Player { get; }
        public int? MatchStatusCode { get; }
        public string MatchClock { get; }

        internal TimelineEvent(TimelineEventCacheItem ci)
        {
            Guard.Argument(ci, nameof(ci)).NotNull();

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
                GoalScorer = new GoalScorer(ci.GoalScorer);
            }
            if (ci.Player != null)
            {
                Player = new EventPlayer(ci.Player);
            }
            MatchStatusCode = ci.MatchStatusCode;
            MatchClock = ci.MatchClock;
        }
    }
}
