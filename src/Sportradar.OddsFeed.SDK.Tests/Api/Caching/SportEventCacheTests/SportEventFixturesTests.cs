// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.API.Caching.SportEventCacheTests;

public class SportEventFixturesTests : SportEventCacheSetup
{
    private const string LugasReferenceKey = "lugas";
    private const string LugasReferenceValue = "123456789";
    private readonly Uri _match1EnFixtureUri;

    public SportEventFixturesTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _match1EnFixtureUri = new Uri($"{TestConfig.Api.BaseUrl}/v1/sports/{TestConsts.CultureEn.TwoLetterISOLanguageName}/sport_events/{TestConsts.AnyMatchId}/fixture.xml");
    }

    [Fact]
    public async Task WhenLugasReferenceIsInFixtureThenItIsAddedToReferencesDictionary()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(TestConsts.AnyMatchId.ToString(), LugasReferenceKey, LugasReferenceValue);

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);

        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldContainKey(LugasReferenceKey);
        matchFixtures.References.References[LugasReferenceKey].ShouldBe(LugasReferenceValue);
    }

    [Fact]
    public async Task WhenLugasReferenceIsInFixtureThenFixtureLugasIdIsPopulatedWithTheValueFromReference()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(TestConsts.AnyMatchId.ToString(), LugasReferenceKey, LugasReferenceValue);

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);

        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldNotBeNull();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBe(LugasReferenceValue);
    }

    [Fact]
    public async Task WhenLugasReferenceIsMissingFromFixtureThenItIsNotAddedToReferencesDictionary()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithEmptyReferences(TestConsts.AnyMatchId.ToString());

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);

        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotContainKey(LugasReferenceKey);
    }

    [Fact]
    public async Task WhenLugasReferenceIsMissingFromFixtureThenFixtureLugasIdIsNull()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithEmptyReferences(TestConsts.AnyMatchId.ToString());

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);

        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBeNull();
    }

    [Fact]
    public async Task WhenLugasReferenceIsNotLowercaseThenFixtureLugasIdIsNull()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(TestConsts.AnyMatchId.ToString(), LugasReferenceKey.ToUpperInvariant(), LugasReferenceValue);

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (IMatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);

        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBeNull();
    }

    [Fact]
    public async Task WhenLugasReferenceIsInFixtureThenExportAndReimportPreservesLugasReference()
    {
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithReference(TestConsts.AnyMatchId.ToString(), LugasReferenceKey, LugasReferenceValue);

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (MatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);
        await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);
        await ExportAndReimportSportEvent(sportEvent);

        sportEvent = (MatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);
        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        FixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
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
        var fixtureWithLugasReference = FixtureEndpoint.GetFixtureWithEmptyReferences(TestConsts.AnyMatchId.ToString());

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixtureWithLugasReference));

        var sportEvent = (MatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);
        await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);
        await ExportAndReimportSportEvent(sportEvent);

        sportEvent = (MatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);
        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        FixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldNotContainKey(LugasReferenceKey);
        matchFixtures.References.ShouldBeAssignableTo<IReferenceV1>();
        ((IReferenceV1)matchFixtures.References).LugasId.ShouldBeNull();
    }

    [Fact]
    public async Task WhenReferenceIdsIsMissingFromFixtureThenResultingReferenceIdsIsNull()
    {
        var fixturesEndpoint = FixtureEndpoint.Raw(TestConsts.AnyMatchId.ToString());

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixturesEndpoint));

        var sportEvent = (MatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);
        await sportEvent!.GetFixtureAsync(TestData.Cultures1);
        await ExportAndReimportSportEvent(sportEvent);

        var matchFixtures = await sportEvent!.GetFixtureAsync(TestData.Cultures1);

        FixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
        matchFixtures.References.ShouldBeNull();
    }

    [Fact]
    public async Task WhenReferenceIdsIsEmptyThenResultingReferenceIdsCollectionIsEmpty()
    {
        var fixturesEndpoint = FixtureEndpoint.GetFixtureWithEmptyReferences(TestConsts.AnyMatchId.ToString());

        FixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(_match1EnFixtureUri))
                          .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(fixturesEndpoint));

        var sportEvent = (MatchCacheItem)SportEventCache.GetEventCacheItem(TestConsts.AnyMatchId);
        await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);
        await ExportAndReimportSportEvent(sportEvent);

        var matchFixtures = await sportEvent!.GetFixtureAsync([TestConsts.CultureEn]);

        FixtureFetcherMock.Verify(fixtureFetcher => fixtureFetcher.GetDataAsync(_match1EnFixtureUri), Times.Once);
        matchFixtures.References.ShouldNotBeNull();
        matchFixtures.References.References.ShouldBeEmpty();
    }

    private async Task ExportAndReimportSportEvent(MatchCacheItem sportEvent)
    {
        var exportedSportEvent = await sportEvent.ExportAsync();
        SportEventCache.CacheDeleteItem(TestConsts.AnyMatchId, CacheItemType.SportEvent);
        await SportEventCache.ImportAsync([exportedSportEvent]);
    }
}
