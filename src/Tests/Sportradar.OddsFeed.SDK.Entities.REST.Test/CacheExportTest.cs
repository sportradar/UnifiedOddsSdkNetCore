/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    /// <summary>
    /// For testing functionality of various caches handling missing fetches
    /// </summary>
    [TestClass]
    public class CacheExportTest
    {
        const string AllTournaments = "GetAllTournamentsForAllSportAsync";
        const string AllSports = "GetAllSportsAsync";
        const string SportEventSummary = "GetSportEventSummaryAsync";
        private const string Competitor = "GetCompetitorAsync";
        private const string PlayerProfile = "GetPlayerProfileAsync";

        private SportDataCache _sportDataCache;
        private SportEventCache _sportEventCache;
        private ProfileCache _profileCache;
        private MemoryCache _profileMemoryCache;

        private TestTimer _timer;

        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(new MemoryCache("tournamentDetailsCache"), _dataRouterManager, new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5), TestData.Cultures.First(), new MemoryCache("FixtureTimestampCache")), _timer, TestData.Cultures, _cacheManager);
            _sportDataCache = new SportDataCache(_dataRouterManager, _timer, TestData.Cultures, _sportEventCache, _cacheManager);
            _profileMemoryCache = new MemoryCache("profileCache");
            _profileCache = new ProfileCache(_profileMemoryCache, _dataRouterManager, _cacheManager);
        }

        [TestMethod]
        public void SportDataCacheStatusTest()
        {
            var status = _sportDataCache.CacheStatus();
            Assert.AreEqual(0, status["SportCI"]);
            Assert.AreEqual(0, status["CategoryCI"]);


            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).Result; // initial load

            Assert.IsNotNull(sports);
            status = _sportDataCache.CacheStatus();
            Assert.AreEqual(TestData.CacheSportCount, status["SportCI"]);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, status["CategoryCI"]);
        }

        [TestMethod]
        public async Task SportDataCacheEmptyExportTest()
        {
            var export = (await _sportDataCache.ExportAsync()).ToList();
            Assert.AreEqual(0, export.Count);

            await _sportDataCache.ImportAsync(export);
            Assert.AreEqual(0, _sportDataCache.Sports.Count);
            Assert.AreEqual(0, _sportDataCache.Categories.Count);
        }

        [TestMethod]
        public async Task SportDataCacheFullExportTest()
        {
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).Result; // initial load
            Assert.IsNotNull(sports);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");


            var export = (await _sportDataCache.ExportAsync()).ToList();
            Assert.AreEqual(TestData.CacheSportCount + TestData.CacheCategoryCountPlus, export.Count);

            _sportDataCache.Sports.Clear();
            _sportDataCache.Categories.Clear();
            _sportDataCache.FetchedCultures.Clear();
            
            await _sportDataCache.ImportAsync(export);
            Assert.AreEqual(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.AreEqual(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);

            // No calls to the data router manager
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments), $"{AllTournaments} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports), $"{AllSports} should be called exactly {TestData.Cultures.Count} times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly 0 times.");

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _sportDataCache.ExportAsync());
            Assert.AreEqual(exportString, secondExportString);
        }

        [TestMethod]
        public void ProfileCacheStatusTest()
        {
            var status = _profileCache.CacheStatus();
            Assert.AreEqual(0, status["TeamCompetitorCI"]);
            Assert.AreEqual(0, status["CompetitorCI"]);
            Assert.AreEqual(0, status["PlayerProfileCI"]);

            object _ci = _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), TestData.Cultures).Result;
            _ci = _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:2"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:2"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:1"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:2"), TestData.Cultures).Result;

            status = _profileCache.CacheStatus();
            Assert.AreEqual(0, status["TeamCompetitorCI"]);
            Assert.AreEqual(4, status["CompetitorCI"]);
            Assert.AreEqual(62, status["PlayerProfileCI"]);
            Assert.AreEqual(_profileMemoryCache.GetCount(), status.Sum(i => i.Value));
        }

        [TestMethod]
        public async Task ProfileCacheEmptyExportTest()
        {
            var export = (await _profileCache.ExportAsync()).ToList();
            Assert.AreEqual(0, export.Count);

            await _profileCache.ImportAsync(export);
            Assert.AreEqual(0, _profileMemoryCache.GetCount());
        }

        [TestMethod]
        public async Task ProfileCacheFullExportTest()
        {
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(Competitor), $"{Competitor} should be called exactly 0 times.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(PlayerProfile), $"{PlayerProfile} should be called exactly 0 times.");

            object _ci = _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), TestData.Cultures).Result;
            _ci = _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:2"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:2"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:1"), TestData.Cultures).Result;
            _ci = _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:2"), TestData.Cultures).Result;

            Assert.AreEqual(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(Competitor), $"{Competitor} should be called exactly {TestData.Cultures.Count * 4} times.");
            Assert.AreEqual(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(PlayerProfile), $"{PlayerProfile} should be called exactly {TestData.Cultures.Count * 2} times.");

            var export = (await _profileCache.ExportAsync()).ToList();
            Assert.AreEqual(_profileMemoryCache.GetCount(), export.Count);

            _profileMemoryCache.ToList().ForEach(i => _profileMemoryCache.Remove(i.Key));

            await _profileCache.ImportAsync(export);
            Assert.AreEqual(export.Count, _profileMemoryCache.GetCount());

            // No calls to the data router manager
            Assert.AreEqual(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(Competitor), $"{Competitor} should be called exactly {TestData.Cultures.Count * 4} times.");
            Assert.AreEqual(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(PlayerProfile), $"{PlayerProfile} should be called exactly {TestData.Cultures.Count * 2} times.");

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _profileCache.ExportAsync());
            Assert.AreEqual(exportString, secondExportString);
        }

        [TestMethod]
        public void SportEventCacheStatusTest()
        {
            var status = _sportEventCache.CacheStatus();
            Assert.AreEqual(0, status["SportEventCI"]);
            Assert.AreEqual(0, status["CompetitionCI"]);
            Assert.AreEqual(0, status["DrawCI"]);
            Assert.AreEqual(0, status["LotteryCI"]);
            Assert.AreEqual(0, status["TournamentInfoCI"]);
            Assert.AreEqual(0, status["MatchCI"]);
            Assert.AreEqual(0, status["StageCI"]);

            var tournaments = _sportEventCache.GetActiveTournamentsAsync().Result; // initial load

            Assert.IsNotNull(tournaments);
            status = _sportEventCache.CacheStatus();
            Assert.AreEqual(213, status["TournamentInfoCI"]);
            Assert.AreEqual(974, status["MatchCI"]);
        }

        [TestMethod]
        public async Task SportEventCacheEmptyExportTest()
        {
            var export = (await _sportEventCache.ExportAsync()).ToList();
            Assert.AreEqual(0, export.Count);

            await _sportDataCache.ImportAsync(export);
            Assert.AreEqual(0, _sportEventCache.Cache.GetCount());
        }

        [TestMethod]
        public async Task SportEventCacheFullExportTest()
        {
            var tournaments = _sportEventCache.GetActiveTournamentsAsync().Result; // initial load
            Assert.IsNotNull(tournaments);

             Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            var export = (await _sportEventCache.ExportAsync()).ToList();
            Assert.AreEqual(1187, export.Count);

            _sportEventCache.Cache.ToList().ForEach(i => _sportEventCache.Cache.Remove(i.Key));

            await _sportEventCache.ImportAsync(export);
            Assert.AreEqual(1187, _sportEventCache.Cache.GetCount());

            // No calls to the data router manager
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(SportEventSummary), $"{SportEventSummary} should be called exactly {TestData.Cultures.Count} times.");

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _sportEventCache.ExportAsync());
            Assert.AreEqual(exportString, secondExportString);
        }

        private string SerializeExportables(IEnumerable<ExportableCI> exportables)
        {
            return JsonConvert.SerializeObject(exportables.OrderBy(e => e.Id));
        }
    }
}
