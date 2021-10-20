/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SportEventCacheTest
    {
        private const int ScheduleEventCount = 974;
        private const int TournamentEventCount = 241;

        const string DateSchedule = "GetSportEventsForDateAsync";
        const string TournamentSchedule = "GetSportEventsForTournamentAsync";
        const string SportEventSummary = "GetSportEventSummaryAsync";

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
        public void TestDataIsCaching()
        {
            // even if called several times, loads only once for all specified languages
            _timer.FireOnce(TimeSpan.Zero);
            _timer.FireOnce(TimeSpan.Zero);
            _timer.FireOnce(TimeSpan.Zero);

            var i = 0;
            while (_dataRouterManager.GetCallCount(DateSchedule) != (TestData.Cultures.Count * 3) && i < 100)
            {
                System.Threading.Thread.Sleep(100);
                i++;
            }
            Assert.IsTrue(_memoryCache.GetCount() > 0, "Nothing was cached.");
            Assert.AreEqual(ScheduleEventCount, _sportEventCache.Cache.Count(c => c.Key.Contains("match")), "cache item count is wrong");
            Assert.AreEqual(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly {TestData.Cultures.Count * 3} times.");
        }

        [TestMethod]
        public void SportEventBaseDataIsCachedCorrectly()
        {
            TestDataIsCaching();
            var e = (IMatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);
            ValidateSportEventCacheItem(e);
        }

        [TestMethod]
        public void SportEventCacheItemMergeFixture()
        {
            var item = (IMatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);
            Assert.IsNotNull(item); // empty with providers

            var tourId = item.GetTournamentIdAsync(TestData.Cultures).Result;
            var fixture = item.GetFixtureAsync(TestData.Cultures).Result;

            Assert.AreEqual("sr:tournament:1030", tourId.ToString());
            Assert.IsNotNull(fixture);

            ValidateSportEventCacheItem(item, true);
        }

        [TestMethod]
        public void SportEventCacheItemMergeDetails()
        {
            var item = (IMatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);
            Assert.IsNotNull(item); // empty with providers
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            SportEventConditionsCI cons = null;
            Task.Run(async () =>
            {
                cons = await item.GetConditionsAsync(TestData.Cultures);
                cons = await item.GetConditionsAsync(TestData.Cultures);
                await item.GetCompetitorsIdsAsync(TestData.Cultures);
            }).GetAwaiter().GetResult();

            Assert.IsNotNull(cons);
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly 0 times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            ValidateSportEventCacheItem(item, true);
        }

        [TestMethod]
        public void SportEventCacheItemMergeFixtureOnPreloadedItem()
        {
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            TestDataIsCaching();

            Assert.AreEqual(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly {TestData.Cultures.Count * 3} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var item = (IMatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);
            Assert.IsNotNull(item); // preloaded with event summary with providers
            Assert.AreEqual(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly {TestData.Cultures.Count * 3} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            SeasonCI seasonCI = null;
            Task.Run(async () =>
            {
                await item.GetTournamentIdAsync(TestData.Cultures);
                seasonCI = await item.GetSeasonAsync(TestData.Cultures);
                await item.GetCompetitorsIdsAsync(TestData.Cultures);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly {TestData.Cultures.Count * 3} times.");
            //Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");
            Assert.IsNotNull(seasonCI);

            ValidateSportEventCacheItem(item, true);

            //merge fixture
            URN tourId = TestData.TournamentId;
            IFixture fixture = null;
            Task.Run(async () =>
            {
                tourId = await item.GetTournamentIdAsync(TestData.Cultures);
                fixture = await item.GetFixtureAsync(TestData.Cultures);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly {TestData.Cultures.Count * 3} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");
            Assert.AreEqual("sr:tournament:1030", tourId.ToString());
            Assert.IsNotNull(fixture);

            ValidateSportEventCacheItem(item, true);
        }

        [TestMethod]
        public void GetScheduleForDate()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            Task.Run(async () =>
            {
                await _sportEventCache.GetEventIdsAsync(DateTime.Now, TestData.Culture);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(ScheduleEventCount, _memoryCache.Count());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly 1 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var a = _sportEventCache.GetEventCacheItem(TestData.EventId);
            Assert.IsNotNull(a, "Cached item not found.");
        }

        [TestMethod]
        public void GetScheduleForDateThatWasPreloadedByTimer()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly 0 times.");

            TestDataIsCaching();

            Assert.IsTrue(_memoryCache.GetCount() > 0, "Nothing was cached.");
            Assert.AreEqual(ScheduleEventCount, _sportEventCache.Cache.Count(c => c.Key.Contains("match")), "cache item count is wrong");
            Assert.AreEqual(TestData.Cultures.Count * 3, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly {TestData.Cultures.Count * 3} times.");

            Task.Run(async () =>
            {
                await _sportEventCache.GetEventIdsAsync(DateTime.Now, TestData.Culture);
            }).GetAwaiter().GetResult();

            Assert.AreEqual(ScheduleEventCount, _sportEventCache.Cache.Count(c => c.Key.Contains("match")));
            Assert.AreEqual(TestData.Cultures.Count * 3 + 1, _dataRouterManager.GetCallCount(DateSchedule), $"{DateSchedule} should be called exactly {TestData.Cultures.Count * 3 + 1} times.");

            var a = (IMatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);
            ValidateSportEventCacheItem(a);
            Assert.IsNotNull(a, "Cached item not found.");
        }

        [TestMethod]
        public void GetScheduleForTournamentTest()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TournamentSchedule), $"{TournamentSchedule} should be called exactly 0 times.");

            Task.Run(async () =>
            {
                await _sportEventCache.GetEventIdsAsync(TestData.TournamentId, TestData.Culture);
                Assert.AreEqual(TournamentEventCount, _memoryCache.Count());
                Assert.AreEqual(1, _dataRouterManager.GetCallCount(TournamentSchedule), $"{TournamentSchedule} should be called exactly 1 times.");

                var a = _sportEventCache.GetEventCacheItem(TestData.EventId);
                Assert.IsNotNull(a, "Cached item not found.");
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GetScheduleForTournament()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(TournamentSchedule), $"{TournamentSchedule} should be called exactly 0 times.");

            var events = _sportEventCache.GetEventIdsAsync(TestData.TournamentId, TestData.Culture).Result;
            Assert.AreEqual(TournamentEventCount, _memoryCache.Count());
            Assert.AreEqual(TournamentEventCount, events.Count());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(TournamentSchedule), $"{TournamentSchedule} should be called exactly 1 times.");

            var a = _sportEventCache.GetEventCacheItem(TestData.EventId);
            Assert.IsNotNull(a, "Cached item not found.");
        }

        [TestMethod]
        public void GetSportEventCacheItemForUnknownId()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            var eventCI = _sportEventCache.GetEventCacheItem(StaticRandom.Urn(18718, "match"));
            Assert.AreEqual(1, _memoryCache.Count());
            Assert.IsNotNull(eventCI, "Cached item not found.");
        }

        [TestMethod, Timeout(150000)]
        public async Task SportEventCacheItemSequentialTest()
        {
            // slow implementation of async calls
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            while (i > 0)
            {
                i--;
                var startTime = stopWatch.Elapsed;
                var culture = TestData.Cultures4[StaticRandom.I(4)];

                Debug.WriteLine($"{GetElapsed(stopWatch)} Loading {i} culture: {culture.TwoLetterISOLanguageName}");
                var ci = (MatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);

                var name = await ci.GetNamesAsync(new[] { culture });
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} name");
                var fixture = await ci.GetFixtureAsync(new[] { culture });
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} fixture");
                var tour = await ci.GetTournamentIdAsync(TestData.Cultures);
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} tournament");
                var booking  = await ci.GetBookingStatusAsync();
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} booking status");
                var comps = await ci.GetCompetitorsIdsAsync(new[] {culture});
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} competitors");
                var venue = await ci.GetVenueAsync(new[] { culture });
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} venue");
                var liveOdds = await ci.GetLiveOddsAsync();
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} live odds");
                var eventType = await ci.GetSportEventTypeAsync();
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} sport event type");
                var stageType = await ci.GetStageTypeAsync();
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} stage type");
                var status = await ci.FetchSportEventStatusAsync();
                Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} status");

                Debug.WriteLine($"{GetElapsed(stopWatch)} Tasks {i} completed.");
                if (i % 10 == 3)
                {
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Deleting {i} culture: {culture.TwoLetterISOLanguageName}");
                    _sportEventCache.CacheDeleteItem(TestData.EventId, CacheItemType.All);
                }
                else
                {
                    var c1 = _dataRouterManager.GetCallCount(SportEventSummary);
                    TestData.ValidateTestEventId(ci, new[] { culture }, true);
                    var c2 = _dataRouterManager.GetCallCount(SportEventSummary);
                    Assert.AreEqual(c1, c2);
                }

                var took = stopWatch.Elapsed - startTime;
                Debug.WriteLine($"{GetElapsed(stopWatch)} Iteration {i} completed. Took {took.Milliseconds} ms.");
            }
            Assert.AreEqual(2, _memoryCache.Count());
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheItemConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var tasks = new List<Task>();
            MatchCI ci = null;
            while (i > 0)
            {
                var startTime = stopWatch.Elapsed;
                i--;
                tasks.Clear();
                var culture = TestData.Cultures4[StaticRandom.I(4)];
                try
                {
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Loading {i} culture: {culture.TwoLetterISOLanguageName}");
                    
                    ci = (MatchCI) _sportEventCache.GetEventCacheItem(TestData.EventId);
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} culture: {culture.TwoLetterISOLanguageName}");

                    tasks.Add(ci.GetNamesAsync(new[] { culture }));
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} name");
                    tasks.Add(ci.GetFixtureAsync(new[] { culture }));
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} fixture");
                    tasks.Add(ci.GetTournamentIdAsync(TestData.Cultures));
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} tournament");
                    tasks.Add(ci.GetBookingStatusAsync());
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} booking status");
                    tasks.Add(ci.GetCompetitorsIdsAsync(new[] { culture }));
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} competitors");
                    tasks.Add(ci.GetVenueAsync(new[] { culture }));
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} venue");
                    tasks.Add(ci.GetLiveOddsAsync());
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} live odds");
                    tasks.Add(ci.GetSportEventTypeAsync());
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} sport event type");
                    tasks.Add(ci.GetStageTypeAsync());
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} stage type");
                    tasks.Add(ci.FetchSportEventStatusAsync());
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} status");
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Prepare tasks {i} culture: {culture.TwoLetterISOLanguageName}");

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Error: {e}");
                }
                
                Debug.WriteLine($"{GetElapsed(stopWatch)} Tasks {i} completed.");
                if (i % 10 == 3)
                {
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Deleting {i} culture: {culture.TwoLetterISOLanguageName}");
                    _sportEventCache.CacheDeleteItem(TestData.EventId, CacheItemType.All);
                }
                else
                {
                    var c1 = _dataRouterManager.GetCallCount(SportEventSummary);
                    TestData.ValidateTestEventId(ci, new[] { culture }, true);
                    var c2 = _dataRouterManager.GetCallCount(SportEventSummary);
                    Assert.AreEqual(c1, c2);
                }

                var took = stopWatch.Elapsed - startTime;
                Debug.WriteLine($"{GetElapsed(stopWatch)} Iteration {i} completed. Took {took.Milliseconds} ms.");
            }
            Assert.AreEqual(2, _memoryCache.Count());
        }

        private string GetElapsed(Stopwatch stopwatch)
        {
            return stopwatch == null ? string.Empty : stopwatch.Elapsed.TotalSeconds.ToString("N5");
        }

        private static void ValidateSportEventCacheItem(IMatchCI item, bool ignoreDate = false)
        {
            Assert.IsNotNull(item, "Cached item not found.");
            Assert.AreEqual(TestData.EventId, item.Id);
            var date = new DateTime?();
            List<URN> competitors = null;
            TeamCompetitorCI comp = null;
            RoundCI round = null;
            SeasonCI season = null;

            Task.Run(async () =>
            {
                date = await item.GetScheduledAsync();
                competitors = (await item.GetCompetitorsIdsAsync(TestData.Cultures)).ToList();
                //comp = competitors.FirstOrDefault();
                round = await item.GetTournamentRoundAsync(TestData.Cultures);
                season = await item.GetSeasonAsync(TestData.Cultures);
            }).GetAwaiter().GetResult();

            if (!ignoreDate)
            {
                if (date != null)
                {
                    Assert.AreEqual(new DateTime(2016, 08, 10), new DateTime(date.Value.Year, date.Value.Month, date.Value.Day));
                }
            }

            Assert.AreEqual(2, competitors.Count);

            //TODO - this was removed
            if (comp != null)
            {
                Assert.AreEqual("sr:competitor:66390", comp.Id.ToString());
                Assert.AreEqual(@"Pericos de Puebla", comp.GetName(TestData.Culture));
                Assert.AreEqual("Mexico", comp.GetCountry(TestData.Culture));
                Assert.AreNotEqual(comp.GetCountry(TestData.Culture), comp.GetCountry(new CultureInfo("de")));
            }
            Assert.IsTrue(string.IsNullOrEmpty(round.GetName(TestData.Culture)));

            Assert.AreEqual(3, season.Names.Count);
            Assert.AreEqual("Mexican League 2016", season.Names[TestData.Culture]);
        }
    }
}
