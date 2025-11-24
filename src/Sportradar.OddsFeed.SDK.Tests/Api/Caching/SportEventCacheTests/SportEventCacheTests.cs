// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ConvertToPrimaryConstructor

namespace Sportradar.OddsFeed.SDK.Tests.API.Caching.SportEventCacheTests;

public abstract class SportEventCacheTests : SportEventCacheSetup
{
    private SportEventCacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    public class WhenGetEventSportIdAsyncAndNotInCache : SportEventCacheTests
    {
        public WhenGetEventSportIdAsyncAndNotInCache(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task ThenEventIsRetrievedFromProviderAndAddedToCache()
        {
            var apiMatch = Soccer.Summary();
            var dtoMatch = new MatchDto(apiMatch);
            SetupSummaryEndpointReturning(apiMatch);

            using (new AssertionScope())
            {
                var sportId = await SportEventCache.GetEventSportIdAsync(dtoMatch.Id);

                // the sport event should be retrieved from the api (because it wasn't in the cache)
                SummaryFetcherMock.Verify(x => x.GetDataAsync(It.IsAny<Uri>()), Times.Once);

                // the sport event should now be in the cache
                SportEventCacheStore.Get(apiMatch.sport_event.id).Should().NotBeNull();

                sportId.Should().BeEquivalentTo(dtoMatch.Tournament.Sport.Id);
            }
        }
    }

    public class WhenGetMatchEventAndNotInCache : SportEventCacheTests
    {
        public WhenGetMatchEventAndNotInCache(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task ThenEventIsRetrievedFromProviderAndAddedToCache()
        {
            var apiMatch = Soccer.Summary();
            var dtoMatch = new MatchDto(apiMatch);
            SetupSummaryEndpointReturning(apiMatch);
            SetupFixtureEndpointReturning(Soccer.Fixture());

            var sportId = await SportEventCache.GetEventSportIdAsync(dtoMatch.Id);
            var match = (MatchCacheItem)SportEventCache.GetEventCacheItem(dtoMatch.Id);

            var fixture = await match.GetFixtureAsync(TestConfig.Languages);

            using (new AssertionScope())
            {
                // the sport event should be retrieved from the api (because it wasn't in the cache)
                SummaryFetcherMock.Verify(x => x.GetDataAsync(It.IsAny<Uri>()), Times.Once);
                FixtureFetcherMock.Verify(x => x.GetDataAsync(It.IsAny<Uri>()), Times.Exactly(TestConfig.Languages.Count));

                // the sport event should now be in the cache
                SportEventCacheStore.Get(apiMatch.sport_event.id).Should().NotBeNull();

                sportId.Should().BeEquivalentTo(dtoMatch.Tournament.Sport.Id);

                fixture.Should().NotBeNull();
            }
        }
    }

    public class WhenGetEventSportIdAsyncAndAlreadyInCache : SportEventCacheTests
    {
        public WhenGetEventSportIdAsyncAndAlreadyInCache(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task ThenEventIsRetrievedFromTheCache()
        {
            var apiMatch = Soccer.Summary();
            var dtoMatch = new MatchDto(apiMatch);
            SetupSummaryEndpointReturning(apiMatch);

            var sportEvent = SportEventCacheItemFactory.Build(dtoMatch, TestConfig.DefaultLanguage);
            SportEventCacheStore.Add(apiMatch.sport_event.id, sportEvent);

            var sportId = await SportEventCache.GetEventSportIdAsync(dtoMatch.Id);

            using (new AssertionScope())
            {
                // the sport event should not be retrieved from the provider (because it is in the cache)
                SummaryFetcherMock.Verify(x => x.GetDataAsync(It.IsAny<Uri>()), Times.Never);

                sportId.Should().BeEquivalentTo(dtoMatch.Tournament.Sport.Id);
            }
        }
    }
}
