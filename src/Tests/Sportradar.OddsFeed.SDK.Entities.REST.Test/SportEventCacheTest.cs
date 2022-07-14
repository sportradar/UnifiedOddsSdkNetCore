/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using MatchCI = Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events.MatchCI;

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
            var e = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);
            ValidateSportEventCacheItem(e);
        }

        [TestMethod]
        public void SportEventCacheItemMergeFixture()
        {
            var item = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);
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
            var item = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);
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

            var item = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);
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

            var a = (IMatchCI)_sportEventCache.GetEventCacheItem(TestData.EventId);
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
        public async Task SportEventCacheSingleItemSequentialTest()
        {
            // slow implementation of async calls
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            while (i > 0)
            {
                i--;

                var ci = await GetMatchCacheItemAsync(TestData.EventId, culture, stopWatch, i).ConfigureAwait(false);
                Assert.IsNotNull(ci);
                Assert.AreEqual(TestData.EventId, ci.Id);

                if (i % 10 != 3)
                {
                    var c1 = _dataRouterManager.GetCallCount(SportEventSummary);
                    TestData.ValidateTestEventId(ci, new[] { culture }, true);
                    var c2 = _dataRouterManager.GetCallCount(SportEventSummary);
                    Assert.AreEqual(c1, c2);
                }
            }
            Assert.AreEqual(2, _memoryCache.Count());
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheSingleItemConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var tasks = new List<Task<MatchCI>>();
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            while (i > 0)
            {
                i--;
                tasks.Add(GetMatchCacheItemAsync(TestData.EventId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                if (task.Result == null)
                {
                    continue;
                }
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);

                if (task.Result.Id.Id % 10 != 3)
                {
                    var c1 = _dataRouterManager.GetCallCount(SportEventSummary);
                    TestData.ValidateTestEventId(task.Result, new[] { culture }, true);
                    var c2 = _dataRouterManager.GetCallCount(SportEventSummary);
                    Assert.AreEqual(c1, c2);
                }
            }

            Assert.AreEqual(2, _memoryCache.Count());
        }

        [TestMethod, Timeout(150000)]
        public async Task SportEventCacheUniqueItemSequentialTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:{i}");
                i--;

                var ci = await GetMatchCacheItemAsync(matchId, culture, stopWatch, i).ConfigureAwait(false);
                Assert.IsNotNull(ci);
                Assert.AreEqual(matchId, ci.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 500);

        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheUniqueItemConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:{i}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 500);
        }

        [TestMethod, Timeout(150000)]
        public async Task SportEventCacheSemiSequentialTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                var ci = await GetMatchCacheItemAsync(matchId, culture, stopWatch, i).ConfigureAwait(false);
                Assert.IsNotNull(ci);
                Assert.AreEqual(matchId, ci.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        //[TestMethod, Timeout(300000)]
        public async Task SportEventCacheSemiWithDelaySequentialTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), true, 30);
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                var ci = await GetMatchCacheItemAsync(matchId, culture, stopWatch, i).ConfigureAwait(false);
                Assert.IsNotNull(ci);
                Assert.AreEqual(matchId, ci.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), true, 30);
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayAndPause050ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), false, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(50));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        //[TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayAndPause100ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), false, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(100));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        //[TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayAndPause150ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), false, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(150));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayAndPause200ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), false, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        //[TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayAndPause250ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), false, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(250));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        //[TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayAndPause350ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), false, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(350));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithDelayAndPause500ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), false, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(500));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheSemiWithVariableDelayAndPause200ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(1000), true, 30);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheCustomerScenarioAllRequestsSlowerAndPause200ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(300), false, 90);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheCustomerScenarioSomeRequestSlowAndPause200ConcurrencyTest()
        {
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(3000), false, 10);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheCustomerScenarioAllRequestSlowAndPause200ConcurrencyThreadPool1Test()
        {
            ThreadPool.SetMaxThreads(1, 1);
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(300), false, 90);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheCustomerScenarioAllRequestSlowAndPause200ConcurrencyThreadPool2Test()
        {
            ThreadPool.SetMaxThreads(2, 2);
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(300), false, 90);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                tasks.Add(GetMatchCacheItemAsync(matchId, culture, stopWatch, i));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        [TestMethod, Timeout(120000)]
        public async Task SportEventCacheCustomerScenarioAllRequestSlowAndPause200ConcurrencyNewThreadsTest()
        {
            //ThreadPool.SetMaxThreads(10, 10);
            var stopWatch = Stopwatch.StartNew();
            Assert.AreEqual(0, _memoryCache.Count());
            var i = 1000;
            var culture = TestData.Cultures4[StaticRandom.I(4)];
            _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(300), false, 90);
            _sportEventCache.LockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            var tasks = new List<Task<MatchCI>>();
            while (i > 0)
            {
                var matchId = URN.Parse($"sr:match:1{StaticRandom.I100}");
                i--;

                var task = GetMatchCacheItemAsync(matchId, culture, stopWatch, i);
                tasks.Add(task);
                var t = new Thread(async () => { await task.ConfigureAwait(false); });
                t.Name = $"thread {i}-{matchId}";
                t.Start();
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var task in tasks)
            {
                Assert.IsNotNull(task.Result);
                Assert.IsNotNull(task.Result.Id);
            }
            Assert.IsTrue(_memoryCache.Count() > 50);
            Assert.IsTrue(_memoryCache.Count() < 100);
        }

        private async Task<MatchCI> GetMatchCacheItemAsync(URN matchId, CultureInfo culture, Stopwatch stopWatch, int i)
        {
            MatchCI ci = null;
            var startTime = stopWatch.Elapsed;

            try
            {
                Debug.WriteLine($"{GetElapsed(stopWatch)} Loading {i} culture: {culture.TwoLetterISOLanguageName}");

                ci = (MatchCI)_sportEventCache.GetEventCacheItem(matchId);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} culture: {culture.TwoLetterISOLanguageName}");

                var name = await ci.GetNamesAsync(new[] { culture }).ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} name: {name?.Values.First()}");
                var fixture = await ci.GetFixtureAsync(new[] { culture }).ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} fixture: {fixture.StartTime}");
                var tournamentId = await ci.GetTournamentIdAsync(TestData.Cultures).ConfigureAwait(false);
                // Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} tournament: {tournamentId}");
                var bookingStatus = await ci.GetBookingStatusAsync().ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} booking status: {bookingStatus}");
                var competitorIds = await ci.GetCompetitorsIdsAsync(new[] { culture }).ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} competitors: {string.Join(",", competitorIds.Select(s => s.ToString()))}");
                var venue = await ci.GetVenueAsync(new[] { culture }).ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} venue: {venue?.Id}");
                var liveOdds = await ci.GetLiveOddsAsync().ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} live odds: {liveOdds}");
                var sportEventType = await ci.GetSportEventTypeAsync().ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} sport event type: {sportEventType}");
                var stageType = await ci.GetStageTypeAsync().ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} stage type: {stageType}");
                var status = await ci.FetchSportEventStatusAsync().ConfigureAwait(false);
                //Debug.WriteLine($"{GetElapsed(stopWatch)} Get {i} status: {status}");
                Debug.WriteLine($"{GetElapsed(stopWatch)} Tasks {i} completed.");

                if (i % 10 == 3)
                {
                    Debug.WriteLine($"{GetElapsed(stopWatch)} Deleting {i} culture: {culture.TwoLetterISOLanguageName}");
                    _sportEventCache.CacheDeleteItem(matchId, CacheItemType.All);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{GetElapsed(stopWatch)} Error: {e}");
            }

            var took = stopWatch.Elapsed - startTime;
            Debug.WriteLine($"{GetElapsed(stopWatch)} Iteration {i} completed. Took {took.Milliseconds} ms.");

            return ci;
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
