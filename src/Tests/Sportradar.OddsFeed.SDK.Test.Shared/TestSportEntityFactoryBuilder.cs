/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using Moq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Messages;
// ReSharper disable UnusedVariable

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestSportEntityFactoryBuilder
    {
        private MemoryCache _eventMemoryCache;
        private MemoryCache _profileCache;
        private MemoryCache _statusMemoryCache;
        private MemoryCache _ignoreTimelineMemoryCache;
        internal SportEventCache SportEventCache;
        internal SportDataCache SportDataCache;
        internal ISportEventStatusCache EventStatusCache;

        private TestTimer _timer;

        internal SportEntityFactory SportEntityFactory;

        public ICompetition Competition;
        public ITournament Tournament;
        public ISeason Season;
        public ISport Sport;
        public List<ISport> Sports;
        private CacheManager _cacheManager;
        private IDataRouterManager _dataRouterManager;

        public void Init()
        {
            _timer = new TestTimer(false);
            _eventMemoryCache = new MemoryCache("EventCache");
            _profileCache = new MemoryCache("ProfileCache");
            _statusMemoryCache = new MemoryCache("StatusCache");
            _ignoreTimelineMemoryCache = new MemoryCache("IgnoreTimeline");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            var sportEventCacheItemFactory = new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5), TestData.Culture, new MemoryCache("FixtureTimestampCache"));
            var profileCache = new ProfileCache(_profileCache, _dataRouterManager, _cacheManager);
            SportEventCache = new SportEventCache(_eventMemoryCache, _dataRouterManager, sportEventCacheItemFactory, _timer, TestData.Cultures3, _cacheManager);
            SportDataCache = new SportDataCache(_dataRouterManager, _timer, TestData.Cultures3, SportEventCache, _cacheManager);

            var sportEventStatusCache = new TestLocalizedNamedValueCache();
            var namedValuesProviderMock = new Mock<INamedValuesProvider>();
            namedValuesProviderMock.Setup(args => args.MatchStatuses).Returns(sportEventStatusCache);

            EventStatusCache = new SportEventStatusCache(_statusMemoryCache, new SportEventStatusMapperFactory(), SportEventCache, _cacheManager, _ignoreTimelineMemoryCache);

            SportEntityFactory = new SportEntityFactory(SportDataCache, SportEventCache, EventStatusCache, sportEventStatusCache, profileCache, SdkInfo.SoccerSportUrns);
        }

        public void InitializeSportEntities()
        {
            var watch = Stopwatch.StartNew();
            if (SportEntityFactory == null)
            {
                Init();
            }
            var time = watch.Elapsed.TotalMilliseconds;
            Debug.WriteLine($"Init time: {time}");
            Competition = SportEntityFactory.BuildSportEvent<ICompetition>(TestData.EventId, URN.Parse("sr:sport:3"), TestData.Cultures3, TestData.ThrowingStrategy);
            Debug.WriteLine($"Competition time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Sport = SportEntityFactory.BuildSportAsync(TestData.SportId, TestData.Cultures3, TestData.ThrowingStrategy).Result;
            Debug.WriteLine($"Sport time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Sports = SportEntityFactory.BuildSportsAsync(TestData.Cultures3, TestData.ThrowingStrategy).Result?.ToList();
            Debug.WriteLine($"Sports time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Tournament = SportEntityFactory.BuildSportEvent<ITournament>(TestData.TournamentId, TestData.SportId, TestData.Cultures3, TestData.ThrowingStrategy);
            Debug.WriteLine($"Tournament time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Season = SportEntityFactory.BuildSportEvent<ISeason>(TestData.SeasonId, TestData.SportId, TestData.Cultures3, TestData.ThrowingStrategy);
            Debug.WriteLine($"Season time: {watch.Elapsed.TotalMilliseconds - time}");
        }

        public void LoadTournamentMissingValues()
        {
            var a = Tournament.GetScheduledTimeAsync().Result;
            var b = Tournament.GetScheduledEndTimeAsync().Result;
            var c = Tournament.GetCategoryAsync().Result;
            var d = Tournament.GetTournamentCoverage().Result;
            var e = Tournament.GetCurrentSeasonAsync().Result;
            //var s = Tournament.GetScheduleAsync().Result;
            //var g = Tournament.GetGroupsAsync().Result;
        }

        public void LoadSeasonMissingValues()
        {
            var a = Season.GetScheduledTimeAsync().Result;
            var b = Season.GetScheduledEndTimeAsync().Result;
            var c = Season.GetTournamentCoverage().Result;
            var d = Season.GetCurrentRoundAsync().Result;
            var e = Season.GetSeasonCoverageAsync().Result;
            var s = Season.GetScheduleAsync().Result;
            var g = Season.GetGroupsAsync().Result;
            var f = Season.GetYearAsync().Result;
        }

        public ITournament GetNewTournament(int id = 0)
        {
            return Tournament;
        }
    }
}
