// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.API.Caching.SportEventCacheTests;

public class SportEventCacheSetup
{
    internal readonly UofConfiguration TestConfig = (UofConfiguration)TestConfiguration.GetConfig();

    internal readonly Mock<IDataFetcher> SummaryFetcherMock;
    internal readonly Mock<IDataFetcher> FixtureFetcherMock;
    internal readonly ICacheStore<string> SportEventCacheStore;

    internal readonly SportEventCache SportEventCache;

    internal readonly ISportEventCacheItemFactory SportEventCacheItemFactory;

    protected SportEventCacheSetup(ITestOutputHelper outputHelper)
    {
        var converter = new OutputConverter(outputHelper);
        ILoggerFactory loggerFactory = new XunitLoggerFactory(outputHelper);
        Console.SetOut(converter);

        SummaryFetcherMock = new Mock<IDataFetcher>();
        FixtureFetcherMock = new Mock<IDataFetcher>();

        var cacheManager = new CacheManager();
        var dataRouterManager = CreateMockDataRouterManager(cacheManager, SummaryFetcherMock.Object, FixtureFetcherMock.Object);
        SportEventCacheItemFactory = CreateSportEventCacheItemFactory(loggerFactory, dataRouterManager, TestConfig);

        SportEventCacheStore = CreateCacheStore<string>(loggerFactory, UofSdkBootstrap.CacheStoreNameForSportEventCache);
        SportEventCache = CreateSportEventCache(loggerFactory, dataRouterManager, cacheManager, TestConfig, SportEventCacheStore);
    }

    private static CacheStore<T> CreateCacheStore<T>(ILoggerFactory loggerFactory, string cacheStoreName)
    {
        return new CacheStore<T>(cacheStoreName, new MemoryCache(new MemoryCacheOptions()), loggerFactory.CreateLogger(cacheStoreName), TimeSpan.FromMinutes(10));
    }

    private static SportEventCache CreateSportEventCache(ILoggerFactory loggerFactory, IDataRouterManager dataRouterManager, ICacheManager cacheManager, UofConfiguration config, ICacheStore<string> sportEventCacheStore)
    {
        var fixturesCacheStore = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache,
                                                        new MemoryCache(new MemoryCacheOptions()),
                                                        loggerFactory.CreateLogger(typeof(CacheStore<string>)),
                                                        TimeSpan.FromMinutes(10));
        var sportEventCacheItemFactory = new SportEventCacheItemFactory(dataRouterManager,
                                                                        new SemaphorePool(5, ExceptionHandlingStrategy.Throw),
                                                                        config,
                                                                        fixturesCacheStore);
        return new SportEventCache(sportEventCacheStore,
                                   dataRouterManager,
                                   sportEventCacheItemFactory,
                                   new Mock<ISdkTimer>().Object,
                                   config.Languages,
                                   cacheManager,
                                   loggerFactory);
    }

    private static SportEventCacheItemFactory CreateSportEventCacheItemFactory(ILoggerFactory loggerFactory, IDataRouterManager dataRouterManager, UofConfiguration config)
    {
        var fixturesCacheStore = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache,
                                                        new MemoryCache(new MemoryCacheOptions()),
                                                        loggerFactory.CreateLogger(typeof(CacheStore<string>)),
                                                        TimeSpan.FromMinutes(10));
        return new SportEventCacheItemFactory(dataRouterManager,
                                              new SemaphorePool(5, ExceptionHandlingStrategy.Throw),
                                              config,
                                              fixturesCacheStore);
    }

    private DataRouterManager CreateMockDataRouterManager(CacheManager cacheManager, IDataFetcher summaryDataFetcherFastFailing, IDataFetcher fixtureDataFetcher)
    {
        var fixturesDataProvider = new DataProvider<fixturesEndpoint, FixtureDto>($"{TestConfig.Api.BaseUrl}/v1/sports/{{1}}/sport_events/{{0}}/fixture.xml",
                                                                                  fixtureDataFetcher,
                                                                                  new Deserializer<fixturesEndpoint>(),
                                                                                  new FixtureMapperFactory());

        var summaryEndpoint = $"{TestConfig.Api.BaseUrl}/v1/sports/{{1}}/sport_events/{{0}}/summary.xml";
        var matchSummaryDataProvider = new DataProvider<RestMessage, SportEventSummaryDto>(summaryEndpoint,
                                                                                           summaryDataFetcherFastFailing,
                                                                                           new Deserializer<RestMessage>(),
                                                                                           new SportEventSummaryMapperFactory());
        var executionPathDataProvider = new SDK.Entities.Rest.Internal.ExecutionPathDataProvider<SportEventSummaryDto>(matchSummaryDataProvider, new Mock<IDataProvider<SportEventSummaryDto>>().Object);

        return new DataRouterManagerBuilder()
              .AddMockedDependencies()
              .WithCacheManager(cacheManager)
              .WithSportEventFixtureProvider(fixturesDataProvider)
              .WithSportEventSummaryProvider(executionPathDataProvider)
              .Build();
    }

    protected void SetupSummaryEndpointReturning(matchSummaryEndpoint matchSummary)
    {
        foreach (var language in TestConfig.Languages)
        {
            var summaryUri = matchSummary.GetMatchSummaryUri(TestConfig.Api.BaseUrl, language);
            SummaryFetcherMock.Setup(fetcher => fetcher.GetDataAsync(summaryUri))
                              .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(matchSummary));
        }
    }

    protected void SetupFixtureEndpointReturning(fixturesEndpoint fixturesEndpoint)
    {
        foreach (var language in TestConfig.Languages)
        {
            var fixtureUri = fixturesEndpoint.GetFixtureUri(TestConfig.Api.BaseUrl, language);
            FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(fixtureUri))
                              .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixturesEndpoint));
        }
    }
}
