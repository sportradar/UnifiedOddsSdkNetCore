using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Timer;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using ITimer = Sportradar.OddsFeed.SDK.Common.Internal.ITimer;

namespace Sportradar.OddsFeed.SDK.Tests.Caching
{
    public abstract class SportEventCacheTests : AutoMockerUnitTest
    {
        private readonly URN _sportId = new URN("sr", "sport", 12345678L);

        private readonly MemoryCache _sportEventMemoryCache;

        private readonly SportEventCache _sportEventCache;

        private readonly CacheManager _cacheManager;
        private readonly IDataRouterManager _dataRouterManager;
        private readonly ISportEventCacheItemFactory _sportEventCacheItemFactory;
        private readonly Mock<ITimer> _timerMock = new Mock<ITimer>();
        private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
        private readonly CultureInfo[] _cultures = { CultureInfo.CurrentCulture };

        private readonly Mock<IDataProvider<SportEventSummaryDTO>> _sportEventSummaryProviderMock = new Mock<IDataProvider<SportEventSummaryDTO>>();
        private readonly Mock<IDataProvider<FixtureDTO>> _fixtureProviderMock = new Mock<IDataProvider<FixtureDTO>>();

        protected SportEventCacheTests()
        {
            _sportEventMemoryCache = new MemoryCache("SportEventCache");
            _ = new MemoryCache("SportEventStatusCache");
            _ = new MemoryCache("IgnoreEventsMemoryCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = BuildDataRouterManager();
            _sportEventCacheItemFactory = BuildSportEventCacheItemFactory();

            _sportEventCache = new SportEventCache(
                _sportEventMemoryCache,
                _dataRouterManager,
                _sportEventCacheItemFactory,
                _timerMock.Object,
                _cultures,
                _cacheManager);
        }

        public class WhenGetEventSportIdAsyncAndNotInCache : SportEventCacheTests
        {
            private readonly URN _eventId = new URN("sr", "match", 12345678L);

            [Fact]
            public async Task Then_event_is_retrieved_from_provider_and_added_to_cache()
            {
                _sportEventSummaryProviderMock
                    .Setup(x => x.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(BuildSportEventSummary(_eventId));

                var sportId = await _sportEventCache.GetEventSportIdAsync(_eventId);

                using (new AssertionScope())
                {
                    // the sport event should be retrieved from the provider (because it wasn't in the cache)
                    _sportEventSummaryProviderMock.Verify(x => x.GetDataAsync(_eventId.ToString(), "en"), Times.Once);

                    // the sport event should now be in the cache
                    _sportEventMemoryCache.Get(_eventId.ToString()).Should().NotBeNull();

                    sportId.Should().BeEquivalentTo(_sportId);
                }
            }
        }

        public class WhenGetMatchEventAndNotInCache : SportEventCacheTests
        {
            private readonly URN _eventId = new URN("sr", "match", 12345678L);

            [Fact]
            public async Task Then_event_is_retrieved_from_provider_and_added_to_cache()
            {
                _sportEventSummaryProviderMock
                    .Setup(x => x.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(BuildMatch(_eventId));

                _fixtureProviderMock
                    .Setup(x => x.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(BuildFixture(GetFixtureId(_eventId.Id)));

                var sportId = await _sportEventCache.GetEventSportIdAsync(_eventId);
                var match = (MatchCI)_sportEventCache.GetEventCacheItem(_eventId);

                var fixture = (Fixture)await match.GetFixtureAsync(_cultures);

                using (new AssertionScope())
                {
                    // the sport event should be retrieved from the provider (because it wasn't in the cache)
                    _sportEventSummaryProviderMock.Verify(x => x.GetDataAsync(_eventId.ToString(), "en"), Times.Once);

                    // the sport event should now be in the cache
                    _sportEventMemoryCache.Get(_eventId.ToString()).Should().NotBeNull();

                    sportId.Should().BeEquivalentTo(_sportId);

                    fixture.Should().NotBeNull();
                }
            }
        }

        public class WhenGetEventSportIdAsyncAndAlreadyInCache : SportEventCacheTests
        {
            private readonly URN _eventId = new URN("sr", "match", 12345678L);

            [Fact]
            public async Task Then_event_is_retrieved_from_the_cache()
            {
                var sportEventSummary = BuildSportEventSummary(_eventId);

                var sportEvent = _sportEventCacheItemFactory.Build(sportEventSummary, CultureInfo.CurrentCulture);
                _sportEventMemoryCache.Add(_eventId.ToString(), sportEvent, DateTimeOffset.MaxValue);

                var sportId = await _sportEventCache.GetEventSportIdAsync(_eventId);

                using (new AssertionScope())
                {
                    // the sport event should not be retrieved from the provider (because it is in the cache)
                    _sportEventSummaryProviderMock.Verify(x => x.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

                    sportId.Should().BeEquivalentTo(_sportId);
                }
            }
        }

        private static URN GetFixtureId(long id) => new URN("sr", "fixture", id);

        private SportEventSummaryDTO BuildSportEventSummary(URN eventId)
        {
            return new SportEventSummaryDTO(new sportEvent
            {
                id = eventId.ToString(),
                name = $"test event name {eventId}",
                status = "test event status",
                scheduledSpecified = false,
                scheduled_endSpecified = false,
                start_time_tbdSpecified = false,
                tournament = new tournament
                {
                    sport = new sport
                    {
                        id = _sportId.ToString()
                    }
                }
            });
        }

        private MatchDTO BuildMatch(URN eventId)
        {
            return new MatchDTO(new sportEvent
            {
                id = eventId.ToString(),
                name = $"test event name {eventId}",
                status = "test event status",
                scheduledSpecified = false,
                scheduled_endSpecified = false,
                start_time_tbdSpecified = false,
                tournament = new tournament
                {
                    id = $"vf:tournament:{eventId.Id}",
                    name = "a tournament",
                    sport = new sport
                    {
                        id = _sportId.ToString()
                    }
                }
            });
        }

        private FixtureDTO BuildFixture(URN eventId)
        {
            return new FixtureDTO(new fixture
            {
                id = eventId.ToString()
            }, DateTime.Now);
        }

        private DataRouterManager BuildDataRouterManager()
        {
            var producerMock = new Mock<IProducer>();
            producerMock.Setup(x => x.IsAvailable).Returns(false);
            var producerManagerMock = new Mock<IProducerManager>();
            producerManagerMock.Setup(x => x.Get(7)).Returns(producerMock.Object);

            var metricsRootMock = new Mock<IMetricsRoot>();
            var measureMetrics = new Mock<IMeasureMetrics>();
            var measureTimerMetrics = new Mock<IMeasureTimerMetrics>();
            measureMetrics.Setup(x => x.Timer).Returns(measureTimerMetrics.Object);
            metricsRootMock.Setup(x => x.Measure).Returns(measureMetrics.Object);

            return new DataRouterManager(
                _cacheManager,
                producerManagerMock.Object,
                metricsRootMock.Object,
                ExceptionHandlingStrategy.THROW,
                _culture,
                _sportEventSummaryProviderMock.Object,
                _fixtureProviderMock.Object,
                _fixtureProviderMock.Object,
                new Mock<IDataProvider<EntityList<SportDTO>>>().Object,
                new Mock<IDataProvider<EntityList<SportDTO>>>().Object,
                new Mock<IDataProvider<EntityList<SportEventSummaryDTO>>>().Object,
                new Mock<IDataProvider<EntityList<SportEventSummaryDTO>>>().Object,
                new Mock<IDataProvider<PlayerProfileDTO>>().Object,
                new Mock<IDataProvider<CompetitorProfileDTO>>().Object,
                new Mock<IDataProvider<SimpleTeamProfileDTO>>().Object,
                new Mock<IDataProvider<TournamentSeasonsDTO>>().Object,
                new Mock<IDataProvider<MatchTimelineDTO>>().Object,
                new Mock<IDataProvider<SportCategoriesDTO>>().Object,
                new Mock<IDataProvider<EntityList<MarketDescriptionDTO>>>().Object,
                new Mock<IDataProvider<MarketDescriptionDTO>>().Object,
                new Mock<IDataProvider<EntityList<VariantDescriptionDTO>>>().Object,
                new Mock<IDataProvider<DrawDTO>>().Object,
                new Mock<IDataProvider<DrawDTO>>().Object,
                new Mock<IDataProvider<LotteryDTO>>().Object,
                new Mock<IDataProvider<EntityList<LotteryDTO>>>().Object,
                new Mock<IDataProvider<AvailableSelectionsDto>>().Object,
                new Mock<ICalculateProbabilityProvider>().Object,
                new Mock<ICalculateProbabilityFilteredProvider>().Object,
                new Mock<IDataProvider<IEnumerable<FixtureChangeDTO>>>().Object,
                new Mock<IDataProvider<IEnumerable<ResultChangeDTO>>>().Object,
                new Mock<IDataProvider<EntityList<SportEventSummaryDTO>>>().Object,
                new Mock<IDataProvider<EntityList<TournamentInfoDTO>>>().Object,
                new Mock<IDataProvider<TournamentInfoDTO>>().Object,
                new Mock<IDataProvider<TournamentInfoDTO>>().Object,
                new Mock<IDataProvider<PeriodSummaryDTO>>().Object,
                new Mock<IDataProvider<EntityList<SportEventSummaryDTO>>>().Object);
        }

        private SportEventCacheItemFactory BuildSportEventCacheItemFactory()
        {
            return new SportEventCacheItemFactory(
                _dataRouterManager,
                new SemaphorePool(1, ExceptionHandlingStrategy.THROW),
                _cultures[0],
                new MemoryCache("fixtureTimestampCache"));
        }
    }
}
