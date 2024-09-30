// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class PlayerProfileExpressionTests
{
    private readonly IOperandFactory _operandFactory = new OperandFactory();
    private readonly TestSportEntityFactoryBuilder _sportEntityFactoryBuilder;

    public PlayerProfileExpressionTests(ITestOutputHelper outputHelper)
    {
        _sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3.ToList());
    }

    [Fact]
    public async Task NotCachedPlayerProfileExpressionReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));

        var result = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), new[] { TestData.Culture }, true);
        Assert.Equal(profile.GetName(TestData.Culture), result);
    }

    [Fact]
    public async Task NotCachedPlayerProfileMultiCallExpressionInvokesEndpointOnlyOnce()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));

        var result1 = await expression.BuildNameAsync(TestData.Culture);
        var result2 = await expression.BuildNameAsync(TestData.Culture);
        var result3 = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(result1, result2);
        Assert.Equal(result3, result2);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), new[] { TestData.Culture }, true);
        Assert.Equal(profile.GetName(TestData.Culture), result1);
    }

    [Fact]
    public async Task NotCachedCompetitorProfileExpressionReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

        var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

        var result = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains("competitor")));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
        Assert.Equal(profile.GetName(TestData.Culture), result);
    }

    [Fact]
    public async Task NotCachedCompetitorProfileMultiCallExpressionInvokesEndpointOnlyOnce()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

        var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

        var result1 = await expression.BuildNameAsync(TestData.Culture);
        var result2 = await expression.BuildNameAsync(TestData.Culture);
        var result3 = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(result1, result2);
        Assert.Equal(result3, result2);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains("competitor")));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
        Assert.Equal(profile.GetName(TestData.Culture), result1);
    }

    [Fact]
    public async Task CachedPlayerProfileExpressionDoesNotInvokeAgain()
    {
        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), new[] { TestData.Culture }, true);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
        var result = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        Assert.Equal(profile.GetName(TestData.Culture), result);
    }

    [Fact]
    public async Task CachedFromCompetitorPlayerProfileExpressionDoesNotInvokeAgain()
    {
        await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
        Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
        var result = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), new[] { TestData.Culture }, true);
        Assert.Equal(profile.GetName(TestData.Culture), result);
    }

    [Fact]
    public async Task CachedCompetitorProfileExpressionDoesNotInvokeAgain()
    {
        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:1"), new[] { TestData.Culture }, true);
        Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains("competitor")));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
        var result = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains("competitor")));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        Assert.Equal(profile.GetName(TestData.Culture), result);
    }

    [Fact]
    public async Task CachedFromMatchCompetitorProfileExpressionDoesNotInvokeAgain()
    {
        var matchId = Urn.Parse("sr:match:1");
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, TestData.Culture, null);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

        _ = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task CachedFromStageCompetitorProfileExpressionDoesNotInvokeAgain()
    {
        const string competitorId = "sr:competitor:7135";
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(TestData.EventStageId, TestData.Culture, null);
        Assert.Equal(22, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

        _ = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(22, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task CachedFromSeasonCompetitorProfileExpressionDoesNotInvokeAgain()
    {
        const string competitorId = "sr:competitor:2";
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(TestData.SeasonId, TestData.Culture, null);
        Assert.Equal(20, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
        var result = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(20, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(Urn.Parse(competitorId), new[] { TestData.Culture }, true);
        Assert.Equal(profile.GetName(TestData.Culture), result);
    }

    [Fact]
    public async Task CachedFromTournamentCompetitorProfileExpressionDoesNotInvokeAgain()
    {
        const string tournamentId = "sr:tournament:1030";
        const string competitorId = "sr:competitor:66412";
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(Urn.Parse(tournamentId), TestData.Culture, null);
        Assert.Equal(16, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));

        _ = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(16, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task CachedFromScheduleForDateCompetitorProfileExpressionDoesNotInvokeAgain()
    {
        const string competitorId = "sr:competitor:66390";
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventsForDateAsync(DateTime.Now, TestData.Culture);
        Assert.Equal(1796, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventsForDate));

        var specifiers = new Dictionary<string, string> { { "competitor", competitorId } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
        var result = await expression.BuildNameAsync(TestData.Culture);
        Assert.Equal(1796, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventsForDate));

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(Urn.Parse(competitorId), new[] { TestData.Culture }, true);
        Assert.Equal(profile.GetName(TestData.Culture), result);
    }

    [Fact]
    public async Task PartiallyPlayerProfileExpressionDoesInvokeForMissingCultures()
    {
        var cultures = TestData.Cultures3.ToList();

        _ = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), new[] { cultures[0] }, true);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
        var result1 = await expression.BuildNameAsync(cultures[0]);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var result2 = await expression.BuildNameAsync(cultures[1]);
        var result3 = await expression.BuildNameAsync(cultures[2]);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(result1, result2);
        Assert.Equal(result1, result3);

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), new[] { cultures[0] }, true);
        Assert.Equal(profile.GetName(cultures[0]), result1);
    }

    [Fact]
    public async Task PartiallyCachedFromCompetitorPlayerProfileExpressionDoesInvokeForMissingCultures()
    {
        var cultures = TestData.Cultures3.ToList();

        _ = await _sportEntityFactoryBuilder.ProfileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:1"), new[] { cultures[0] }, true);
        Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));
        var result1 = await expression.BuildNameAsync(cultures[0]);
        Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var result2 = await expression.BuildNameAsync(cultures[1]);
        var result3 = await expression.BuildNameAsync(cultures[2]);
        Assert.Equal(35, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile)); // since player is associated with competitor, that will be invoked instead of player profile
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(result1, result2);
        Assert.Equal(result1, result3);

        var profile = await _sportEntityFactoryBuilder.ProfileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), new[] { cultures[0] }, true);
        Assert.Equal(profile.GetName(cultures[0]), result1);
    }

    [Fact]
    public async Task PartiallyCachedFromMatchCompetitorProfileExpressionDoesInvokeForMissingCultures()
    {
        var cultures = TestData.Cultures3.ToList();
        var matchId = Urn.Parse("sr:match:1");
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(matchId, cultures[0], null);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var specifiers = new Dictionary<string, string> { { "competitor", "sr:competitor:1" } };
        var expression = new PlayerProfileExpression(_sportEntityFactoryBuilder.ProfileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "competitor"));
        await expression.BuildNameAsync(cultures[0]);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        await expression.BuildNameAsync(cultures[1]);
        await expression.BuildNameAsync(cultures[2]);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains("competitor")));
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }
}
