// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
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
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.OutMatch.MatchSummary;

public class MatchBasicPropertiesTests
{
    private readonly IUofConfiguration _uofConfiguration;
    private readonly ILoggerFactory _loggerFactory;

    public MatchBasicPropertiesTests(ITestOutputHelper testOutputHelper)
    {
        _uofConfiguration = UofConfigurations.SingleLanguage;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);
    }

    [Fact]
    public void IdShouldBeCorrectForMatch()
    {
        var iceHokeyMatchSummary = IceHockey.Summary();
        var matchId = iceHokeyMatchSummary.GetMatchId();
        var sportId = iceHokeyMatchSummary.GetSportId();
        var sportEntityFactory = GetSportEntityFactory(new Mock<IDataFetcher>().Object, _loggerFactory);

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Catch);

        match.Id.ShouldBe(matchId);
    }

    [Fact]
    public async Task ToBeDeterminedReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var iceHokeyMatchSummary = IceHockey.Summary();
        var matchId = iceHokeyMatchSummary.GetMatchId();
        var sportId = iceHokeyMatchSummary.GetSportId();

        var matchSummaryUri = iceHokeyMatchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var dataFetcherMock = DataFetcherMockHelper.GetDataFetcherProvidingSummary(iceHokeyMatchSummary, matchSummaryUri);
        var sportEntityFactory = GetSportEntityFactory(dataFetcherMock, _loggerFactory);

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Catch);
        var matchStartDateToBeDetermined = await match.GetStartTimeTbdAsync();

        matchStartDateToBeDetermined.ShouldNotBeNull();
        matchStartDateToBeDetermined.ShouldBe(iceHokeyMatchSummary.sport_event.start_time_tbd);
    }

    [Fact]
    public async Task ScheduledTimeReflectsCorrectValueProvidedBySummaryEndpoint()
    {
        var iceHokeyMatchSummary = IceHockey.Summary();
        var matchId = iceHokeyMatchSummary.GetMatchId();
        var sportId = iceHokeyMatchSummary.GetSportId();

        var matchSummaryUri = iceHokeyMatchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var dataFetcherMock = DataFetcherMockHelper.GetDataFetcherProvidingSummary(iceHokeyMatchSummary, matchSummaryUri);
        var sportEntityFactory = GetSportEntityFactory(dataFetcherMock, _loggerFactory);

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Catch);
        var scheduledTime = await match.GetScheduledTimeAsync();

        scheduledTime.ShouldNotBeNull();
        scheduledTime.ShouldBe(iceHokeyMatchSummary.sport_event.scheduled);
    }

    [Fact]
    public async Task ToBeDeterminedReturnsNullWhenSummaryRequestFailsAndExceptionHandlingStrategyIsCatch()
    {
        var iceHokeyMatchSummary = IceHockey.Summary();
        var matchId = iceHokeyMatchSummary.GetMatchId();
        var sportId = iceHokeyMatchSummary.GetSportId();

        var matchSummaryUri = iceHokeyMatchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var dataFetcherMock = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationException(matchSummaryUri);
        var sportEntityFactory = GetSportEntityFactory(dataFetcherMock, _loggerFactory);

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Catch);

        bool? matchStartDateToBeDetermined = false;
        await Should.NotThrowAsync(async () => matchStartDateToBeDetermined = await match.GetStartTimeTbdAsync());

        matchStartDateToBeDetermined.ShouldBeNull();
    }

    [Fact]
    public async Task ToBeDeterminedThrowsExceptionWhenSummaryRequestFailsAndExceptionHandlingStrategyIsThrow()
    {
        var iceHokeyMatchSummary = IceHockey.Summary();
        var matchId = iceHokeyMatchSummary.GetMatchId();
        var sportId = iceHokeyMatchSummary.GetSportId();

        var matchSummaryUri = iceHokeyMatchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var dataFetcherMock = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationException(matchSummaryUri);
        var sportEntityFactory = GetSportEntityFactory(dataFetcherMock, _loggerFactory);

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId,
                                                                sportId,
                                                                    [_uofConfiguration.DefaultLanguage],
                                                                ExceptionHandlingStrategy.Throw);

        await Should.ThrowAsync<CommunicationException>(async () => await match.GetStartTimeTbdAsync());
    }

    [Fact]
    public async Task ScheduledTimeWhenSummaryRequestFailsAndExceptionHandlingStrategyIsCatchThenReturnsNull()
    {
        var iceHokeyMatchSummary = IceHockey.Summary();
        var matchId = iceHokeyMatchSummary.GetMatchId();
        var sportId = iceHokeyMatchSummary.GetSportId();

        var matchSummaryUri = iceHokeyMatchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var dataFetcherMock = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationException(matchSummaryUri);
        var sportEntityFactory = GetSportEntityFactory(dataFetcherMock, _loggerFactory);

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId,
                                                               sportId,
                                                                   [_uofConfiguration.DefaultLanguage],
                                                               ExceptionHandlingStrategy.Catch);

        DateTime? scheduledStartTime = DateTime.MinValue;
        await Should.NotThrowAsync(async () => scheduledStartTime = await match.GetScheduledTimeAsync());

        scheduledStartTime.ShouldBeNull();
    }

    [Fact]
    public async Task ScheduledTimeThrowsExceptionWhenSummaryRequestFailsAndExceptionHandlingStrategyIsThrow()
    {
        var iceHokeyMatchSummary = IceHockey.Summary();
        var matchId = iceHokeyMatchSummary.GetMatchId();
        var sportId = iceHokeyMatchSummary.GetSportId();

        var matchSummaryUri = iceHokeyMatchSummary.GetMatchSummaryUri(_uofConfiguration.Api.BaseUrl, _uofConfiguration.DefaultLanguage);
        var dataFetcherMock = DataFetcherMockHelper.GetDataFetcherThrowingCommunicationException(matchSummaryUri);
        var sportEntityFactory = GetSportEntityFactory(dataFetcherMock, _loggerFactory);

        var match = sportEntityFactory.BuildSportEvent<IMatch>(matchId,
                                                               sportId,
                                                                   [_uofConfiguration.DefaultLanguage],
                                                               ExceptionHandlingStrategy.Throw);

        await Should.ThrowAsync<CommunicationException>(async () => await match.GetScheduledTimeAsync());
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
