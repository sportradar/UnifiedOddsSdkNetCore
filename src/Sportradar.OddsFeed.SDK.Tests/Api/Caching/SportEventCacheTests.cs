// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public abstract class SportEventCacheTests : AutoMockerUnitTest
{
    private readonly Urn _sportId = new Urn("sr", "sport", 12345678L);

    private readonly ICacheStore<string> _sportEventMemoryCache;

    private readonly SportEventCache _sportEventCache;

    private readonly CacheManager _cacheManager;
    private readonly ISportEventCacheItemFactory _sportEventCacheItemFactory;
    private readonly Mock<ISdkTimer> _timerMock = new Mock<ISdkTimer>();
    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")] private readonly CultureInfo _culture = CultureInfo.CurrentCulture;
    private readonly CultureInfo[] _cultures = { CultureInfo.CurrentCulture };

    private readonly Mock<IExecutionPathDataProvider<SportEventSummaryDto>> _sportEventSummaryProviderMock = new();
    private readonly Mock<IDataProvider<FixtureDto>> _fixtureProviderMock = new();

    protected SportEventCacheTests(ITestOutputHelper outputHelper)
    {
        var loggerFactory = new XunitLoggerFactory(outputHelper);
        var testCacheStoreManager = new TestCacheStoreManager();
        _sportEventMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache);

        _cacheManager = testCacheStoreManager.CacheManager;
        IDataRouterManager dataRouterManager = BuildDataRouterManager();
        _sportEventCacheItemFactory = new SportEventCacheItemFactory(dataRouterManager,
                                                                     new SemaphorePool(1, ExceptionHandlingStrategy.Throw),
                                                                     testCacheStoreManager.UofConfig,
                                                                     testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache));

        _sportEventCache = new SportEventCache(_sportEventMemoryCache,
                                               dataRouterManager,
                                               _sportEventCacheItemFactory,
                                               _timerMock.Object,
                                               _cultures,
                                               _cacheManager,
                                               loggerFactory);
    }

    public class WhenGetEventSportIdAsyncAndNotInCache : SportEventCacheTests
    {
        private readonly Urn _eventId = new Urn("sr", "match", 12345678L);

        public WhenGetEventSportIdAsyncAndNotInCache(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Then_event_is_retrieved_from_provider_and_added_to_cache()
        {
            _sportEventSummaryProviderMock
               .Setup(x => x.GetDataAsync(It.IsAny<RequestOptions>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(BuildSportEventSummary(_eventId));

            var sportId = await _sportEventCache.GetEventSportIdAsync(_eventId);

            using (new AssertionScope())
            {
                // the sport event should be retrieved from the provider (because it wasn't in the cache)
                _sportEventSummaryProviderMock.Verify(x => x.GetDataAsync(It.IsAny<RequestOptions>(), _eventId.ToString(), "en"), Times.Once);

                // the sport event should now be in the cache
                _sportEventMemoryCache.Get(_eventId.ToString()).Should().NotBeNull();

                sportId.Should().BeEquivalentTo(_sportId);
            }
        }
    }

    public class WhenGetMatchEventAndNotInCache : SportEventCacheTests
    {
        private readonly Urn _eventId = new Urn("sr", "match", 12345678L);

        public WhenGetMatchEventAndNotInCache(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Then_event_is_retrieved_from_provider_and_added_to_cache()
        {
            _sportEventSummaryProviderMock
               .Setup(x => x.GetDataAsync(It.IsAny<RequestOptions>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(BuildMatch(_eventId));

            _fixtureProviderMock
               .Setup(x => x.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(BuildFixture(GetFixtureId(_eventId.Id)));

            var sportId = await _sportEventCache.GetEventSportIdAsync(_eventId);
            var match = (MatchCacheItem)_sportEventCache.GetEventCacheItem(_eventId);

            var fixture = (Fixture)await match.GetFixtureAsync(_cultures);

            using (new AssertionScope())
            {
                // the sport event should be retrieved from the provider (because it wasn't in the cache)
                _sportEventSummaryProviderMock.Verify(x => x.GetDataAsync(It.IsAny<RequestOptions>(), _eventId.ToString(), "en"), Times.Once);

                // the sport event should now be in the cache
                _sportEventMemoryCache.Get(_eventId.ToString()).Should().NotBeNull();

                sportId.Should().BeEquivalentTo(_sportId);

                fixture.Should().NotBeNull();
            }
        }
    }

    public class WhenGetEventSportIdAsyncAndAlreadyInCache : SportEventCacheTests
    {
        private readonly Urn _eventId = new Urn("sr", "match", 12345678L);

        public WhenGetEventSportIdAsyncAndAlreadyInCache(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public async Task Then_event_is_retrieved_from_the_cache()
        {
            var sportEventSummary = BuildSportEventSummary(_eventId);

            var sportEvent = _sportEventCacheItemFactory.Build(sportEventSummary, CultureInfo.CurrentCulture);
            _sportEventMemoryCache.Add(_eventId.ToString(), sportEvent);

            var sportId = await _sportEventCache.GetEventSportIdAsync(_eventId);

            using (new AssertionScope())
            {
                // the sport event should not be retrieved from the provider (because it is in the cache)
                _sportEventSummaryProviderMock.Verify(x => x.GetDataAsync(It.IsAny<RequestOptions>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

                sportId.Should().BeEquivalentTo(_sportId);
            }
        }
    }

    private static Urn GetFixtureId(long id)
    {
        return new Urn("sr", "fixture", id);
    }

    private SportEventSummaryDto BuildSportEventSummary(Urn eventId)
    {
        return new SportEventSummaryDto(new sportEvent
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

    private MatchDto BuildMatch(Urn eventId)
    {
        return new MatchDto(new sportEvent
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

    private FixtureDto BuildFixture(Urn eventId)
    {
        return new FixtureDto(new fixture
        {
            id = eventId.ToString()
        },
                              DateTime.Now);
    }

    private DataRouterManager BuildDataRouterManager()
    {
        var producerMock = new Mock<IProducer>();
        producerMock.Setup(x => x.IsAvailable).Returns(false);
        var producerManagerMock = new Mock<IProducerManager>();
        producerManagerMock.Setup(x => x.GetProducer(7)).Returns(producerMock.Object);

        return new DataRouterManager(_cacheManager,
                                     producerManagerMock.Object,
                                     TestConfiguration.GetConfig(),
                                     _sportEventSummaryProviderMock.Object,
                                     _fixtureProviderMock.Object,
                                     _fixtureProviderMock.Object,
                                     new Mock<IDataProvider<EntityList<SportDto>>>().Object,
                                     new Mock<IDataProvider<EntityList<SportDto>>>().Object,
                                     new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>().Object,
                                     new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>().Object,
                                     new Mock<IDataProvider<PlayerProfileDto>>().Object,
                                     new Mock<IDataProvider<CompetitorProfileDto>>().Object,
                                     new Mock<IDataProvider<SimpleTeamProfileDto>>().Object,
                                     new Mock<IDataProvider<TournamentSeasonsDto>>().Object,
                                     new Mock<IDataProvider<MatchTimelineDto>>().Object,
                                     new Mock<IDataProvider<SportCategoriesDto>>().Object,
                                     new Mock<IDataProvider<EntityList<MarketDescriptionDto>>>().Object,
                                     new Mock<IDataProvider<MarketDescriptionDto>>().Object,
                                     new Mock<IDataProvider<EntityList<VariantDescriptionDto>>>().Object,
                                     new Mock<IDataProvider<DrawDto>>().Object,
                                     new Mock<IDataProvider<DrawDto>>().Object,
                                     new Mock<IDataProvider<LotteryDto>>().Object,
                                     new Mock<IDataProvider<EntityList<LotteryDto>>>().Object,
                                     new Mock<IDataProvider<AvailableSelectionsDto>>().Object,
                                     new Mock<ICalculateProbabilityProvider>().Object,
                                     new Mock<ICalculateProbabilityFilteredProvider>().Object,
                                     new Mock<IDataProvider<EntityList<FixtureChangeDto>>>().Object,
                                     new Mock<IDataProvider<EntityList<ResultChangeDto>>>().Object,
                                     new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>().Object,
                                     new Mock<IDataProvider<EntityList<TournamentInfoDto>>>().Object,
                                     new Mock<IDataProvider<TournamentInfoDto>>().Object,
                                     new Mock<IDataProvider<TournamentInfoDto>>().Object,
                                     new Mock<IDataProvider<PeriodSummaryDto>>().Object,
                                     new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>().Object);
    }
}
