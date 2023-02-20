/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class SportEventCacheItemTests
    {
        private const string SportEventSummary = "GetSportEventSummaryAsync";
        private const string SportEventFixture = "GetSportEventFixtureAsync";

        private readonly SportEventCache _sportEventCache;
        private readonly TestDataRouterManager _dataRouterManager;

        public SportEventCacheItemTests(ITestOutputHelper outputHelper)
        {
            var memoryCache = new MemoryCache("sportEventCache");
            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);
            var timer = new TestTimer(false);

            _sportEventCache = new SportEventCache(
                memoryCache,
                _dataRouterManager,
                new SportEventCacheItemFactory(
                    _dataRouterManager,
                    new SemaphorePool(5, ExceptionHandlingStrategy.THROW),
                    TestData.Cultures.First(),
                    new MemoryCache("FixtureTimestampCache")),
                timer,
                TestData.Cultures,
                cacheManager);
        }

        [Fact]
        public async Task FixtureProviderIsCalledOnlyOnceForEachLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

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

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task DetailsProviderIsCalledOnlyOnceForEachLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
            var cultures = new[] { new CultureInfo("en") };

            await cacheItem.GetConditionsAsync(cultures);
            await cacheItem.GetConditionsAsync(cultures);
            await cacheItem.GetConditionsAsync(cultures);

            Assert.Equal(cultures.Length, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task GetBookingStatusCallsProviderWithDefaultLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetBookingStatusAsync();
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.Equal(1, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task GetBookingStatusCallsProviderCallsOnlyOnce()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetBookingStatusAsync();
            await cacheItem.GetVenueAsync(TestData.Cultures);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task GetBookingStatusForStageCallsProviderCallsOnlyOnce()
        {
            var cacheItem = (IStageCI)_sportEventCache.GetEventCacheItem(TestData.EventStageId);

            await cacheItem.GetBookingStatusAsync();

            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task GetBookingStatusForStageCallsProviderCallsOnlyOnceRepeated()
        {
            var cacheItem = (IStageCI)_sportEventCache.GetEventCacheItem(TestData.EventStageId);

            await cacheItem.GetBookingStatusAsync();

            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(1, _dataRouterManager.GetCallCount(SportEventFixture));

            await cacheItem.GetBookingStatusAsync();

            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task GetScheduleAsyncCallsProviderWithDefaultLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetScheduledAsync();
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.Equal(2, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task GetScheduleEndAsyncCallsProviderWithDefaultLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetScheduledEndAsync();
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.Equal(2, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task GetTournamentIdAsyncCallsProviderWithAllLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetTournamentIdAsync(TestData.Cultures);
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });

            Assert.Equal(3, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task NumberOfCallsToFixtureProviderIsEqualToNumberOfLocalsWhenAccessingTheSameProperty()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetVenueAsync(TestData.Cultures);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task NumberOfCallsToSummaryProviderIsEqualNumberOfLanguage()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetVenueAsync(new[] { new CultureInfo("en") });
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
            await cacheItem.GetVenueAsync(new[] { new CultureInfo("hu") });

            Assert.Equal(3, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task NumberOfCallsToSummaryProviderForNonTranslatableProperty()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.FetchSportEventStatusAsync();
            await cacheItem.FetchSportEventStatusAsync();
            await cacheItem.FetchSportEventStatusAsync();

            Assert.Equal(3, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventFixture));
        }

        [Fact]
        public async Task NumberOfCallsToFixtureProviderIsEqualToNumberOfLocalsWhenAccessingDifferentProperties()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);

            await cacheItem.GetVenueAsync(new[] { new CultureInfo("en") });
            await cacheItem.GetFixtureAsync(new[] { new CultureInfo("de") });
            await cacheItem.GetTournamentRoundAsync(new[] { new CultureInfo("hu") });

            Assert.Equal(2, _dataRouterManager.GetCallCount(SportEventSummary));
            Assert.Equal(1, _dataRouterManager.GetCallCount(SportEventFixture));
        }
    }
}
