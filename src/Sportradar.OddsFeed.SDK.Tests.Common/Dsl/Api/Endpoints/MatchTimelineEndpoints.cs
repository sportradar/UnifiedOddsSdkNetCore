// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public static class MatchTimelineEndpoints
{
    public static matchTimelineEndpoint LiverpoolVsPsg2025MatchTimeline()
    {
        var liverpoolVsPsgSummary = Soccer.Summary();
        return MatchTimelineEndpointBuilder.Create()
                                           .WithGeneratedAtUtc(DateTime.Parse("2025-09-23T13:56:54+00:00"))
                                           .WithSportEvent(liverpoolVsPsgSummary.sport_event)
                                           .WithSportEventConditions(liverpoolVsPsgSummary.sport_event_conditions)
                                           .WithSportEventStatus(liverpoolVsPsgSummary.sport_event_status)
                                           .WithCoverageInfo(liverpoolVsPsgSummary.coverage_info)
                                           .AddEvent(BasicEventBuilder.MatchStarted(DateTime.Parse("2025-03-11T20:00:40+00:00")).WithId(1990585941).Build())
                                           .AddEvent(BasicEventBuilder.PeriodStart("1st half", "1", DateTime.Parse("2025-03-11T20:00:40+00:00"), 6).WithId(1990585937).Build())
                                           .AddEvent(BasicEventBuilder.CornerKick("away", DateTime.Parse("2025-03-11T20:03:56+00:00"), 4, "3:16").WithId(1990588727).WithCoordinates(0, 0).Build())
                                           .AddEvent(BasicEventBuilder.CornerKick("home", DateTime.Parse("2025-03-11T20:05:26+00:00"), 5, "4:46")
                                                                      .WithId(1990590053)
                                                                      .WithCoordinates(100, 100)
                                                                      .Build())
                                           .AddEvent(BasicEventBuilder.CornerKick("home", DateTime.Parse("2025-03-11T20:10:15+00:00"), 10, "9:34").WithId(1990594583).WithCoordinates(100, 0).Build())
                                           .AddEvent(BasicEventBuilder.ScoreChange("away", DateTime.Parse("2025-03-11T20:12:11+00:00"), 0, 1, 7, 46, 12, "11:29").WithId(1990596609).Build())
                                           .AddEvent(BasicEventBuilder.CornerKick("home", DateTime.Parse("2025-03-11T20:16:34+00:00"), 16, "15:53")
                                                                      .WithId(1990600823)
                                                                      .WithCoordinates(100, 0)
                                                                      .Build())
                                           .AddEvent(BasicEventBuilder.CornerKick("home", DateTime.Parse("2025-03-11T20:18:11+00:00"), 18, "17:31")
                                                                      .WithId(1990602203)
                                                                      .WithCoordinates(100, 100)
                                                                      .Build())
                                           .Build();
    }
}
