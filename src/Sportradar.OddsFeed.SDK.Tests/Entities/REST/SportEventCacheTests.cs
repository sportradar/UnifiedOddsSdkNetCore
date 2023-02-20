/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class SportEventCacheTests
    {
        private const int ScheduleEventCount = 974;
        private const int TournamentEventCount = 241;

        const string DateSchedule = "GetSportEventsForDateAsync";
        const string TournamentSchedule = "GetSportEventsForTournamentAsync";
        const string SportEventSummary = "GetSportEventSummaryAsync";

        private readonly SportEventCache _sportEventCache;
        private readonly MemoryCache _memoryCache;
        private readonly TestTimer _timer;
        private readonly TestDataRouterManager _dataRouterManager;

        public SportEventCacheTests(ITestOutputHelper outputHelper)
        {
            _memoryCache = new MemoryCache("sportEventCache");

            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(
                _memoryCache,
                _dataRouterManager,
                new SportEventCacheItemFactory(
                    _dataRouterManager,
                    new SemaphorePool(5, ExceptionHandlingStrategy.THROW),
                    TestData.Cultures.First(),
                    new MemoryCache("FixtureTimestampCache")),
                _timer,
                TestData.Cultures,
                cacheManager);
        }

        [Fact]
        public void InitialSetupTest()
        {
            Assert.NotNull(_sportEventCache);
            Assert.NotNull(_memoryCache);
            Assert.NotNull(_timer);
            Assert.NotNull(_dataRouterManager);
        }

        // TODO - add also when it finishes in TestDataRouterManager
        //[Fact]
        //public void TestDataIsCaching()
        //{
        //    // even if called several times, loads only once for all specified languages
        //    _timer.FireOnce(TimeSpan.Zero);
        //    _timer.FireOnce(TimeSpan.Zero);
        //    _timer.FireOnce(TimeSpan.Zero);

        //    var i = 0;
        //    while (_dataRouterManager.GetCallCount(DateSchedule) != TestData.Cultures.Count * 3 && i < 100)
        //    {
        //        Task.Delay(100).Wait();
        //        i++;
        //    }
        //    i = 0;
        //    while (_memoryCache.GetCount() < 900 && i < 20)
        //    {
        //        Task.Delay(1000).Wait();
        //        i++;
        //    }
        //    Assert.True(_memoryCache.GetCount() > 0, "Nothing was cached.");
        //    Assert.Equal(ScheduleEventCount, _sportEventCache.Cache.Count(c => c.Key.Contains("match")));
        //    Assert.Equal(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule));
        //}

        //[Fact]
        //public void SportEventBaseDataIsCachedCorrectly()
        //{
        //    TestDataIsCaching();
        //    var e = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
        //    ValidateSportEventCacheItem(e);
        //}

        [Fact]
        public async Task SportEventCacheItemMergeFixture()
        {
            var item = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
            Assert.NotNull(item); // empty with providers

            var tourId = await item.GetTournamentIdAsync(TestData.Cultures);
            var fixture = await item.GetFixtureAsync(TestData.Cultures);

            Assert.Equal("sr:tournament:1030", tourId.ToString());
            Assert.NotNull(fixture);

            ValidateSportEventCacheItem(item, true);
        }

        [Fact]
        public void SportEventCacheItemMergeDetails()
        {
            var item = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
            Assert.NotNull(item); // empty with providers
            Assert.Equal(0, _dataRouterManager.GetCallCount(DateSchedule));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            SportEventConditionsCI cons = null;
            Task.Run(async () =>
            {
                cons = await item.GetConditionsAsync(TestData.Cultures);
                cons = await item.GetConditionsAsync(TestData.Cultures);
                await item.GetCompetitorsIdsAsync(TestData.Cultures);
            }).GetAwaiter().GetResult();

            Assert.NotNull(cons);
            Assert.Equal(0, _dataRouterManager.GetCallCount(DateSchedule));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary));

            ValidateSportEventCacheItem(item, true);
        }

        //[Fact]
        //public void SportEventCacheItemMergeFixtureOnPreloadedItem()
        //{
        //    Assert.Equal(0, _dataRouterManager.GetCallCount(DateSchedule));
        //    Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

        //    TestDataIsCaching();

        //    Assert.Equal(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule));
        //    Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

        //    var item = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
        //    Assert.NotNull(item); // preloaded with event summary with providers
        //    Assert.Equal(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule));
        //    Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

        //    SeasonCI seasonCI = null;
        //    Task.Run(async () =>
        //    {
        //        await item.GetTournamentIdAsync(TestData.Cultures);
        //        seasonCI = await item.GetSeasonAsync(TestData.Cultures);
        //        await item.GetCompetitorsIdsAsync(TestData.Cultures);
        //    }).GetAwaiter().GetResult();

        //    Assert.Equal(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule));
        //    Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));
        //    Assert.NotNull(seasonCI);

        //    ValidateSportEventCacheItem(item, true);

        //    //merge fixture
        //    var tourId = TestData.TournamentId;
        //    IFixture fixture = null;
        //    Task.Run(async () =>
        //    {
        //        tourId = await item.GetTournamentIdAsync(TestData.Cultures);
        //        fixture = await item.GetFixtureAsync(TestData.Cultures);
        //    }).GetAwaiter().GetResult();

        //    Assert.Equal(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule));
        //    Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));
        //    Assert.Equal("sr:tournament:1030", tourId.ToString());
        //    Assert.NotNull(fixture);

        //    ValidateSportEventCacheItem(item, true);
        //}

        [Fact]
        public void GetScheduleForDate()
        {
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(DateSchedule));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            Task.Run(async () => await _sportEventCache.GetEventIdsAsync(DateTime.Now, TestData.Culture)).Wait();

            Assert.Equal(ScheduleEventCount, _memoryCache.Count());
            Assert.Equal(1, _dataRouterManager.GetCallCount(DateSchedule));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            var a = _sportEventCache.GetEventCacheItem(TestData.EventMatchId);
            Assert.NotNull(a);
        }

        //[Fact]
        //public async Task GetScheduleForDateThatWasPreloadedByTimer()
        //{
        //    Assert.Empty(_memoryCache);
        //    Assert.Equal(0, _dataRouterManager.GetCallCount(DateSchedule));

        //    TestDataIsCaching();

        //    Assert.True(_memoryCache.GetCount() > 0, "Nothing was cached.");
        //    Assert.Equal(ScheduleEventCount, _sportEventCache.Cache.Count(c => c.Key.Contains("match")));
        //    Assert.Equal(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule));

        //    var eventsForDate = await _sportEventCache.GetEventIdsAsync(DateTime.Now, TestData.Culture).ConfigureAwait(false);
        //    Assert.NotEmpty(eventsForDate);

        //    Assert.Equal(ScheduleEventCount, _sportEventCache.Cache.Count(c => c.Key.Contains("match")));
        //    Assert.Equal((TestData.Cultures.Count * 3) + 1, _dataRouterManager.GetCallCount(DateSchedule));

        //    var a = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventMatchId);
        //    ValidateSportEventCacheItem(a);
        //    Assert.NotNull(a);
        //}

        [Fact]
        public void GetScheduleForTournamentTest()
        {
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TournamentSchedule));

            Task.Run(async () =>
            {
                await _sportEventCache.GetEventIdsAsync(TestData.TournamentId, TestData.Culture);
                Assert.Equal(TournamentEventCount, _memoryCache.Count());
                Assert.Equal(1, _dataRouterManager.GetCallCount(TournamentSchedule));

                var a = _sportEventCache.GetEventCacheItem(TestData.EventMatchId);
                Assert.NotNull(a);
            }).GetAwaiter().GetResult();
        }

        [Fact]
        public void GetScheduleForTournament()
        {
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TournamentSchedule));

            var events = _sportEventCache.GetEventIdsAsync(TestData.TournamentId, TestData.Culture).Result;
            Assert.Equal(TournamentEventCount, _memoryCache.Count());
            Assert.Equal(TournamentEventCount, events.Count());
            Assert.Equal(1, _dataRouterManager.GetCallCount(TournamentSchedule));

            var a = _sportEventCache.GetEventCacheItem(TestData.EventMatchId);
            Assert.NotNull(a);
        }

        [Fact]
        public void GetSportEventCacheItemForUnknownId()
        {
            Assert.Empty(_memoryCache);
            var eventCI = _sportEventCache.GetEventCacheItem(StaticRandom.Urn(18718, "match"));
            Assert.Single(_memoryCache);
            Assert.NotNull(eventCI);
        }

        private static void ValidateSportEventCacheItem(IMatchCI item, bool ignoreDate = false)
        {
            Assert.NotNull(item);
            Assert.Equal(TestData.EventMatchId, item.Id);
            var date = new DateTime?();
            List<URN> competitors = null;
            TeamCompetitorCI comp = null;
            RoundCI round = null;
            SeasonCI season = null;

            Task.Run(async () =>
                     {
                         date = await item.GetScheduledAsync();
                         competitors = (await item.GetCompetitorsIdsAsync(TestData.Cultures)).ToList();
                         round = await item.GetTournamentRoundAsync(TestData.Cultures);
                         season = await item.GetSeasonAsync(TestData.Cultures);
                     })
                .Wait();

            if (!ignoreDate && date != null)
            {
                Assert.Equal(new DateTime(2016, 08, 10), new DateTime(date.Value.Year, date.Value.Month, date.Value.Day));
            }

            Assert.Equal(2, competitors.Count);

            //TODO - this was removed
            if (comp != null)
            {
                Assert.Equal("sr:competitor:66390", comp.Id.ToString());
                Assert.Equal("Pericos de Puebla", comp.GetName(TestData.Culture));
                Assert.Equal("Mexico", comp.GetCountry(TestData.Culture));
                Assert.NotEqual(comp.GetCountry(TestData.Culture), comp.GetCountry(new CultureInfo("de")));
            }
            Assert.True(string.IsNullOrEmpty(round.GetName(TestData.Culture)));

            Assert.Equal(3, season.Names.Count);
            Assert.Equal("Mexican League 2016", season.Names[TestData.Culture]);
        }
    }
}
