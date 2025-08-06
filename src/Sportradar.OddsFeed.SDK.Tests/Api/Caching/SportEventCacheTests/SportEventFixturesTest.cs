// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.API.Caching.SportEventCacheTests;

public class SportEventFixturesTest
{
    private const string MatchId = "sr:match:1";
    private const string BaseApiHost = "http://localhost";
    private const string LugasReferenceKey = "lugas";
    private const string LugasReferenceValue = "123456789";
    private readonly CultureInfo _en;

    private readonly ISportEventCache _sportEventCache;
    private readonly Mock<IDataFetcher> _fixtureFetcherMock;
    private readonly Uri _match1EnFixtureUri;

    public SportEventFixturesTest(ITestOutputHelper outputHelper)
    {
        _en = new CultureInfo("en");
        _match1EnFixtureUri = new Uri($"{BaseApiHost}/v1/sports/{_en.TwoLetterISOLanguageName}/sport_events/{MatchId}/fixture.xml");
        _fixtureFetcherMock = new Mock<IDataFetcher>();
        var loggerFactory = new XunitLoggerFactory(outputHelper);
        var cacheManager = new CacheManager();
        var dataRouterManager = CreateMockDataRouterManager(cacheManager, _fixtureFetcherMock.Object);
        _sportEventCache = CreateSportEventCache(loggerFactory, dataRouterManager, cacheManager);
    }

    [Fact]
    public async Task WhenLugasReferenceIsInFixtureThenItIsAddedToReferencesDictionary()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(MatchId, LugasReferenceKey, LugasReferenceValue);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));

        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldContainKey(LugasReferenceKey);
        matchFixtures.References.References[LugasReferenceKey].ShouldBe(LugasReferenceValue);
    }

    [Fact]
    public async Task WhenLugasReferenceIsInFixtureThenFixtureLugasIdIsPopulatedWithTheValueFromReference()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(MatchId, LugasReferenceKey, LugasReferenceValue);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));

        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldNotBeNull();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBe(LugasReferenceValue);
    }

    [Fact]
    public async Task WhenLugasReferenceIsMissingFromFixtureThenItIsNotAddedToReferencesDictionary()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithEmptyReferences(MatchId);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));

        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotContainKey(LugasReferenceKey);
    }

    [Fact]
    public async Task WhenLugasReferenceIsMissingFromFixtureThenFixtureLugasIdIsNull()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithEmptyReferences(MatchId);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));

        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBeNull();
    }

    [Fact]
    public async Task WhenLugasReferenceIsNotLowercaseThenFixtureLugasIdIsNull()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(MatchId, LugasReferenceKey.ToUpperInvariant(), LugasReferenceValue);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));

        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBeNull();
    }

    [Fact]
    public async Task WhenLugasReferenceIsInFixtureThenExportAndReimportPreservesLugasReference()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(MatchId, LugasReferenceKey, LugasReferenceValue);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (MatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));
        await sportEvent!.GetFixtureAsync([_en]);
        await ExportAndReimportSportEvent(sportEvent);

        sportEvent = (MatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));
        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        _fixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldContainKey(LugasReferenceKey);
        matchFixtures.References.References[LugasReferenceKey].ShouldBe(LugasReferenceValue);
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldNotBeNull();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBe(LugasReferenceValue);
    }

    [Fact]
    public async Task WhenLugasReferenceIsMissingFromFixtureThenExportAndReimportPreservesEmptyLugasReference()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithEmptyReferences(MatchId);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (MatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));
        await sportEvent!.GetFixtureAsync([_en]);
        await ExportAndReimportSportEvent(sportEvent);

        sportEvent = (MatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));
        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        _fixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotContainKey(LugasReferenceKey);
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBeNull();
    }

    [Fact]
    public async Task WhenReferenceIdsIsMissingFromFixtureThenResultingReferenceIdsIsNull()
    {
        var fixturesEndpoint = FixtureEndpoint.Raw(MatchId);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixturesEndpoint));

        var sportEvent = (MatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));
        await sportEvent!.GetFixtureAsync(TestData.Cultures1);
        await ExportAndReimportSportEvent(sportEvent);

        var matchFixtures = await sportEvent!.GetFixtureAsync(TestData.Cultures1);

        _fixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
        matchFixtures.References.ShouldBeNull();
    }

    [Fact]
    public async Task WhenReferenceIdsIsEmptyThenResultingReferenceIdsCollectionIsEmpty()
    {
        var fixturesEndpoint = FixtureEndpoint.GetFixtureWithEmptyReferences(MatchId);

        _fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                           .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixturesEndpoint));

        var sportEvent = (MatchCacheItem)_sportEventCache.GetEventCacheItem(Urn.Parse(MatchId));
        await sportEvent!.GetFixtureAsync([_en]);
        await ExportAndReimportSportEvent(sportEvent);

        var matchFixtures = await sportEvent!.GetFixtureAsync([_en]);

        _fixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldBeEmpty();
    }

    private async Task ExportAndReimportSportEvent(MatchCacheItem sportEvent)
    {
        var exportedSportEvent = await sportEvent.ExportAsync();
        _sportEventCache.CacheDeleteItem(Urn.Parse(MatchId), CacheItemType.SportEvent);
        await _sportEventCache.ImportAsync([exportedSportEvent]);
    }

    private SportEventCache CreateSportEventCache(ILoggerFactory loggerFactory, IDataRouterManager dataRouterManager, ICacheManager cacheManager)
    {
        var cacheStore = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache, new MemoryCache(new MemoryCacheOptions()), loggerFactory.CreateLogger(typeof(CacheStore<string>)), TimeSpan.FromMinutes(10));
        var fixturesCacheStore = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache,
                                                        new MemoryCache(new MemoryCacheOptions()),
                                                        loggerFactory.CreateLogger(typeof(CacheStore<string>)),
                                                        TimeSpan.FromMinutes(10));
        var sportEventCacheItemFactory = new SportEventCacheItemFactory(
                                                                        dataRouterManager,
                                                                        new SemaphorePool(5, ExceptionHandlingStrategy.Throw),
                                                                        TestConfiguration.GetConfig(),
                                                                        fixturesCacheStore);
        return new SportEventCache(cacheStore,
                                   dataRouterManager,
                                   sportEventCacheItemFactory,
                                   new Mock<ISdkTimer>().Object,
                                       [_en],
                                   cacheManager,
                                   loggerFactory);
    }

    private static DataRouterManager CreateMockDataRouterManager(CacheManager cacheManager, IDataFetcher dataFetcher)
    {
        var fixturesDataProvider = new DataProvider<fixturesEndpoint, FixtureDto>($"{BaseApiHost}/v1/sports/{{1}}/sport_events/{{0}}/fixture.xml",
                                                                                  dataFetcher,
                                                                                  new Deserializer<fixturesEndpoint>(),
                                                                                  new FixtureMapperFactory());

        return new DataRouterManagerBuilder()
                               .AddMockedDependencies()
                               .WithCacheManager(cacheManager)
                               .WithSportEventFixtureProvider(fixturesDataProvider)
                               .Build();
    }
}
