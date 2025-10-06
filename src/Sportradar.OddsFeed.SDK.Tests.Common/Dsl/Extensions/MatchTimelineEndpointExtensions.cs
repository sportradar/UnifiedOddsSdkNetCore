// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using static Sportradar.OddsFeed.SDK.Tests.Common.Helpers.DataFetcherMockHelper;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;

public static class MatchTimelineEndpointExtensions
{
    internal static ISportEventCache CreateSportEventCacheWithTimeline(this matchTimelineEndpoint matchTimelineEndpoint, IUofConfiguration uofConfiguration, ILoggerFactory loggerFactory)
    {
        var cacheManager = new CacheManager();

        var matchTimelineDataProvider = GetDataProviderReturning(matchTimelineEndpoint, uofConfiguration);
        var dataRouterManager = new DataRouterManagerBuilder()
                               .AddMockedDependencies()
                               .WithConfiguration(uofConfiguration)
                               .WithOngoingSportEventProvider(matchTimelineDataProvider)
                               .WithCacheManager(cacheManager)
                               .Build();
        var cacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache, loggerFactory);
        var sportEventCacheItemFactory = CreateSportEventCacheItemFactory(dataRouterManager, uofConfiguration, loggerFactory);
        return new SportEventCacheBuilder().WithAllMockedDependencies()
                                           .WithDataRouterManager(dataRouterManager)
                                           .WithCacheManager(cacheManager)
                                           .WithCache(cacheStore)
                                           .WithCacheItemFactory(sportEventCacheItemFactory)
                                           .WithLoggerFactory(loggerFactory)
                                           .WithLanguages([uofConfiguration.DefaultLanguage])
                                           .Build();
    }

    private static SportEventCacheItemFactory CreateSportEventCacheItemFactory(DataRouterManager dataRouterManager,
        IUofConfiguration uofConfiguration,
        ILoggerFactory loggerFactory)
    {
        var fixtureCacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache, loggerFactory);
        var semaphorePool = new SemaphorePool(10, ExceptionHandlingStrategy.Throw, loggerFactory.CreateLogger<SemaphorePool>());
        var sportEventCacheItemFactory = new SportEventCacheItemFactory(dataRouterManager,
                                                                        semaphorePool,
                                                                        uofConfiguration,
                                                                        fixtureCacheStore);
        return sportEventCacheItemFactory;
    }

    private static DataProvider<matchTimelineEndpoint, MatchTimelineDto> GetDataProviderReturning(matchTimelineEndpoint matchTimelineEndpoint, IUofConfiguration uofConfiguration)
    {
        var matchId = matchTimelineEndpoint.sport_event.id.ToUrn();
        var matchTimelineUrl = new Uri($"{uofConfiguration.Api.BaseUrl}/v1/sports/{uofConfiguration.DefaultLanguage.TwoLetterISOLanguageName}/sport_events/{matchId}/timeline.xml");
        var timelineDataFetcherMock = GetDataFetcherProvidingSummary(matchTimelineEndpoint, matchTimelineUrl);
        var timelineEndpointFormattedUrl = $"{uofConfiguration.Api.BaseUrl}/v1/sports/{{1}}/sport_events/{{0}}/timeline.xml";
        var matchTimelineDataProvider = new DataProvider<matchTimelineEndpoint, MatchTimelineDto>(timelineEndpointFormattedUrl,
                                                                                                  timelineDataFetcherMock.Object,
                                                                                                  new Deserializer<matchTimelineEndpoint>(),
                                                                                                  new MatchTimelineMapperFactory());
        return matchTimelineDataProvider;
    }
}
