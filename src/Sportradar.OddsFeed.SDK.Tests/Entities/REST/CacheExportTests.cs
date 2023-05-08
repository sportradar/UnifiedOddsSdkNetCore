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
            _profileCache = new ProfileCache(_profileMemoryCache, _dataRouterManager, cacheManager, _sportEventCache);
        }

        [Fact]
        public async Task SportDataCacheStatus()
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
        public async Task SportDataCacheEmptyExport()
        {
            var export = (await _sportDataCache.ExportAsync()).ToList();
            export.Count.Should().Be(0);

            await _sportDataCache.ImportAsync(export);
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
        }

        [Fact]
        public async Task SportDataCacheFullExport()
        {
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures); // initial load
            Assert.NotNull(sports);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var export = (await _sportDataCache.ExportAsync()).ToList();
            Assert.Equal(TestData.CacheSportCount + TestData.CacheCategoryCountPlus, export.Count);

            _sportDataCache.Sports.Clear();
            _sportDataCache.Categories.Clear();
            _sportDataCache.FetchedCultures.Clear();

            await _sportDataCache.ImportAsync(export);
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);

            // No calls to the data router manager
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _sportDataCache.ExportAsync());
            Assert.Equal(exportString, secondExportString);
        }

        [Fact]
        public async Task ProfileCacheStatus()
        {
            var status = _profileCache.CacheStatus();
            Assert.Equal(0, status["TeamCompetitorCI"]);
            Assert.Equal(0, status["CompetitorCI"]);
            Assert.Equal(0, status["PlayerProfileCI"]);

            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), TestData.Cultures, false);
            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:2"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:2"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:1"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:2"), TestData.Cultures, false);

            status = _profileCache.CacheStatus();
            Assert.Equal(0, status["TeamCompetitorCI"]);
            Assert.Equal(4, status["CompetitorCI"]);
            Assert.Equal(62, status["PlayerProfileCI"]);
            Assert.Equal(_profileMemoryCache.GetCount(), status.Sum(i => i.Value));
        }

        [Fact]
        public async Task ProfileCacheEmptyExport()
        {
            var export = (await _profileCache.ExportAsync()).ToList();
            export.Count.Should().Be(0);

            await _profileCache.ImportAsync(export);
            Assert.Equal(0, _profileMemoryCache.GetCount());
        }

        [Fact]
        public async Task ProfileCacheFullExport()
        {
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), TestData.Cultures, false);
            await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:2"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:1"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:competitor:2"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:1"), TestData.Cultures, false);
            await _profileCache.GetCompetitorProfileAsync(URN.Parse("sr:simpleteam:2"), TestData.Cultures, false);

            Assert.Equal(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var export = (await _profileCache.ExportAsync()).ToList();
            Assert.Equal(_profileMemoryCache.GetCount(), export.Count);

            _profileMemoryCache.ToList().ForEach(i => _profileMemoryCache.Remove(i.Key));

            await _profileCache.ImportAsync(export);
            Assert.Equal(export.Count, _profileMemoryCache.GetCount());

            // No calls to the data router manager
            Assert.Equal(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

            var exportString = SerializeExportables(export);
            var secondExportString = SerializeExportables(await _profileCache.ExportAsync());
            Assert.Equal(exportString, secondExportString);
        }

        [Fact]
        public async Task SportEventCacheStatus()
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
        public async Task SportEventCacheEmptyExport()
        {
            var export = (await _sportEventCache.ExportAsync()).ToList();
            export.Count.Should().Be(0);

            await _sportDataCache.ImportAsync(export);
            Assert.Equal(0, _sportEventCache.Cache.GetCount());
        }

        [Fact]
        public async Task SportEventCacheFullExport()
        {
            var tournaments = _sportEventCache.GetActiveTournamentsAsync().GetAwaiter().GetResult(); // initial load
            Assert.NotNull(tournaments);

            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var export = (await _sportEventCache.ExportAsync()).ToList();
            Assert.Equal(1187, export.Count);

            _sportEventCache.Cache.ToList().ForEach(i => _sportEventCache.Cache.Remove(i.Key));

            await _sportEventCache.ImportAsync(export);
            Assert.Equal(1187, _sportEventCache.Cache.GetCount());

            // No calls to the data router manager
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

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
