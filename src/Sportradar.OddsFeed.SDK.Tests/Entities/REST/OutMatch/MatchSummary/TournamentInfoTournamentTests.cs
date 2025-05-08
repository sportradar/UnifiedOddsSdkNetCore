// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.OutMatch.MatchSummary;

public class TournamentInfoTournamentTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IUofConfiguration _uofConfiguration;

    public TournamentInfoTournamentTests(ITestOutputHelper testOutputHelper)
    {
        _uofConfiguration = UofConfigurations.SingleLanguage;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);
    }

    [Fact]
    public void TournamentIdWhenSummaryContainsValidTournamentInfoReturnsCorrectId()
    {
        var tournamentInfo = TournamentInfoEndpoints.GetVolleyballWorldChampionshipWomenTournamentInfo();
        var tournamentId = tournamentInfo.GetTournamentId();
        var sportId = tournamentInfo.GetSportId();
        var sportEntityFactory = GetSportEntityFactory(new Mock<IDataFetcher>().Object, _loggerFactory);

        var tournament = sportEntityFactory.BuildSportEvent<ITournament>(tournamentId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Catch);

        tournament.Id.ShouldBe(tournamentId);
    }

    [Fact]
    public async Task GetNameWhenSummaryRequestFailsAndExceptionHandlingStrategyIsCatchReturnsNull()
    {
        var tournamentInfo = TournamentInfoEndpoints.GetVolleyballWorldChampionshipWomenTournamentInfo();
        var tournamentId = tournamentInfo.GetTournamentId();
        var sportId = tournamentInfo.GetSportId();
        var sportEventUri = tournamentInfo.GetTournamentSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var defaultLanguage = _uofConfiguration.DefaultLanguage;
        var failingFetcher = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationException(sportEventUri);
        var sportEntityFactory = GetSportEntityFactory(failingFetcher, _loggerFactory);

        var tournament = sportEntityFactory.BuildSportEvent<ITournament>(tournamentId, sportId, [defaultLanguage], ExceptionHandlingStrategy.Catch);

        var tournamentName = await tournament.GetNameAsync(defaultLanguage);

        tournamentName.ShouldBeNull();
    }

    [Fact]
    public async Task GetNameWhenSummaryRequestFailsAndExceptionHandlingStrategyIsThrowThrowsException()
    {
        var tournamentInfo = TournamentInfoEndpoints.GetVolleyballWorldChampionshipWomenTournamentInfo();
        var tournamentId = tournamentInfo.GetTournamentId();
        var sportId = tournamentInfo.GetSportId();
        var sportEventUri = tournamentInfo.GetTournamentSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var defaultLanguage = _uofConfiguration.DefaultLanguage;
        var failingFetcher = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationException(sportEventUri);
        var sportEntityFactory = GetSportEntityFactory(failingFetcher, _loggerFactory);

        var tournament = sportEntityFactory.BuildSportEvent<ITournament>(tournamentId, sportId, [defaultLanguage], ExceptionHandlingStrategy.Throw);

        await Should.ThrowAsync<CommunicationException>(async () => await tournament.GetNameAsync(defaultLanguage));
    }

    [Fact]
    public async Task GetNameWhenSummaryContainsValidTournamentInfoReturnsCorrectName()
    {
        var tournamentInfo = TournamentInfoEndpoints.GetVolleyballWorldChampionshipWomenTournamentInfo();
        var tournamentId = tournamentInfo.GetTournamentId();
        var sportId = tournamentInfo.GetSportId();
        var sportEventUri = tournamentInfo.GetTournamentSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var defaultLanguage = _uofConfiguration.DefaultLanguage;
        var dataFetcherMock = DataFetcherMockHelper.GetDataFetcherProvidingSummary(tournamentInfo, sportEventUri);
        var sportEntityFactory = GetSportEntityFactory(dataFetcherMock, _loggerFactory);

        var tournament = sportEntityFactory.BuildSportEvent<ITournament>(tournamentId, sportId, [defaultLanguage], ExceptionHandlingStrategy.Catch);

        var tournamentName = await tournament.GetNameAsync(defaultLanguage);

        tournamentName.ShouldBe(tournamentInfo.tournament.name);
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

    private SportEntityFactory GetSportEntityFactory(IDataFetcher dataFetcher, ILoggerFactory loggerFactory)
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
