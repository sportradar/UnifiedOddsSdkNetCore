// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.OutMatch.MatchSummary;

public class MatchTournamentRoundTests
{
    private readonly IUofConfiguration _uofConfiguration;
    private readonly ILoggerFactory _loggerFactory;

    public MatchTournamentRoundTests(ITestOutputHelper testOutputHelper)
    {
        _uofConfiguration = UofConfigurations.SingleLanguage;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);
    }

    [Fact]
    public async Task RoundReturnsNullWhenSummaryRequestFailsAndExceptionHandlingStrategyIsCatch()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateFactoryWithFailingSummaryFetcherFor(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Catch);

        var round = await match.GetTournamentRoundAsync();

        round.ShouldBeNull();
    }

    [Fact]
    public async Task RoundThrowsExceptionWhenSummaryRequestFailsAndExceptionHandlingStrategyIsThrow()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateFactoryWithFailingSummaryFetcherFor(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        await Should.ThrowAsync<CommunicationException>(async () => await match.GetTournamentRoundAsync());
    }

    [Fact]
    public async Task RoundTypeReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        round.Type.ShouldBe(matchSummary.sport_event.tournament_round.type);
    }

    [Fact]
    public async Task RoundCupRoundMatchesReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        round.CupRoundMatches.ShouldBe(matchSummary.sport_event.tournament_round.cup_round_matches);
    }

    [Fact]
    public async Task RoundCupRoundMatchNumberReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        round.CupRoundMatchNumber.ShouldBe(matchSummary.sport_event.tournament_round.cup_round_match_number);
    }

    [Fact]
    public async Task RoundCupRoundMatchNumberIsEmptyWhenNotProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        matchSummary.sport_event.tournament_round.cup_round_match_numberSpecified = false;
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        round.CupRoundMatchNumber.ShouldBeNull();
    }

    [Fact]
    public async Task RoundBetradarIdReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        round.BetradarId.ShouldBe(matchSummary.sport_event.tournament_round.betradar_id);
    }

    [Fact]
    public async Task RoundBetradarNameReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        round.BetradarName.ShouldBe(matchSummary.sport_event.tournament_round.betradar_name);
    }

    [Fact]
    public async Task RoundNameReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        var culture = _uofConfiguration.DefaultLanguage;
        round.GetName(culture).ShouldBe(matchSummary.sport_event.tournament_round.name);
    }

    [Fact]
    public async Task RoundGroupReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        var expectedGroup = matchSummary.sport_event.tournament_round.group;
        round.Group.ShouldBe(expectedGroup);
    }

    [Fact]
    public async Task RoundOtherMatchIdReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        var expectedOtherMatchId = matchSummary.sport_event.tournament_round.other_match_id;
        round.OtherMatchId.ShouldBe(expectedOtherMatchId);
    }

    [Theory]
    [InlineData("GroupLongName", "GroupLongName")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public async Task RoundPhaseOrGroupLongNamesReflectCorrectValueProvidedBySummaryEndpoint(string phaseOrGroupLongName, string expectedValue)
    {
        var matchSummary = Soccer.SummaryFull();
        matchSummary.sport_event.tournament_round.group_long_name = phaseOrGroupLongName;
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        var culture = _uofConfiguration.DefaultLanguage;
        round.PhaseOrGroupLongNames[culture].ShouldBe(expectedValue);
    }

    [Fact]
    public async Task RoundGroupIdReflectCorrectValueProvidedBySummaryEndpoint()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        var expectedGroupId = matchSummary.sport_event.tournament_round.group_id;
        round.GroupId.ToString().ShouldBe(expectedGroupId);
    }

    [Theory]
    [InlineData("phase-test-value")]
    [InlineData("")]
    [InlineData(null)]
    public async Task RoundPhaseReflectCorrectValueProvidedBySummaryEndpoint(string phase)
    {
        var matchSummary = Soccer.SummaryFull();
        matchSummary.sport_event.tournament_round.phase = phase;
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var round = await match.GetTournamentRoundAsync();

        round.Phase.ShouldBe(matchSummary.sport_event.tournament_round.phase);
    }

    private ISportEntityFactory CreateSummaryFactoryReturning(matchSummaryEndpoint matchSummary)
    {
        var summaryUri = matchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var workingFetcher = DataFetcherMockHelper.GetDataFetcherProvidingSummary(matchSummary, summaryUri);

        return GetSportEntityFactory(workingFetcher, _loggerFactory);
    }

    private ISportEntityFactory CreateFactoryWithFailingSummaryFetcherFor(matchSummaryEndpoint matchSummary)
    {
        var summaryUri = matchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var failingFetcher = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationException(summaryUri);

        return GetSportEntityFactory(failingFetcher, _loggerFactory);
    }

    private static DataRouterManager GetDataRouterManager(CacheManager cacheManager, IUofConfiguration uofConfiguration, IDataFetcher dataFetcherFastFailing)
    {
        var summaryEndpoint = $"{uofConfiguration.Api.BaseUrl}/v1/sports/{{1}}/sport_events/{{0}}/summary.xml";
        var matchSummaryDataProvider = new DataProvider<RestMessage, SportEventSummaryDto>(summaryEndpoint,
                                                                                           dataFetcherFastFailing,
                                                                                           new Deserializer<RestMessage>(),
                                                                                           new SportEventSummaryMapperFactory());
        return new DataRouterManagerBuilder()
              .AddMockedDependencies()
              .WithConfiguration(uofConfiguration)
              .WithSportEventSummaryProvider(matchSummaryDataProvider)
              .WithCacheManager(cacheManager)
              .Build();
    }

    private ISportEntityFactory GetSportEntityFactory(IDataFetcher dataFetcher, ILoggerFactory loggerFactory)
    {
        var cacheManager = new CacheManager();

        var dataRouterManager = GetDataRouterManager(cacheManager, _uofConfiguration, dataFetcher);
        return BuildSportEntityFactory(dataRouterManager, cacheManager, loggerFactory);
    }

    private SportEntityFactory BuildSportEntityFactory(IDataRouterManager dataRouterManager, ICacheManager cacheManager, ILoggerFactory loggerFactory)
    {
        var sportEventCache = BuildSportEventCache(dataRouterManager, cacheManager, loggerFactory);

        return new SportEntityFactoryBuilder()
              .WithAllMockedDependencies()
              .WithSportEventCache(sportEventCache)
              .Build();
    }

    private SportEventCache BuildSportEventCache(IDataRouterManager dataRouterManager,
        ICacheManager cacheManager,
        ILoggerFactory loggerFactory)
    {
        var cacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache, loggerFactory);
        var sportEventCacheItemFactory = GetSportEventCacheItemFactory(dataRouterManager, loggerFactory);

        return new SportEventCacheBuilder().WithAllMockedDependencies()
                                           .WithDataRouterManager(dataRouterManager)
                                           .WithCacheManager(cacheManager)
                                           .WithCache(cacheStore)
                                           .WithCacheItemFactory(sportEventCacheItemFactory)
                                           .WithLoggerFactory(loggerFactory)
                                           .WithLanguages([_uofConfiguration.DefaultLanguage])
                                           .Build();
    }

    private SportEventCacheItemFactory GetSportEventCacheItemFactory(IDataRouterManager dataRouterManager, ILoggerFactory loggerFactory)
    {
        var fixtureCacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache, loggerFactory);
        var semaphorePool = new SemaphorePool(10, ExceptionHandlingStrategy.Throw, loggerFactory.CreateLogger<SemaphorePool>());

        return new SportEventCacheItemFactory(dataRouterManager,
                                              semaphorePool,
                                              _uofConfiguration,
                                              fixtureCacheStore);
    }
}
