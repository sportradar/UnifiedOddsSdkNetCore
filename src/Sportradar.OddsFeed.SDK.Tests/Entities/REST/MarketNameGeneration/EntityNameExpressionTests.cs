// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class EntityNameExpressionTests
{
    private readonly EntityNameExpression _matchEventEntityNameExpression;
    private readonly EntityNameExpression _stageEventEntityNameExpression;
    private readonly EntityNameExpression _tournamentEventEntityNameExpression;
    private readonly IMatch _match;
    private readonly IStage _stage;
    private readonly ITournament _tournament;

    private readonly TestSportEntityFactoryBuilder _sportEntityFactory;

    public EntityNameExpressionTests(ITestOutputHelper outputHelper)
    {
        _sportEntityFactory = new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3);
        _match = _sportEntityFactory.SportEntityFactory.BuildSportEvent<IMatch>(Urn.Parse("sr:match:1"), Urn.Parse("sr:sport:5"), TestData.Cultures3, ExceptionHandlingStrategy.Throw);
        _stage = _sportEntityFactory.SportEntityFactory.BuildSportEvent<IStage>(Urn.Parse("sr:stage:940265"), Urn.Parse("sr:sport:40"), TestData.Cultures3, ExceptionHandlingStrategy.Throw);
        _tournament = _sportEntityFactory.SportEntityFactory.BuildSportEvent<ITournament>(Urn.Parse("sr:tournament:40"), Urn.Parse("sr:sport:1"), TestData.Cultures3, ExceptionHandlingStrategy.Throw);
        _matchEventEntityNameExpression = new EntityNameExpression("event", _match, _sportEntityFactory.ProfileCache);
        _stageEventEntityNameExpression = new EntityNameExpression("event", _stage, _sportEntityFactory.ProfileCache);
        _tournamentEventEntityNameExpression = new EntityNameExpression("event", _tournament, _sportEntityFactory.ProfileCache);
    }

    [Fact]
    public void InitialSetup()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task MatchEventWhenRequestingNameThenReturnHomeAndAwayCompetitor()
    {
        var name = await _matchEventEntityNameExpression.BuildNameAsync(TestData.Culture);
        var homeCompetitor = await _match.GetHomeCompetitorAsync();
        var awayCompetitor = await _match.GetAwayCompetitorAsync();

        Assert.NotNull(name);
        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal($"{homeCompetitor.GetName(TestData.Culture)} vs {awayCompetitor.GetName(TestData.Culture)}", name);
    }

    [Fact]
    public async Task MatchEventWhenRequestingNameThenRequestSportEventSummary()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var name = await _matchEventEntityNameExpression.BuildNameAsync(TestData.Culture);

        name.Should().NotBeNullOrEmpty();
        Assert.Equal(2, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task MatchEventForAllCulturesWhenRequestingNameThenReturnHomeAndAwayCompetitor()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var homeCompetitor = await _match.GetHomeCompetitorAsync();
        var awayCompetitor = await _match.GetAwayCompetitorAsync();

        foreach (var cultureInfo in TestData.Cultures3)
        {
            var name = await _matchEventEntityNameExpression.BuildNameAsync(cultureInfo);
            name.Should().NotBeNullOrEmpty();
            Assert.Equal($"{homeCompetitor.GetName(cultureInfo)} vs {awayCompetitor.GetName(cultureInfo)}", name);
        }
    }

    [Fact]
    public async Task MatchEventForAllCulturesWhenRequestingNameThenRequestSportEventSummary()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var numberOfCalls = 0;
        foreach (var cultureInfo in TestData.Cultures3)
        {
            numberOfCalls++;
            await _matchEventEntityNameExpression.BuildNameAsync(cultureInfo);
            Assert.Equal(2, _sportEntityFactory.ProfileMemoryCache.Count());
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }
    }

    [Fact]
    public async Task MatchEventNameForAllCulturesForPreloadedCompetitorsInvokesMinimalCalls()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        foreach (var cultureInfo in TestData.Cultures3)
        {
            await _sportEntityFactory.DataRouterManager.GetCompetitorAsync(Urn.Parse("sr:competitor:1"), cultureInfo, null);
            await _sportEntityFactory.DataRouterManager.GetCompetitorAsync(Urn.Parse("sr:competitor:2"), cultureInfo, null);
        }
        Assert.Equal(63, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(6, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(6, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        foreach (var cultureInfo in TestData.Cultures3)
        {
            var name = await _matchEventEntityNameExpression.BuildNameAsync(cultureInfo);
            Assert.NotNull(name);
            Assert.False(string.IsNullOrEmpty(name));
            Assert.Equal(63, _sportEntityFactory.ProfileMemoryCache.Count());
            Assert.Equal(7, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(6, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }
    }

    [Fact]
    public async Task StageEventNameForDefaultCultureReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var expressionName = await _stageEventEntityNameExpression.BuildNameAsync(TestData.Culture);
        var stageName = await _stage.GetNameAsync(TestData.Culture);

        Assert.NotNull(expressionName);
        Assert.False(string.IsNullOrEmpty(expressionName));
        Assert.Equal(22, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(stageName, expressionName);
    }

    [Fact]
    public async Task StageEventNameForAllCulturesReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var numberOfCalls = 0;
        foreach (var cultureInfo in TestData.Cultures3)
        {
            numberOfCalls++;
            var name = await _stageEventEntityNameExpression.BuildNameAsync(cultureInfo);
            var stageName = await _stage.GetNameAsync(cultureInfo);

            Assert.NotNull(name);
            Assert.False(string.IsNullOrEmpty(name));
            Assert.Equal(22, _sportEntityFactory.ProfileMemoryCache.Count());
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(stageName, name);
        }
    }

    [Fact]
    public async Task TournamentEventNameForDefaultCultureReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var expressionName = await _tournamentEventEntityNameExpression.BuildNameAsync(TestData.Culture);

        expressionName.Should().NotBeNullOrEmpty();
        Assert.Equal(16, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        var tournamentName = await _tournament.GetNameAsync(TestData.Culture);
        Assert.Equal(tournamentName, expressionName);
    }

    [Fact]
    public async Task MatchNameCompetitor1ForDefaultCultureReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var name = await new EntityNameExpression("competitor1", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture);

        Assert.NotNull(name);
        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(2, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        var homeCompetitor = await _match.GetHomeCompetitorAsync();
        Assert.Equal(homeCompetitor.GetName(TestData.Culture), name);
    }

    [Fact]
    public async Task MatchNameCompetitor2ForDefaultCultureReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var name = await new EntityNameExpression("competitor2", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture);

        Assert.NotNull(name);
        Assert.Equal(2, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        var awayCompetitor = await _match.GetAwayCompetitorAsync();
        Assert.Equal(awayCompetitor.GetName(TestData.Culture), name);
    }

    [Fact]
    public async Task MatchNameWrongPlayerOperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        await Assert.ThrowsAsync<NameExpressionException>(async () => name = await new EntityNameExpression("player", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture));
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task MatchNameWrongCompetitorOperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        await Assert.ThrowsAsync<NameExpressionException>(async () => name = await new EntityNameExpression("competitor", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture));
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task MatchNameWrongCompetitor0OperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        await Assert.ThrowsAsync<NameExpressionException>(async () => name = await new EntityNameExpression("competitor0", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture));
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task MatchNameWrongCompetitor3OperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        await Assert.ThrowsAsync<NameExpressionException>(async () => name = await new EntityNameExpression("competitor3", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture));
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task TournamentNameCompetitor1ForDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        await Assert.ThrowsAsync<NameExpressionException>(async () => name = await new EntityNameExpression("competitor1", _tournament, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture));
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task TournamentNameCompetitor2ForDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        await Assert.ThrowsAsync<NameExpressionException>(async () => name = await new EntityNameExpression("competitor2", _tournament, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture));
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }
}
