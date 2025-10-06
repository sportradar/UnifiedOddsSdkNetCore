// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public sealed class BasicEventBuilder
{
    private readonly basicEvent _timelineEvent = new();

    private BasicEventBuilder() { }

    public static BasicEventBuilder Create()
    {
        return new BasicEventBuilder();
    }

    public static BasicEventBuilder MatchStarted(DateTime timeUtc)
    {
        return Create()
              .WithType("match_started")
              .WithTimeUtc(timeUtc);
    }

    public static BasicEventBuilder PeriodStart(string periodName, string period, DateTime timeUtc, int? matchStatusCode)
    {
        var builder = Create()
                     .WithType("period_start")
                     .WithPeriodName(periodName)
                     .WithPeriod(period)
                     .WithTimeUtc(timeUtc);

        if (matchStatusCode.HasValue)
        {
            builder.WithMatchStatusCode(matchStatusCode.Value);
        }

        return builder;
    }

    public static BasicEventBuilder CornerKick(string team, DateTime timeUtc, int? matchTimeMinutes, string matchClock)
    {
        var builder = Create()
                     .WithType("corner_kick")
                     .WithTeam(team)
                     .WithTimeUtc(timeUtc)
                     .WithMatchClock(matchClock);

        if (matchTimeMinutes.HasValue)
        {
            builder.WithMatchTime(matchTimeMinutes.Value);
        }

        return builder;
    }

    public static BasicEventBuilder ScoreChange(string team,
        DateTime timeUtc,
        int homeScore,
        int awayScore,
        int? x,
        int? y,
        int? matchTimeMinutes,
        string matchClock)
    {
        var builder = Create()
               .WithType("score_change")
               .WithTeam(team)
               .WithTimeUtc(timeUtc)
               .WithHomeScore(homeScore)
               .WithAwayScore(awayScore)
               .WithMatchClock(matchClock);

        if (x.HasValue)
        {
            builder.WithX(x.Value);
        }

        if (y.HasValue)
        {
            builder.WithY(y.Value);
        }

        if (matchTimeMinutes.HasValue)
        {
            builder.WithMatchTime(matchTimeMinutes.Value);
        }

        return builder;
    }

    public BasicEventBuilder WithId(long id)
    {
        _timelineEvent.id = id;
        return this;
    }

    public BasicEventBuilder WithType(string type)
    {
        _timelineEvent.type = type;
        return this;
    }

    public BasicEventBuilder WithTimeUtc(DateTime timeUtc)
    {
        _timelineEvent.time = timeUtc;
        return this;
    }

    public BasicEventBuilder WithPeriodName(string periodName)
    {
        _timelineEvent.period_name = periodName;
        return this;
    }

    public BasicEventBuilder WithPeriod(string period)
    {
        _timelineEvent.period = period;
        return this;
    }

    public BasicEventBuilder WithStoppageTime(string stoppage)
    {
        _timelineEvent.stoppage_time = stoppage;
        return this;
    }

    public BasicEventBuilder WithTeam(string team)
    {
        _timelineEvent.team = team;
        return this;
    }

    public BasicEventBuilder WithMatchClock(string matchClock)
    {
        _timelineEvent.match_clock = matchClock;
        return this;
    }

    public BasicEventBuilder WithMatchTime(int minutes)
    {
        _timelineEvent.match_time = minutes;
        _timelineEvent.match_timeSpecified = true;
        return this;
    }

    public BasicEventBuilder WithoutMatchTime()
    {
        _timelineEvent.match_time = default;
        _timelineEvent.match_timeSpecified = false;
        return this;
    }

    public BasicEventBuilder WithX(int x)
    {
        _timelineEvent.x = x;
        _timelineEvent.xSpecified = true;
        return this;
    }

    public BasicEventBuilder WithoutX()
    {
        _timelineEvent.x = default;
        _timelineEvent.xSpecified = false;
        return this;
    }

    public BasicEventBuilder WithY(int y)
    {
        _timelineEvent.y = y;
        _timelineEvent.ySpecified = true;
        return this;
    }

    public BasicEventBuilder WithoutY()
    {
        _timelineEvent.y = default;
        _timelineEvent.ySpecified = false;
        return this;
    }

    public BasicEventBuilder WithCoordinates(int x, int y)
    {
        return WithX(x).WithY(y);
    }

    public BasicEventBuilder WithHomeScore(string score)
    {
        _timelineEvent.home_score = score;
        return this;
    }

    public BasicEventBuilder WithHomeScore(int score)
    {
        _timelineEvent.home_score = score.ToString();
        return this;
    }

    public BasicEventBuilder WithAwayScore(string score)
    {
        _timelineEvent.away_score = score;
        return this;
    }

    public BasicEventBuilder WithAwayScore(int score)
    {
        _timelineEvent.away_score = score.ToString();
        return this;
    }

    public BasicEventBuilder WithValue(string value)
    {
        _timelineEvent.value = value;
        return this;
    }

    public BasicEventBuilder WithPoints(string points)
    {
        _timelineEvent.points = points;
        return this;
    }

    public BasicEventBuilder WithMatchStatusCode(int code)
    {
        _timelineEvent.match_status_code = code;
        _timelineEvent.match_status_codeSpecified = true;
        return this;
    }

    public BasicEventBuilder WithoutMatchStatusCode()
    {
        _timelineEvent.match_status_code = default;
        _timelineEvent.match_status_codeSpecified = false;
        return this;
    }

    public BasicEventBuilder WithGoalScorer(eventPlayer player)
    {
        _timelineEvent.goal_scorer = player;
        return this;
    }

    public BasicEventBuilder ConfigureGoalScorer(Action<eventPlayer> configure)
    {
        if (configure == null)
        {
            return this;
        }

        var p = _timelineEvent.goal_scorer ?? new eventPlayer();
        configure(p);
        _timelineEvent.goal_scorer = p;
        return this;
    }

    public BasicEventBuilder WithPlayer(eventPlayer player)
    {
        _timelineEvent.player = player;
        return this;
    }

    public BasicEventBuilder AddAssist(eventPlayerAssist assist)
    {
        if (assist == null)
        {
            return this;
        }

        var list = _timelineEvent.assist == null ? new List<eventPlayerAssist>() : new List<eventPlayerAssist>(_timelineEvent.assist);
        list.Add(assist);
        _timelineEvent.assist = list.ToArray();
        return this;
    }

    public basicEvent Build()
    {
        return _timelineEvent;
    }
}
