// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using static System.Array;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Extensions.Assertions;

internal static class TimelineAssertions
{
    internal static void ShouldMatchEvents(this IEnumerable<ITimelineEvent> actual, basicEvent[] expected)
    {
        var timelineEvents = actual?.ToList();
        timelineEvents.ShouldNotBeNull();
        expected.ShouldNotBeNull();

        timelineEvents.Count.ShouldBe(expected.Length);

        for (var i = 0; i < expected.Length; i++)
        {
            timelineEvents[i].ShouldMatchExpectedBasicEvent(expected[i]);
        }
    }

    internal static void ShouldMatchEvents(this IEnumerable<TimelineEventCacheItem> actual, basicEvent[] expected)
    {
        var eventsList = actual?.ToList();
        eventsList.ShouldNotBeNull();
        expected.ShouldNotBeNull();
        eventsList!.ShouldBeOfSize(expected.Length);

        for (var i = 0; i < expected.Length; i++)
        {
            eventsList[i].ShouldMatchExpectedBasicEvent(expected[i]);
        }
    }

    private static void ShouldMatchExpectedBasicEvent(this ITimelineEvent timelineEvent, basicEvent expectedTimelineEvent)
    {
        timelineEvent.Id.ShouldBe(expectedTimelineEvent.id);

        var homeScore = ParseNullableDecimal(expectedTimelineEvent.home_score);
        var awayScore = ParseNullableDecimal(expectedTimelineEvent.away_score);
        timelineEvent.AwayScore.ShouldBe(awayScore);
        timelineEvent.HomeScore.ShouldBe(homeScore);

        var matchTime = expectedTimelineEvent.match_timeSpecified ? (int?)expectedTimelineEvent.match_time : null;
        timelineEvent.MatchTime.ShouldBe(matchTime);

        timelineEvent.Period.ShouldBe(expectedTimelineEvent.period);
        timelineEvent.PeriodName.ShouldBe(expectedTimelineEvent.period_name);
        timelineEvent.Points.ShouldBe(expectedTimelineEvent.points);
        timelineEvent.StoppageTime.ShouldBe(expectedTimelineEvent.stoppage_time);
        timelineEvent.Team.ShouldBe(ParseTeam(expectedTimelineEvent.team));
        timelineEvent.Type.ShouldBe(expectedTimelineEvent.type);
        timelineEvent.Value.ShouldBe(expectedTimelineEvent.value);

        timelineEvent.X.ShouldBe(expectedTimelineEvent.x);
        timelineEvent.Y.ShouldBe(expectedTimelineEvent.y);

        timelineEvent.Time.ShouldBe(expectedTimelineEvent.time);
        timelineEvent.MatchStatusCode.ShouldBe(expectedTimelineEvent.match_status_codeSpecified ? expectedTimelineEvent.match_status_code : null);
        timelineEvent.MatchClock.ShouldBe(expectedTimelineEvent.match_clock);

        timelineEvent.Assists.ShouldMatchAssists(expectedTimelineEvent.assist);
        timelineEvent.GoalScorer.ShouldMatchGoalScorer(expectedTimelineEvent.goal_scorer);
        timelineEvent.Player.ShouldMatchPlayer(expectedTimelineEvent.player);
    }

    private static void ShouldMatchExpectedBasicEvent(this TimelineEventCacheItem actual, basicEvent expected)
    {
        actual.ShouldNotBeNull();
        expected.ShouldNotBeNull();

        actual.Id.ShouldBe(expected.id);

        var homeScore = ParseNullableDecimal(expected.home_score);
        var awayScore = ParseNullableDecimal(expected.away_score);
        actual.HomeScore.ShouldBe(homeScore);
        actual.AwayScore.ShouldBe(awayScore);

        var matchTime = expected.match_timeSpecified ? (int?)expected.match_time : null;
        actual.MatchTime.ShouldBe(matchTime);

        actual.Period.ShouldBe(expected.period);
        actual.PeriodName.ShouldBe(expected.period_name);
        actual.Points.ShouldBe(expected.points);
        actual.StoppageTime.ShouldBe(expected.stoppage_time);
        actual.Value.ShouldBe(expected.value);
        actual.Type.ShouldBe(expected.type);

        var team = Enum.TryParse<HomeAway>(expected.team, true, out var parsedTeam) ? parsedTeam : (HomeAway?)null;
        actual.Team.ShouldBe(team);

        var ex = expected.xSpecified ? (int?)expected.x : null;
        var ey = expected.ySpecified ? (int?)expected.y : null;
        actual.X.ShouldBe(ex);
        actual.Y.ShouldBe(ey);

        actual.Time.ShouldBe(expected.time);

        var expectedMatchStatus = expected.match_status_codeSpecified ? (int?)expected.match_status_code : null;
        actual.MatchStatusCode.ShouldBe(expectedMatchStatus);

        actual.MatchClock.ShouldBe(expected.match_clock);

        actual.Assists.ShouldMatchAssists(expected.assist);
        actual.GoalScorer.ShouldMatchEventPlayer(expected.goal_scorer);
        actual.Player.ShouldMatchEventPlayer(expected.player);
    }

    private static void ShouldMatchAssists(this IEnumerable<EventPlayerAssistCacheItem> actualAssists, eventPlayerAssist[] expectedAssists)
    {
        var expected = expectedAssists ?? [];
        var actual = (actualAssists ?? Empty<EventPlayerAssistCacheItem>()).ToList();
        actual.Count.ShouldBe(expected.Length);

        for (var i = 0; i < expected.Length; i++)
        {
            actual[i].Type.ShouldBe(expected[i].type);
            actual[i].Id.ToString().ShouldBe(expected[i].id);
            actual[i].Name.Values.ShouldContain(expected[i].name);
        }
    }

    private static void ShouldMatchEventPlayer(this EventPlayerCacheItem actual, eventPlayer expected)
    {
        if (actual == null)
        {
            expected.ShouldBeNull();
            return;
        }

        expected.ShouldNotBeNull();
        actual.Id.ToString().ShouldBe(expected.id);
        actual.Bench.ShouldBe(expected.bench);
        actual.Method.ShouldBe(expected.method);
        actual.Name.Values.ShouldContain(expected.name);
    }

    private static void ShouldMatchAssists(this IEnumerable<IAssist> actualAssists, eventPlayerAssist[] expectedAssists)
    {
        var expected = expectedAssists ?? [];
        var actual = (actualAssists ?? Empty<IAssist>()).ToList();
        actual.Count.ShouldBe(expected.Length);

        var index = 0;
        foreach (var playerAssist in expected)
        {
            var assist = actual[index++];
            assist.ShouldNotBeNull();
            assist.Type.ShouldBe(playerAssist.type);
            assist.Id.ShouldBe(playerAssist.id.ToUrn());
            assist.Names.Values.ShouldContain(playerAssist.name);
        }
    }

    private static void ShouldMatchGoalScorer(this IGoalScorer actualGoalScorer, eventPlayer expectedGoalScorer)
    {
        if (actualGoalScorer is null)
        {
            expectedGoalScorer.ShouldBeNull();
            return;
        }

        expectedGoalScorer.ShouldNotBeNull();

        actualGoalScorer.Id.ToString().ShouldBe(expectedGoalScorer.id);
        actualGoalScorer.Method.ShouldBe(expectedGoalScorer.method);
        actualGoalScorer.Names.Values.ShouldContain(expectedGoalScorer.name);
    }

    private static void ShouldMatchPlayer(this IEventPlayer player, eventPlayer expectedPlayer)
    {
        if (player is null)
        {
            expectedPlayer.ShouldBeNull();
            return;
        }

        expectedPlayer.ShouldNotBeNull();

        player.Id.ToString().ShouldBe(expectedPlayer.id);
        player.Bench.ShouldBe(expectedPlayer.bench);
        player.Names.Values.ShouldContain(expectedPlayer.name);
    }

    private static decimal? ParseNullableDecimal(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
        {
            return d;
        }

        return null;
    }

    private static HomeAway? ParseTeam(string team)
    {
        return team.IsNullOrEmpty() ? null : Enum.Parse<HomeAway>(team, true);
    }
}
