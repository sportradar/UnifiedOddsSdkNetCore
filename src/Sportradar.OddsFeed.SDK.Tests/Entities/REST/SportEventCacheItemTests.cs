// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class SportEventCacheItemTests
{
    private readonly SportEventCache _sportEventCache;
    private readonly TestDataRouterManager _dataRouterManager;

    public SportEventCacheItemTests(ITestOutputHelper outputHelper)
    {
        var loggerFactory = new XunitLoggerFactory(outputHelper);
        var testCacheStoreManager = new TestCacheStoreManager();
        var cacheManager = testCacheStoreManager.CacheManager;
        _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);
        var timer = new TestTimer(false);

        _sportEventCache = new SportEventCache(
            testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache),
            _dataRouterManager,
            new SportEventCacheItemFactory(
                _dataRouterManager,
                new SemaphorePool(5, ExceptionHandlingStrategy.Throw),
                testCacheStoreManager.UofConfig,
                testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache)),
            timer,
            TestData.Cultures,
            cacheManager,
            loggerFactory);
    }

    [Fact]
    public async Task FixtureProviderIsCalledOnlyOnceForEachLanguage()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetBookingStatusAsync();
        await cacheItem.GetScheduledAsync();
        await cacheItem.GetScheduledEndAsync();
        await cacheItem.GetCompetitorsIdsAsync(TestData.Cultures);
        await cacheItem.GetTournamentRoundAsync(TestData.Cultures);
        await cacheItem.GetSeasonAsync(TestData.Cultures);
        await cacheItem.GetTournamentIdAsync(TestData.Cultures);
        await cacheItem.GetVenueAsync(TestData.Cultures);
        await cacheItem.GetFixtureAsync(TestData.Cultures);
        await cacheItem.GetReferenceIdsAsync();

        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task DetailsProviderIsCalledOnlyOnceForEachLanguage()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
        var cultures = new[] { new CultureInfo("en") };

        await cacheItem.GetConditionsAsync(cultures);
        await cacheItem.GetConditionsAsync(cultures);
        await cacheItem.GetConditionsAsync(cultures);

        Assert.Equal(cultures.Length, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task GetBookingStatusCallsProviderWithDefaultLanguage()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetBookingStatusAsync();
        await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task GetBookingStatusCallsProviderCallsOnlyOnce()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetBookingStatusAsync();
        await cacheItem.GetVenueAsync(TestData.Cultures);

        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task GetBookingStatusForStageCallsProviderCallsOnlyOnce()
    {
        var cacheItem = (IStageCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventStageId);

        await cacheItem.GetBookingStatusAsync();

        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task GetBookingStatusForStageCallsProviderCallsOnlyOnceRepeated()
    {
        var cacheItem = (IStageCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventStageId);

        await cacheItem.GetBookingStatusAsync();

        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));

        await cacheItem.GetBookingStatusAsync();

        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task GetScheduleAsyncCallsProviderWithDefaultLanguage()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetScheduledAsync();
        await cacheItem.GetVenueAsync([new CultureInfo("de")]);

        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task GetScheduleEndAsyncCallsProviderWithDefaultLanguage()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetScheduledEndAsync();
        await cacheItem.GetVenueAsync([new CultureInfo("de")]);

        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task GetTournamentIdAsyncCallsProviderWithAllLanguage()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetTournamentIdAsync(TestData.Cultures);
        await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

        Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task NumberOfCallsToFixtureProviderIsEqualToNumberOfLocalsWhenAccessingTheSameProperty()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetVenueAsync(TestData.Cultures);

        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task NumberOfCallsToSummaryProviderIsEqualNumberOfLanguage()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetVenueAsync(new[] { new CultureInfo("en") });
        await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
        await cacheItem.GetVenueAsync(new[] { new CultureInfo("hu") });

        Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task NumberOfCallsToSummaryProviderForNonTranslatableProperty()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.FetchSportEventStatusAsync();
        await cacheItem.FetchSportEventStatusAsync();
        await cacheItem.FetchSportEventStatusAsync();

        Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }

    [Fact]
    public async Task NumberOfCallsToFixtureProviderIsEqualToNumberOfLocalsWhenAccessingDifferentProperties()
    {
        var cacheItem = (IMatchCacheItem)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

        await cacheItem.GetVenueAsync([new CultureInfo("en")]);
        await cacheItem.GetFixtureAsync([new CultureInfo("de")]);
        await cacheItem.GetTournamentRoundAsync([new CultureInfo("hu")]);

        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventFixture));
    }
}
