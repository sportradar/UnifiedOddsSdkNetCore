// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class SportEventStatusCacheTests
{
    private readonly CacheManager _cacheManager;
    private readonly IReadOnlyList<CultureInfo> _cultures;
    private readonly Urn _defaultMatchId = Urn.Parse("sr:match:1");
    private readonly int _greenCardsAway = 2;
    private readonly int _greenCardsHome = 1;

    private readonly ICacheStore<string> _ignoredEventStatusMemoryCache;
    private readonly int _redCardsAway = 4;
    private readonly int _redCardsHome = 3;

    // private protected readonly TestDataRouterManager _dataRouterManager;
    private readonly Mock<ISportEventCache> _sportEventCacheMock;
    private readonly ISportEventStatusCache _sportEventStatusCache;
    private readonly ICacheStore<string> _sportEventStatusMemoryCache;
    private readonly XunitLoggerFactory _testLoggerFactory;
    private readonly int _yellowCardsAway = 6;
    private readonly int _yellowCardsHome = 5;
    private readonly int _yellowRedCardsAway = 8;
    private readonly int _yellowRedCardsHome = 7;

    public SportEventStatusCacheTests(ITestOutputHelper outputHelper)
    {
        _testLoggerFactory = new XunitLoggerFactory(outputHelper);
        _sportEventCacheMock = new Mock<ISportEventCache>();
        _cultures = ScheduleData.Cultures1.ToList();

        _sportEventStatusMemoryCache = CreateCacheStore();
        _ignoredEventStatusMemoryCache = CreateCacheStore();

        _cacheManager = new CacheManager();
        _sportEventStatusCache = new SportEventStatusCache(_sportEventStatusMemoryCache,
                                                           new SportEventStatusMapperFactory(),
                                                           _sportEventCacheMock.Object,
                                                           _cacheManager,
                                                           _ignoredEventStatusMemoryCache,
                                                           TestConfiguration.GetConfig(),
                                                           _testLoggerFactory);
    }

    [Fact]
    public async Task GetSportEventStatusWhenDisposedThenReturnsNull()
    {
        _sportEventStatusCache.Dispose();
        var status = await _sportEventStatusCache.GetSportEventStatusAsync(_defaultMatchId);

        status.Should().BeNull();
    }

    [Fact]
    public async Task GetSportEventStatusWhenCalledWithNullEventIdThenThrowsException()
    {
        var methodCall = () => _sportEventStatusCache.GetSportEventStatusAsync(null);

        await methodCall.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetSportEventStatusWhenCalledWithCachedEventIdThenReturnsCacheItem()
    {
        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        var sportEventStatusDto = GetSportEventStatusDto();
        await _cacheManager.SaveDtoAsync(_defaultMatchId, sportEventStatusDto, _cultures[0], DtoType.SportEventStatus, sportEventCacheItemMock.Object);

        var status = await _sportEventStatusCache.GetSportEventStatusAsync(_defaultMatchId);

        var homeStatistics = status.SportEventStatistics.TotalStatisticsDtos.First(s => s.HomeOrAway == HomeAway.Home);
        var awayStatistics = status.SportEventStatistics.TotalStatisticsDtos.First(s => s.HomeOrAway == HomeAway.Away);

        homeStatistics.GreenCards.Should().NotBeNull();
        awayStatistics.GreenCards.Should().NotBeNull();
        homeStatistics.YellowCards.Should().NotBeNull();
        awayStatistics.YellowCards.Should().NotBeNull();
        homeStatistics.RedCards.Should().NotBeNull();
        awayStatistics.RedCards.Should().NotBeNull();
        homeStatistics.YellowRedCards.Should().NotBeNull();
        awayStatistics.YellowRedCards.Should().NotBeNull();

        homeStatistics.GreenCards!.Value.Should().Be(_greenCardsHome);
        awayStatistics.GreenCards!.Value.Should().Be(_greenCardsAway);
        homeStatistics.RedCards!.Value.Should().Be(_redCardsHome);
        awayStatistics.RedCards!.Value.Should().Be(_redCardsAway);
        homeStatistics.YellowCards!.Value.Should().Be(_yellowCardsHome);
        awayStatistics.YellowCards!.Value.Should().Be(_yellowCardsAway);
        homeStatistics.YellowRedCards!.Value.Should().Be(_yellowRedCardsHome);
        awayStatistics.YellowRedCards!.Value.Should().Be(_yellowRedCardsAway);
    }

    [Fact]
    public async Task GetSportEventStatusWhenCalledWithoutCachedEventItemAndWithCacheEventThenFetchesSportEventStatus()
    {
        var competitionCacheItem = new Mock<ICompetitionCacheItem>();
        _sportEventCacheMock.Setup(m => m.GetEventCacheItem(It.IsAny<Urn>())).Returns(competitionCacheItem.Object);

        await _sportEventStatusCache.GetSportEventStatusAsync(_defaultMatchId);

        competitionCacheItem.Verify(m => m.FetchSportEventStatusAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSportEventStatusWhenCalledWithoutCachedEventItemAndWithCacheEventThenFetchesSportEventStatusAndStoreInCache()
    {
        var sportEventStatusDto = GetSportEventStatusDto();
        var sportEventCacheItemMock = new Mock<ISportEventCacheItem>();
        var competitionCacheItem = new Mock<ICompetitionCacheItem>();
        _sportEventCacheMock.Setup(m => m.GetEventCacheItem(It.IsAny<Urn>()))
                            .Returns(competitionCacheItem.Object);
        competitionCacheItem.Setup(m => m.FetchSportEventStatusAsync())
                            .Callback(() => _cacheManager.SaveDto(_defaultMatchId, sportEventStatusDto, _cultures[0], DtoType.SportEventStatus, sportEventCacheItemMock.Object));

        var status = await _sportEventStatusCache.GetSportEventStatusAsync(_defaultMatchId);

        var homeStatistics = status.SportEventStatistics.TotalStatisticsDtos.First(s => s.HomeOrAway == HomeAway.Home);
        var awayStatistics = status.SportEventStatistics.TotalStatisticsDtos.First(s => s.HomeOrAway == HomeAway.Away);

        homeStatistics.GreenCards.Should().NotBeNull();
        awayStatistics.GreenCards.Should().NotBeNull();
        homeStatistics.YellowCards.Should().NotBeNull();
        awayStatistics.YellowCards.Should().NotBeNull();
        homeStatistics.RedCards.Should().NotBeNull();
        awayStatistics.RedCards.Should().NotBeNull();
        homeStatistics.YellowRedCards.Should().NotBeNull();
        awayStatistics.YellowRedCards.Should().NotBeNull();

        homeStatistics.GreenCards!.Value.Should().Be(_greenCardsHome);
        awayStatistics.GreenCards!.Value.Should().Be(_greenCardsAway);
        homeStatistics.RedCards!.Value.Should().Be(_redCardsHome);
        awayStatistics.RedCards!.Value.Should().Be(_redCardsAway);
        homeStatistics.YellowCards!.Value.Should().Be(_yellowCardsHome);
        awayStatistics.YellowCards!.Value.Should().Be(_yellowCardsAway);
        homeStatistics.YellowRedCards!.Value.Should().Be(_yellowRedCardsHome);
        awayStatistics.YellowRedCards!.Value.Should().Be(_yellowRedCardsAway);
    }

    [Fact]
    public async Task GetSportEventStatusWhenCalledWithoutCachedEventItemAndEmptyStatusCacheImteIsCreated()
    {
        var competitionCacheItem = new Mock<ICompetitionCacheItem>();
        _sportEventCacheMock.Setup(m => m.GetEventCacheItem(It.IsAny<Urn>()))
                            .Returns(competitionCacheItem.Object);
        competitionCacheItem.Setup(m => m.FetchSportEventStatusAsync());

        var status = await _sportEventStatusCache.GetSportEventStatusAsync(_defaultMatchId);

        status.SportEventStatistics.Should().BeNull();
    }

    [Fact]
    public async Task GetSportEventStatusWhenFetchSportEventStatusThrowsExceptionShouldThrowException()
    {
        var competitionCacheItem = new Mock<ICompetitionCacheItem>();
        _sportEventCacheMock.Setup(m => m.GetEventCacheItem(It.IsAny<Urn>()))
                            .Returns(competitionCacheItem.Object);
        competitionCacheItem.Setup(m => m.FetchSportEventStatusAsync()).Throws(new Exception("Test exception"));

        var getStatusAction = () => _sportEventStatusCache.GetSportEventStatusAsync(_defaultMatchId);

        await getStatusAction.Should().ThrowAsync<Exception>();
    }

    private SportEventStatusDto GetSportEventStatusDto()
    {
        var sportEventStatusDto = new SportEventStatusDto(new sportEventStatus
        {
            statistics = new statisticsType
            {
                green_cards = new statisticsScoreType { home = _greenCardsHome, away = _greenCardsAway },
                red_cards = new statisticsScoreType { home = _redCardsHome, away = _redCardsAway },
                yellow_cards = new statisticsScoreType { home = _yellowCardsHome, away = _yellowCardsAway },
                yellow_red_cards = new statisticsScoreType { home = _yellowRedCardsHome, away = _yellowRedCardsAway }
            }
        },
                                                          new Dictionary<HomeAway, Urn> { { HomeAway.Home, Urn.Parse("sr:competitor:375570") }, { HomeAway.Away, Urn.Parse("sr:competitor:372488") } });
        return sportEventStatusDto;
    }

    private CacheStore<string> CreateCacheStore()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true, TrackLinkedCacheEntries = true, CompactionPercentage = 0.2, ExpirationScanFrequency = TimeSpan.FromMinutes(10) });

        return new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache,
                                      memoryCache,
                                      _testLoggerFactory.CreateLogger(typeof(Cache)),
                                      null,
                                      TestConfiguration.GetConfig().Cache.ProfileCacheTimeout,
                                      1);
    }
}
