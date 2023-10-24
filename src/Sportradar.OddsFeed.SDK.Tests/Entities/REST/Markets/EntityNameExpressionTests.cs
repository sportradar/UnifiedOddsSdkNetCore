/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

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
        _tournament = _sportEntityFactory.SportEntityFactory.BuildSportEvent<ITournament>(Urn.Parse("sr:tournament:940265"), Urn.Parse("sr:sport:40"), TestData.Cultures3, ExceptionHandlingStrategy.Throw);
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
    public async Task MatchEventNameForDefaultCultureReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var name = await _matchEventEntityNameExpression.BuildNameAsync(TestData.Culture);
        Assert.NotNull(name);
        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(2, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal($"{_match.GetHomeCompetitorAsync().GetAwaiter().GetResult().GetName(TestData.Culture)} vs {_match.GetAwayCompetitorAsync().GetAwaiter().GetResult().GetName(TestData.Culture)}", name);
    }

    [Fact]
    public async Task MatchEventNameForAllCulturesReturnsCorrectValue()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        var numberOfCalls = 0;
        var name = string.Empty;
        foreach (var cultureInfo in TestData.Cultures3)
        {
            numberOfCalls++;
            name = await _matchEventEntityNameExpression.BuildNameAsync(cultureInfo);
            Assert.NotNull(name);
            Assert.False(string.IsNullOrEmpty(name));
            Assert.False(string.IsNullOrEmpty(name));
            Assert.Equal(2, _sportEntityFactory.ProfileMemoryCache.Count());
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }
        Assert.Equal($"{_match.GetHomeCompetitorAsync().GetAwaiter().GetResult().GetName(TestData.Cultures3.Last())} vs {_match.GetAwayCompetitorAsync().GetAwaiter().GetResult().GetName(TestData.Cultures3.Last())}", name);
    }

    [Fact]
    public async Task MatchEventNameForAllCulturesForPreloadedCompetitorsInvokesMinimalCalls()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        foreach (var cultureInfo in TestData.Cultures3)
        {
            _sportEntityFactory.DataRouterManager.GetCompetitorAsync(Urn.Parse("sr:competitor:1"), cultureInfo, null).GetAwaiter().GetResult();
            _sportEntityFactory.DataRouterManager.GetCompetitorAsync(Urn.Parse("sr:competitor:2"), cultureInfo, null).GetAwaiter().GetResult();
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

        var name = await _stageEventEntityNameExpression.BuildNameAsync(TestData.Culture);
        Assert.NotNull(name);
        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(22, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(_stage.GetNameAsync(TestData.Culture).GetAwaiter().GetResult(), name);
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
            Assert.NotNull(name);
            Assert.False(string.IsNullOrEmpty(name));
            Assert.Equal(22, _sportEntityFactory.ProfileMemoryCache.Count());
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.TotalRestCalls);
            Assert.Equal(numberOfCalls, _sportEntityFactory.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(_stage.GetNameAsync(cultureInfo).GetAwaiter().GetResult(), name);
        }
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
        Assert.Equal(_match.GetHomeCompetitorAsync().GetAwaiter().GetResult().GetName(TestData.Culture), name);
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
        Assert.Equal(_match.GetAwayCompetitorAsync().GetAwaiter().GetResult().GetName(TestData.Culture), name);
    }

    [Fact]
    public void MatchNameWrongPlayerOperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        Assert.Throws<NameExpressionException>(() => name = new EntityNameExpression("player", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture).GetAwaiter().GetResult());
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public void MatchNameWrongCompetitorOperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        Assert.Throws<NameExpressionException>(() => name = new EntityNameExpression("competitor", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture).GetAwaiter().GetResult());
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public void MatchNameWrongCompetitor0OperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        Assert.Throws<NameExpressionException>(() => name = new EntityNameExpression("competitor0", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture).GetAwaiter().GetResult());
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public void MatchNameWrongCompetitor3OperandDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        Assert.Throws<NameExpressionException>(() => name = new EntityNameExpression("competitor3", _match, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture).GetAwaiter().GetResult());
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public void TournamentNameCompetitor1ForDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        Assert.Throws<NameExpressionException>(() => name = new EntityNameExpression("competitor1", _tournament, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture).GetAwaiter().GetResult());
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }

    [Fact]
    public void TournamentNameCompetitor2ForDefaultCultureThrows()
    {
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);

        string name = null;
        Assert.Throws<NameExpressionException>(() => name = new EntityNameExpression("competitor2", _tournament, _sportEntityFactory.ProfileCache).BuildNameAsync(TestData.Culture).GetAwaiter().GetResult());
        Assert.Null(name);
        Assert.Equal(0, _sportEntityFactory.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactory.DataRouterManager.TotalRestCalls);
    }
}
