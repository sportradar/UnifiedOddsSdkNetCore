/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Messages;
using Xunit.Abstractions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedVariable

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestSportEntityFactoryBuilder
    {
        private readonly ITestOutputHelper _outputHelper;
        internal readonly SportEventCache SportEventCache;
        internal readonly SportDataCache SportDataCache;
        internal readonly ISportEventStatusCache EventStatusCache;
        internal readonly SportEntityFactory SportEntityFactory;

        public ICompetition Competition;
        public ITournament Tournament;
        public ISeason Season;
        public ISport Sport;
        public List<ISport> Sports;

        public TestSportEntityFactoryBuilder(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            var timer = new TestTimer(false);
            var eventMemoryCache = new MemoryCache("EventCache");
            var profileCache1 = new MemoryCache("ProfileCache");
            var statusMemoryCache = new MemoryCache("StatusCache");
            var ignoreTimelineMemoryCache = new MemoryCache("IgnoreTimeline");

            var cacheManager = new CacheManager();
            IDataRouterManager dataRouterManager = new TestDataRouterManager(cacheManager, _outputHelper);

            var sportEventCacheItemFactory = new SportEventCacheItemFactory(
                dataRouterManager,
                new SemaphorePool(5, ExceptionHandlingStrategy.THROW),
                TestData.Culture,
                new MemoryCache("FixtureTimestampCache"));

            var profileCache = new ProfileCache(profileCache1, dataRouterManager, cacheManager);

            SportEventCache = new SportEventCache(
                eventMemoryCache, dataRouterManager, sportEventCacheItemFactory, timer, TestData.Cultures3, cacheManager);

            SportDataCache = new SportDataCache(dataRouterManager, timer, TestData.Cultures3, SportEventCache, cacheManager);

            var sportEventStatusCache = TestLocalizedNamedValueCache.CreateMatchStatusCache(TestData.Cultures3, ExceptionHandlingStrategy.THROW);
            var namedValuesProviderMock = new Mock<INamedValuesProvider>();
            namedValuesProviderMock.Setup(args => args.MatchStatuses).Returns(sportEventStatusCache);

            EventStatusCache = new SportEventStatusCache(
                statusMemoryCache, new SportEventStatusMapperFactory(),
                SportEventCache, cacheManager, ignoreTimelineMemoryCache);

            SportEntityFactory = new SportEntityFactory(
                SportDataCache, SportEventCache, EventStatusCache,
                sportEventStatusCache, profileCache, SdkInfo.SoccerSportUrns);
        }

        public async Task InitializeSportEntities()
        {
            var watch = Stopwatch.StartNew();
            if (SportEntityFactory == null)
            {
                return;
            }
            var time = watch.Elapsed.TotalMilliseconds;
            _outputHelper.WriteLine($"Init time: {time}");
            Competition = SportEntityFactory.BuildSportEvent<ICompetition>(TestData.EventMatchId, URN.Parse("sr:sport:3"), TestData.Cultures3.ToList(), TestData.ThrowingStrategy);
            _outputHelper.WriteLine($"Competition time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Sport = await SportEntityFactory.BuildSportAsync(TestData.SportId, TestData.Cultures3, TestData.ThrowingStrategy);
            _outputHelper.WriteLine($"Sport time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Sports = (await SportEntityFactory.BuildSportsAsync(TestData.Cultures3, TestData.ThrowingStrategy)).ToList();
            _outputHelper.WriteLine($"Sports time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Tournament = SportEntityFactory.BuildSportEvent<ITournament>(TestData.TournamentId, TestData.SportId, TestData.Cultures3.ToList(), TestData.ThrowingStrategy);
            _outputHelper.WriteLine($"Tournament time: {watch.Elapsed.TotalMilliseconds - time}");
            time = watch.Elapsed.TotalMilliseconds;
            Season = SportEntityFactory.BuildSportEvent<ISeason>(TestData.SeasonId, TestData.SportId, TestData.Cultures3.ToList(), TestData.ThrowingStrategy);
            _outputHelper.WriteLine($"Season time: {watch.Elapsed.TotalMilliseconds - time}");
        }

        public async Task LoadTournamentMissingValues()
        {
            _ = await Tournament.GetScheduledTimeAsync();
            _ = await Tournament.GetScheduledEndTimeAsync();
            _ = await Tournament.GetCategoryAsync();
            _ = await Tournament.GetTournamentCoverage();
            _ = await Tournament.GetCurrentSeasonAsync();
        }

        public async Task LoadSeasonMissingValues()
        {
            _ = await Season.GetScheduledTimeAsync();
            _ = await Season.GetScheduledEndTimeAsync();
            _ = await Season.GetTournamentCoverage();
            _ = await Season.GetCurrentRoundAsync();
            _ = await Season.GetSeasonCoverageAsync();
            _ = await Season.GetScheduleAsync();
            _ = await Season.GetGroupsAsync();
            _ = await Season.GetYearAsync();
        }

        public ITournament GetNewTournament(int id = 0)
        {
            return Tournament;
        }
    }
}
