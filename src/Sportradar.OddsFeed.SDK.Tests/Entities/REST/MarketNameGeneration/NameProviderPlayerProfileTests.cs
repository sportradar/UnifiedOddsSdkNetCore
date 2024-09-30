// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class NameProviderPlayerProfileTests
{
    private readonly INameProvider _nameProvider;
    private readonly TestSportEntityFactoryBuilder _sportEntityFactoryBuilder;
    private readonly ScheduleData _scheduleData;
    private readonly ILogger _namedProviderLogger;

    public NameProviderPlayerProfileTests(ITestOutputHelper outputHelper)
    {
        _namedProviderLogger = new XUnitLogger("NameProvider", outputHelper);
        _scheduleData = new ScheduleData(new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3), outputHelper);
        _sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3);
        var match = _sportEntityFactoryBuilder.SportEntityFactory.BuildSportEvent<IMatch>(ScheduleData.MatchId, ScheduleData.MatchSportId, ScheduleData.Cultures3, _sportEntityFactoryBuilder.ThrowingStrategy);
        _nameProvider = new NameProvider(new Mock<IMarketCacheProvider>().Object,
            _sportEntityFactoryBuilder.ProfileCache,
            new Mock<INameExpressionFactory>().Object,
            match,
            1,
            null,
            ExceptionHandlingStrategy.Throw,
            _namedProviderLogger);
    }

    [Fact]
    public async Task OutcomeIdWithWrongPlayerIdThrows()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        await Assert.ThrowsAsync<NameGenerationException>(async () => await _nameProvider.GetOutcomeNameAsync("sr:player2", ScheduleData.CultureEn));
        await Assert.ThrowsAsync<NameGenerationException>(async () => await _nameProvider.GetOutcomeNameAsync("sr:customplayer:2", ScheduleData.CultureEn));
    }

    [Fact]
    public async Task CompositeOutcomeIdWithWrongDelimiterThrows()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        await Assert.ThrowsAsync<NameGenerationException>(async () => await _nameProvider.GetOutcomeNameAsync("sr:player:2, sr:competitor:1", ScheduleData.CultureEn));
        await Assert.ThrowsAsync<NameGenerationException>(async () => await _nameProvider.GetOutcomeNameAsync("sr:player:2;sr:competitor:1", ScheduleData.CultureEn));
    }

    [Fact]
    public async Task OutcomeIdWithSinglePlayerIdGetsCorrectValue()
    {
        var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId1.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor1Player1.Names[ScheduleData.CultureEn], name);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.PlayerProfileMarketPrefix)));
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompositeOutcomeIdForTwoPlayerIdsSameCompetitorGetsCorrectValue()
    {
        var outcomeId = $"{ScheduleData.MatchCompetitor1PlayerId1},{ScheduleData.MatchCompetitor1PlayerId2}";
        var expectedName = $"{_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn)}";

        var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
        Assert.Equal(expectedName, name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompositeOutcomeIdForTwoPlayerIdsDifferentCompetitorGetsCorrectValue()
    {
        var outcomeId = $"{ScheduleData.MatchCompetitor1PlayerId1},{ScheduleData.MatchCompetitor2PlayerId2}";
        var expectedName = $"{_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor2Player2.GetName(ScheduleData.CultureEn)}";

        var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
        Assert.Equal(expectedName, name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompositeOutcomeIdForTwoCompetitorIdsGetsCorrectValue()
    {
        var outcomeId = $"{ScheduleData.MatchCompetitorId1},{ScheduleData.MatchCompetitorId2}";
        var expectedName = $"{_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor2.GetName(ScheduleData.CultureEn)}";

        var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
        Assert.Equal(expectedName, name);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task PreloadedPlayerDataAndOutcomeIdWithSinglePlayerIdGetsCorrectValue()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetPlayerProfileAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.CultureEn, null);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

        var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId1.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task PreloadedCompetitorDataAndOutcomeIdWithSinglePlayerIdGetsCorrectValue()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId1, ScheduleData.CultureEn, null);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

        var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId2.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task WhenMissingDataPlayerProfileIsCalledOnlyOnce()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId2.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor2Player2.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId2.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor2Player2.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompositeOutcomeIdWithCompetitorAndPlayerIsCalledCorrectly()
    {
        var outcomeId = $"{ScheduleData.MatchCompetitorId1},{ScheduleData.MatchCompetitor1PlayerId2}";
        var expectedName = $"{_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn)}";
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
        Assert.Equal(expectedName, name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompositeOutcomeIdWithPlayerAndCompetitorIsCalledCorrectly()
    {
        var outcomeId = $"{ScheduleData.MatchCompetitor1PlayerId2},{ScheduleData.MatchCompetitorId1}";
        var expectedName = $"{_scheduleData.MatchCompetitor1Player2.GetName(ScheduleData.CultureEn)},{_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn)}";
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var name = await _nameProvider.GetOutcomeNameAsync(outcomeId, ScheduleData.CultureEn);
        Assert.Equal(expectedName, name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task WhenNoDataPlayerProfileIsCalledOnlyOncePerCulture()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor1PlayerId2.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1Player2.GetName(cultureInfo), name);
            Assert.False(string.IsNullOrEmpty(name));
        }
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(7, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(6, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompetitorDataIsLoadedFromMatchSummary()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name);
        }
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompetitorProfileIsCalledOnlyOnce()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor2.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor2.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompetitorProfileIsLoadedFromSummaryOnlyOncePerCulture()
    {
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name);
            Assert.False(string.IsNullOrEmpty(name));
        }
        Assert.Equal(2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task PreloadedMatchDataAndOutcomeIdWithSingleCompetitorIdGetsCorrectValue()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureEn, null);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);

        var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task CompetitorProfileIsCalledWhenWantedPlayerProfileLinkedToCompetitor()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetCompetitorAsync(ScheduleData.MatchCompetitorId2, ScheduleData.CultureEn, null);
        Assert.Equal(ScheduleData.MatchCompetitor2PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));

        var name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId1.ToString(), ScheduleData.CultureEn);
        Assert.Equal(_scheduleData.MatchCompetitor2Player1.GetName(ScheduleData.CultureEn), name);
        Assert.Equal(ScheduleData.MatchCompetitor2PlayerCount + 1, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        name = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitor2PlayerId1.ToString(), ScheduleData.CultureHu);
        Assert.Equal(_scheduleData.MatchCompetitor2Player1.GetName(ScheduleData.CultureHu), name);
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + ScheduleData.MatchCompetitor2PlayerCount + 2, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(4, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompetitorDataIsLoadedFromTournamentSummary()
    {
        var tournament = _sportEntityFactoryBuilder.SportEntityFactory.BuildSportEvent<ITournament>(ScheduleData.MatchTournamentId, ScheduleData.MatchSportId, ScheduleData.Cultures3, _sportEntityFactoryBuilder.ThrowingStrategy);
        var nameProvider = new NameProvider(new Mock<IMarketCacheProvider>().Object,
            _sportEntityFactoryBuilder.ProfileCache,
            new Mock<INameExpressionFactory>().Object,
            tournament,
            1,
            null,
            ExceptionHandlingStrategy.Throw,
            _namedProviderLogger);
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
            var name2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
        }
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CompetitorDataIsLoadedFromSeasonSummary()
    {
        var season = _sportEntityFactoryBuilder.SportEntityFactory.BuildSportEvent<ISeason>(ScheduleData.MatchSeasonId, ScheduleData.MatchSportId, ScheduleData.Cultures3, _sportEntityFactoryBuilder.ThrowingStrategy);
        var nameProvider = new NameProvider(new Mock<IMarketCacheProvider>().Object,
            _sportEntityFactoryBuilder.ProfileCache,
            new Mock<INameExpressionFactory>().Object,
            season,
            1,
            null,
            ExceptionHandlingStrategy.Throw,
            _namedProviderLogger);
        Assert.Equal(0, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name1 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
            var name2 = await nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
        }
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task MatchDataOverridesCompetitorAssociatedEventIdOverTournamentData()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchTournamentId, ScheduleData.CultureEn, null);
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null);
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
            var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
        }
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("tournament")));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task TournamentDataDoesNotOverridesCompetitorAssociatedEventIdOverMatchData()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null);
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchTournamentId, ScheduleData.CultureEn, null);
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
            var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
        }
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("tournament")));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task MatchDataOverridesCompetitorAssociatedEventIdOverSeasonData()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchSeasonId, ScheduleData.CultureEn, null);
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null);
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
            var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
        }
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("season")));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task SeasonDataDoesNotOverridesCompetitorAssociatedEventIdOverMatchData()
    {
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureDe, null);
        await _sportEntityFactoryBuilder.DataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchSeasonId, ScheduleData.CultureEn, null);
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        foreach (var cultureInfo in ScheduleData.Cultures3)
        {
            var name1 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId1.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor1.GetName(cultureInfo), name1);
            var name2 = await _nameProvider.GetOutcomeNameAsync(ScheduleData.MatchCompetitorId2.ToString(), cultureInfo);
            Assert.Equal(_scheduleData.MatchCompetitor2.GetName(cultureInfo), name2);
        }
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.Count());
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _sportEntityFactoryBuilder.ProfileMemoryCache.GetKeys().Count(c => c.Contains(SdkInfo.CompetitorProfileMarketPrefix)));
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.TotalRestCalls);
        Assert.Equal(3, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("season")));
        Assert.Equal(2, _sportEntityFactoryBuilder.DataRouterManager.RestUrlCalls.Count(c => c.Contains("match")));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _sportEntityFactoryBuilder.DataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }
}
