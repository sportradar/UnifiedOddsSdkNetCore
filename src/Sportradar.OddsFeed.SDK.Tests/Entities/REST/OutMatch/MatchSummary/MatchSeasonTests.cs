// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
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
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.OutMatch.MatchSummary;

public class MatchSeasonTests
{
    private readonly IUofConfiguration _uofConfiguration;
    private readonly ILoggerFactory _loggerFactory;

    public MatchSeasonTests(ITestOutputHelper testOutputHelper)
    {
        _uofConfiguration = UofConfigurations.SingleLanguage;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);
    }

    [Fact]
    public async Task GetSeasonWhenSummaryRequestFailsWithCatchStrategyReturnsNull()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateFactoryWithFailingSummaryFetcherFor(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Catch);

        var season = await match.GetSeasonAsync();
        season.ShouldBeNull();
    }

    [Fact]
    public async Task GetSeasonWhenSummaryRequestFailsWithThrowStrategyThrowsException()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateFactoryWithFailingSummaryFetcherFor(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        await Should.ThrowAsync<CommunicationException>(async () => await match.GetSeasonAsync());
    }

    [Fact]
    public async Task GetSeasonWhenSummaryContainsValidSeasonReturnsCorrectSeasonId()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var season = await match.GetSeasonAsync();

        season.Id.ToString().ShouldBe(matchSummary.sport_event.season.id);
    }

    [Fact]
    public async Task GetSeasonWhenSummaryContainsEndDateAndTimeReturnsCorrectEndDate()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var season = await match.GetSeasonAsync();

        var endTime = TimeOnly.FromTimeSpan(matchSummary.sport_event.season.end_time.TimeOfDay);
        var fullEndDateTime = matchSummary.sport_event.season.end_date.AddTime(endTime);
        season.EndDate.ShouldBe(fullEndDateTime);
    }

    [Fact]
    public async Task GetSeasonWhenSummaryContainsStartDateAndTimeReturnsCorrectStartDate()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var season = await match.GetSeasonAsync();

        var startTime = TimeOnly.FromTimeSpan(matchSummary.sport_event.season.start_time.TimeOfDay);
        var fullStartDateTime = matchSummary.sport_event.season.start_date.AddTime(startTime);
        season.StartDate.ShouldBe(fullStartDateTime);
    }

    [Fact]
    public async Task GetSeasonWhenSummaryContainsValidNameReturnsCorrectSeasonName()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var season = await match.GetSeasonAsync();

        season.Names.ShouldContainKey(_uofConfiguration.DefaultLanguage);
        season.Names[_uofConfiguration.DefaultLanguage].ShouldBe(matchSummary.sport_event.season.name);
    }

    [Fact]
    public async Task GetSeasonWhenSummaryContainsTournamentIdReturnsCorrectTournamentId()
    {
        var matchSummary = Soccer.SummaryFull();
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var season = await match.GetSeasonAsync();

        season.TournamentId.ToString().ShouldBe(matchSummary.sport_event.season.tournament_id);
    }

    [Theory]
    [InlineData("2025")]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetSeasonWhenSummaryContainsYearReturnsCorrectYear(string year)
    {
        var matchSummary = Soccer.SummaryFull();
        matchSummary.sport_event.season.year = year;
        var matchId = matchSummary.GetMatchId();
        var sportId = matchSummary.GetSportId();
        var factory = CreateSummaryFactoryReturning(matchSummary);
        var match = factory.BuildSportEvent<IMatch>(matchId, sportId, [_uofConfiguration.DefaultLanguage], ExceptionHandlingStrategy.Throw);

        var season = await match.GetSeasonAsync();

        season.Year.ShouldBe(year);
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

    private ISportEntityFactory BuildSportEntityFactory(IDataRouterManager dataRouterManager, ICacheManager cacheManager, ILoggerFactory loggerFactory)
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
