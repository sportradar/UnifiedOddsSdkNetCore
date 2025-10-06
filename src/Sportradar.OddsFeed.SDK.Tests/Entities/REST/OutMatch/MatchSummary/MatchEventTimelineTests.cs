// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions.Assertions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints.MatchTimelineEndpoints;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.OutMatch.MatchSummary;

public class MatchEventTimelineTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IUofConfiguration _uofConfiguration;

    public MatchEventTimelineTests(ITestOutputHelper testOutputHelper)
    {
        _uofConfiguration = UofConfigurations.SingleLanguage;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);
    }

    [Fact]
    public async Task MatchTimelineReturnsTimelineEndpointValuesWhenRequestedUsingMatchAndTimelineIdsAreGreaterThanMaxInteger()
    {
        var liverpoolVsPsg2025MatchTimeline = LiverpoolVsPsg2025MatchTimeline();
        var matchId = liverpoolVsPsg2025MatchTimeline.sport_event.id.ToUrn();
        var sportId = liverpoolVsPsg2025MatchTimeline.sport_event.tournament.sport.id.ToUrn();

        AssignLongToFirstTimelineId(liverpoolVsPsg2025MatchTimeline);
        var sportEventCache = liverpoolVsPsg2025MatchTimeline.CreateSportEventCacheWithTimeline(_uofConfiguration, _loggerFactory);
        var sportEntityFactory = new SportEntityFactoryBuilder()
                                .WithAllMockedDependencies()
                                .WithSportEventCache(sportEventCache)
                                .Build();

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], _uofConfiguration.ExceptionHandlingStrategy);
        var eventTimeline = await match.GetEventTimelineAsync();

        eventTimeline.ShouldNotBeNull();
        eventTimeline.TimelineEvents.ShouldNotBeNull();
        eventTimeline.TimelineEvents.ShouldMatchEvents(liverpoolVsPsg2025MatchTimeline.timeline);
    }

    private static void AssignLongToFirstTimelineId(matchTimelineEndpoint liverpoolVsPsg2025MatchTimeline)
    {
        liverpoolVsPsg2025MatchTimeline.timeline[0].id = int.MaxValue + 1L;
    }
}
