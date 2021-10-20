/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SportEventCacheItemTests
    {
        const string SportEventSummary = "GetSportEventSummaryAsync";
        const string SportEventFixture = "GetSportEventFixtureAsync";

        private SportEventCache _sportEventCache;
        private MemoryCache _memoryCache;
        private TestTimer _timer;

        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _memoryCache = new MemoryCache("sportEventCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(_memoryCache, _dataRouterManager, new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5, ExceptionHandlingStrategy.THROW), TestData.Cultures.First(), new MemoryCache("FixtureTimestampCache")), _timer, TestData.Cultures, _cacheManager);
        }

        [TestMethod]
        public void fixture_provider_is_called_only_once_for_each_locale()
        {
            var cacheItem = (IMatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
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
            });

            Task.WaitAll(task);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public void details_provider_is_called_only_once_for_each_locale()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var cultures = new[] {new CultureInfo("en")};
            var task = Task.Run(async () =>
            {
                await cacheItem.GetConditionsAsync(cultures);
                await cacheItem.GetConditionsAsync(cultures);
                await cacheItem.GetConditionsAsync(cultures);
            });

            Task.WaitAll(task);

            Assert.AreEqual(cultures.Length, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {cultures.Length} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 0 times.");
        }

        [TestMethod]
        public void get_booking_status_calls_provider_with_default_locale()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetBookingStatusAsync();
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
            });

            Task.WaitAll(task);

            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 1 times.");
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 1 times.");
        }

        [TestMethod]
        public void get_booking_status_calls_provider_calls_only_once()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetBookingStatusAsync();
                await cacheItem.GetVenueAsync(TestData.Cultures);
            });

            Task.WaitAll(task);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 1 times.");
        }

        [TestMethod]
        public void get_schedule_async_calls_provider_with_default_locale()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetScheduledAsync();
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
            });

            Task.WaitAll(task);

            Assert.AreEqual(2, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 2 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 0 times.");
        }

        [TestMethod]
        public void get_schedule_end_async_calls_provider_with_default_locale()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetScheduledEndAsync();
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
            });

            Task.WaitAll(task);

            Assert.AreEqual(2, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 2 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 0 times.");
        }

        [TestMethod]
        public void get_tournament_id_async_calls_provider_with_default_locale()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetTournamentIdAsync(TestData.Cultures);
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
            });

            Task.WaitAll(task);

            Assert.AreEqual(2, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 2 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 0 times.");
        }

        [TestMethod]
        public void number_of_calls_to_fixture_provider_is_equal_to_number_of_locals_when_accessing_the_same_property()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetVenueAsync(TestData.Cultures);
            });

            Task.WaitAll(task);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 0 times.");
        }

        [TestMethod]
        public void number_of_calls_to_summary_provider_is_equal_number_of_locals()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("en") });
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("de") });
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("fr") });
            });

            Task.WaitAll(task);

            Assert.AreEqual(3, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 3 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 0 times.");
        }

        [TestMethod]
        public void number_of_calls_to_summary_provider_for_non_translatable_property()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.FetchSportEventStatusAsync();
                await cacheItem.FetchSportEventStatusAsync();
                await cacheItem.FetchSportEventStatusAsync();
            });

            Task.WaitAll(task);

            Assert.AreEqual(3, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 1 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 0 times.");
        }

        [TestMethod]
        public void number_of_calls_to_fixture_provider_is_equal_to_number_of_locals_when_accessing_different_properties()
        {
            var cacheItem = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);

            var task = Task.Run(async () =>
            {
                await cacheItem.GetVenueAsync(new[] { new CultureInfo("en") });
                await cacheItem.GetFixtureAsync(new[] { new CultureInfo("de") });
                await cacheItem.GetTournamentRoundAsync(new[] { new CultureInfo("fr") });
            });

            Task.WaitAll(task);

            Assert.AreEqual(2, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 2 times.");
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(SportEventFixture), $"{SportEventFixture} should be called exactly 1 times.");
        }
    }
}
