// /*
// * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
// */

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    /// <summary>
    /// For testing functionality of various caches handling missing fetches
    /// </summary>
    public class CacheExportTests
    {
        private const string AllTournaments = "GetAllTournamentsForAllSportAsync";
        private const string AllSports = "GetAllSportsAsync";
        private const string SportEventSummary = "GetSportEventSummaryAsync";
        private const string Competitor = "GetCompetitorAsync";
        private const string PlayerProfile = "GetPlayerProfileAsync";

        private readonly SportDataCache _sportDataCache;
        private readonly SportEventCache _sportEventCache;
        private readonly ProfileCache _profileCache;
        private readonly MemoryCache _profileMemoryCache;
        private readonly TestDataRouterManager _dataRouterManager;

        public CacheExportTests(ITestOutputHelper outputHelper)
        {
            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

            var timer = new TestTimer(false);
            var semaphorePool = new SemaphorePool(5, ExceptionHandlingStrategy.THROW);

            _sportEventCache = new SportEventCache(
                new MemoryCache("tournamentDetailsCache"),
                _dataRouterManager,
                new SportEventCacheItemFactory(
                    _dataRouterManager,
                    semaphorePool,
                    TestData.Cultures.First(),
                    new MemoryCache("FixtureTimestampCache")),
                timer,
                TestData.Cultures,
                cacheManager);

            _sportDataCache = new SportDataCache(_dataRouterManager, timer, TestData.Cultures, _sportEventCache, cacheManager);
            _profileMemoryCache = new MemoryCache("profileCache");
            _profileCache = new ProfileCache(_profileMemoryCache, _dataRouterManager, cacheManager);
        }

        [Fact]
        public async Task SportDataCacheStatusTest()
        {
            var status = _sportDataCache.CacheStatus();
            Assert.Equal(0, status["SportCI"]);
            Assert.Equal(0, status["CategoryCI"]);

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures); // initial load

            Assert.NotNull(sports);
            status = _sportDataCache.CacheStatus();
            Assert.Equal(TestData.CacheSportCount, status["SportCI"]);
            Assert.Equal(TestData.CacheCategoryCountPlus, status["CategoryCI"]);
        }

        [Fact]
        public async Task SportDataCacheEmptyExportTest()
        {
            var export = (await _sportDataCache.ExportAsync()).ToList();
            export.Count.Should().Be(0);

            await _sportDataCache.ImportAsync(export);
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
        }

        [Fact]
        public async Task SportDataCacheFullExportTest()
        {
            Assert.Equal(0, _dataRouterManager.GetCallCount(AllTournaments));
            Assert.Equal(0, _dataRouterManager.GetCallCount(AllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures); // initial load
            Assert.NotNull(sports);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            var export = (await _sportDataCache.ExportAsync()).ToList();
            Assert.Equal(TestData.CacheSportCount + TestData.CacheCategoryCountPlus, export.Count);

            _sportDataCache.Sports.Clear();
            _sportDataCache.Categories.Clear();
            _sportDataCache.FetchedCultures.Clear();

            await _sportDataCache.ImportAsync(export);
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);

            // No calls to the data router manager
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllTournaments));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(AllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _sportDataCache.ExportAsync());
            Assert.Equal(exportString, secondExportString);
        }

        [Fact]
        public async Task ProfileCacheStatusTest()
        {
            var status = _profileCache.CacheStatus();
            Assert.Equal(0, status["TeamCompetitorCI"]);
            Assert.Equal(0, status["CompetitorCI"]);
            Assert.Equal(0, status["PlayerProfileCI"]);

            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), TestData.Cultures);
            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:2"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:2"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:1"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:2"), TestData.Cultures);

            status = _profileCache.CacheStatus();
            Assert.Equal(0, status["TeamCompetitorCI"]);
            Assert.Equal(4, status["CompetitorCI"]);
            Assert.Equal(62, status["PlayerProfileCI"]);
            Assert.Equal(_profileMemoryCache.GetCount(), status.Sum(i => i.Value));
        }

        [Fact]
        public async Task ProfileCacheEmptyExportTest()
        {
            var export = (await _profileCache.ExportAsync()).ToList();
            export.Count.Should().Be(0);

            await _profileCache.ImportAsync(export);
            Assert.Equal(0, _profileMemoryCache.GetCount());
        }

        [Fact]
        public async Task ProfileCacheFullExportTest()
        {
            Assert.Equal(0, _dataRouterManager.GetCallCount(Competitor));
            Assert.Equal(0, _dataRouterManager.GetCallCount(PlayerProfile));

            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), TestData.Cultures);
            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:2"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:2"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:1"), TestData.Cultures);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:2"), TestData.Cultures);

            Assert.Equal(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(Competitor));
            Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(PlayerProfile));

            var export = (await _profileCache.ExportAsync()).ToList();
            Assert.Equal(_profileMemoryCache.GetCount(), export.Count);

            _profileMemoryCache.ToList().ForEach(i => _profileMemoryCache.Remove(i.Key));

            await _profileCache.ImportAsync(export);
            Assert.Equal(export.Count, _profileMemoryCache.GetCount());

            // No calls to the data router manager
            Assert.Equal(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(Competitor));
            Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(PlayerProfile));

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _profileCache.ExportAsync());
            Assert.Equal(exportString, secondExportString);
        }

        [Fact]
        public async Task SportEventCacheStatusTest()
        {
            var status = _sportEventCache.CacheStatus();
            Assert.Equal(0, status["SportEventCI"]);
            Assert.Equal(0, status["CompetitionCI"]);
            Assert.Equal(0, status["DrawCI"]);
            Assert.Equal(0, status["LotteryCI"]);
            Assert.Equal(0, status["TournamentInfoCI"]);
            Assert.Equal(0, status["MatchCI"]);
            Assert.Equal(0, status["StageCI"]);

            var tournaments = await _sportEventCache.GetActiveTournamentsAsync(); // initial load

            Assert.NotNull(tournaments);
            status = _sportEventCache.CacheStatus();
            Assert.Equal(213, status["TournamentInfoCI"]);
            Assert.Equal(974, status["MatchCI"]);
        }

        [Fact]
        public async Task SportEventCacheEmptyExportTest()
        {
            var export = (await _sportEventCache.ExportAsync()).ToList();
            export.Count.Should().Be(0);

            await _sportDataCache.ImportAsync(export);
            Assert.Equal(0, _sportEventCache.Cache.GetCount());
        }

        [Fact]
        public async Task SportEventCacheFullExportTest()
        {
            var tournaments = _sportEventCache.GetActiveTournamentsAsync().Result; // initial load
            Assert.NotNull(tournaments);

            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            var export = (await _sportEventCache.ExportAsync()).ToList();
            Assert.Equal(1187, export.Count);

            _sportEventCache.Cache.ToList().ForEach(i => _sportEventCache.Cache.Remove(i.Key));

            await _sportEventCache.ImportAsync(export);
            Assert.Equal(1187, _sportEventCache.Cache.GetCount());

            // No calls to the data router manager
            Assert.Equal(0, _dataRouterManager.GetCallCount(SportEventSummary));

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _sportEventCache.ExportAsync());
            Assert.Equal(exportString, secondExportString);
        }

        private static string SerializeExportables(IEnumerable<ExportableCI> exportables)
        {
            return JsonConvert.SerializeObject(exportables.OrderBy(e => e.Id));
        }
    }
}
