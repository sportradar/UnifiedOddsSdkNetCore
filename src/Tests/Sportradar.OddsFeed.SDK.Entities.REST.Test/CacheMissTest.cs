/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    /// <summary>
    /// For testing functionality of various caches handling missing fetches
    /// </summary>
    [TestClass]
    public class CacheMissTest
    {
        const string AllTournaments = "GetAllTournamentsForAllSportAsync";
        const string AllSports = "GetAllSportsAsync";
        const string SportEventSummary = "GetSportEventSummaryAsync";

        private SportDataCache _sportDataCache;
        private SportEventCache _sportEventCache;
        private MemoryCache _memoryCache;

        private TestTimer _timer;

        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _memoryCache = new MemoryCache("tournamentDetailsCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(_memoryCache, _dataRouterManager, new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5), TestData.Cultures.First(), new MemoryCache("FixtureTimestampCache")), _timer, TestData.Cultures, _cacheManager);
            _sportDataCache = new SportDataCache(_dataRouterManager, _timer, TestData.Cultures, _sportEventCache, _cacheManager);
        }

        [TestMethod]
        public void SportDataCacheCorrectlyHandlesFetchMiss()
        {
            var nonExistingTournamentUrn = URN.Parse($"{TestData.SimpleTournamentId}9");
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _sportEventCache.Cache.Count());

            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).Result; // initial load
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(0, _sportEventCache.SpecialTournaments.Count());

            var data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).Result;
            Assert.IsNull(data01);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(1, _sportEventCache.SpecialTournaments.Count());

            data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).Result;
            Assert.IsNull(data01);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(1, _sportEventCache.SpecialTournaments.Count());

            Assert.IsNotNull(sports);
            Assert.IsNull(data01);
        }

        [TestMethod]
        public void SportDataCacheCorrectlySavesSpecialTournament()
        {
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
            Assert.AreEqual(0, _sportEventCache.Cache.Count());

            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).Result; // initial load
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(0, _sportEventCache.SpecialTournaments.Count());

            var data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, TestData.Cultures).Result;
            Assert.IsNotNull(data01);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            Assert.AreEqual(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(1, _sportEventCache.SpecialTournaments.Count());

            data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, TestData.Cultures).Result;
            Assert.IsNotNull(data01);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            Assert.AreEqual(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(1, _sportEventCache.SpecialTournaments.Count());

            Assert.IsNotNull(sports);
            Assert.IsNotNull(data01);
        }

        [TestMethod]
        public void SportDataCacheCorrectlySavesFetchMissWhenCalledManyTimes()
        {
            var nonExistingTournamentUrn = URN.Parse($"{TestData.SimpleTournamentId11111}9");
            Assert.AreEqual(0, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).Result;
            for (var i = 0; i < 3; i++)
            {
                data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).Result;
            }

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount+1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));

            Assert.IsNull(data01);
        }

        [TestMethod]
        public void SportDataCacheCorrectlySavesNonExistingFetchMissWhenCalledForEachCulture()
        {
            var nonExistingTournamentUrn = URN.Parse($"{TestData.SimpleTournamentId11111}9");
            Assert.AreEqual(0, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, new[] { TestData.CultureNl }).Result;
            foreach (var t in TestData.Cultures)
            {
                data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, new[] { t }).Result;
            }

            Assert.AreEqual(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count + 1} times.");
            Assert.AreEqual(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count + 1} times.");

            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));

            Assert.IsNull(data01);
        }

        [TestMethod]
        public void SportDataCacheCorrectlySavesExistingFetchMissWhenCalledForEachCulture()
        {
            Assert.AreEqual(0, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, new[] { TestData.CultureNl }).Result;
            foreach (var t in TestData.Cultures)
            {
                data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, new[] { t }).Result;
            }

            Assert.AreEqual(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count + 1} times.");
            Assert.AreEqual(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count + 1} times.");

            Assert.AreEqual(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);
            Assert.AreEqual(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));

            Assert.IsNotNull(data01);
            Assert.AreEqual(URN.Parse("sr:sport:999"), data01.Id);
            Assert.AreEqual(1, data01.Names.Count);
        }
    }
}
