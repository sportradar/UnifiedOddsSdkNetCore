// /*
// * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
// */

using System.Linq;
using System.Runtime.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    /// <summary>
    /// For testing functionality of various caches handling missing fetches
    /// </summary>
    public class SportDataCacheMissTests
    {
        private readonly SportDataCache _sportDataCache;
        private readonly SportEventCache _sportEventCache;
        private readonly TestDataRouterManager _dataRouterManager;

        public SportDataCacheMissTests(ITestOutputHelper outputHelper)
        {
            var memoryCache = new MemoryCache("tournamentDetailsCache");
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

            _sportDataCache = new SportDataCache(_dataRouterManager, timer, TestData.Cultures, _sportEventCache, cacheManager);
        }

        [Fact]
        public void SportDataCacheCorrectlyHandlesFetchMiss()
        {
            var nonExistingTournamentUrn = URN.Parse($"{TestData.SimpleTournamentId}9");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Empty(_sportEventCache.Cache);

            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).GetAwaiter().GetResult(); // initial load
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Empty(_sportEventCache.SpecialTournaments);

            var data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).GetAwaiter().GetResult();
            Assert.Null(data01);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Single(_sportEventCache.SpecialTournaments);

            data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).GetAwaiter().GetResult();
            Assert.Null(data01);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Single(_sportEventCache.SpecialTournaments);

            Assert.NotNull(sports);
            Assert.Null(data01);
        }

        [Fact]
        public void SportDataCacheCorrectlySavesSpecialTournament()
        {
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Empty(_sportEventCache.Cache);

            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var sports = _sportDataCache.GetSportsAsync(TestData.Cultures).GetAwaiter().GetResult(); // initial load
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Empty(_sportEventCache.SpecialTournaments);

            var data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, TestData.Cultures).GetAwaiter().GetResult();
            Assert.NotNull(data01);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Single(_sportEventCache.SpecialTournaments);

            data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, TestData.Cultures).GetAwaiter().GetResult();
            Assert.NotNull(data01);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Single(_sportEventCache.SpecialTournaments);

            Assert.NotNull(sports);
            Assert.NotNull(data01);
        }

        [Fact]
        public void SportDataCacheCorrectlySavesFetchMissWhenCalledManyTimes()
        {
            var nonExistingTournamentUrn = URN.Parse($"{TestData.SimpleTournamentId11111}9");
            Assert.Equal(0, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).GetAwaiter().GetResult();
            for (var i = 0; i < 3; i++)
            {
                data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, TestData.Cultures).GetAwaiter().GetResult();
            }

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));

            Assert.Null(data01);
        }

        [Fact]
        public void SportDataCacheCorrectlySavesNonExistingFetchMissWhenCalledForEachCulture()
        {
            var nonExistingTournamentUrn = URN.Parse($"{TestData.SimpleTournamentId11111}9");
            Assert.Equal(0, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, new[] { TestData.CultureNl }).GetAwaiter().GetResult();
            foreach (var t in TestData.Cultures)
            {
                data01 = _sportDataCache.GetSportForTournamentAsync(nonExistingTournamentUrn, new[] { t }).GetAwaiter().GetResult();
            }

            Assert.Equal(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));

            Assert.Null(data01);
        }

        [Fact]
        public void SportDataCacheCorrectlySavesExistingFetchMissWhenCalledForEachCulture()
        {
            Assert.Equal(0, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            var data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, new[] { TestData.CultureNl }).GetAwaiter().GetResult();
            foreach (var t in TestData.Cultures)
            {
                data01 = _sportDataCache.GetSportForTournamentAsync(TestData.SimpleTournamentId11111, new[] { t }).GetAwaiter().GetResult();
            }

            Assert.Equal(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count + 1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);
            Assert.Equal(TestData.CacheTournamentCount + 1, _sportEventCache.Cache.Count(c => c.Key.Contains("tournament") || c.Key.Contains("season")));

            Assert.NotNull(data01);
            Assert.Equal(URN.Parse("sr:sport:999"), data01.Id);
            Assert.Equal(1, data01.Names.Count);
        }
    }
}
