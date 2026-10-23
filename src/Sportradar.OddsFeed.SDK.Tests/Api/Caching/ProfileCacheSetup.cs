// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class ProfileCacheSetup
{
    private protected readonly CacheManager _cacheManager;
    private protected readonly TestDataRouterManager _dataRouterManager;
    private protected readonly ProfileCache _profileCache;
    private protected readonly ICacheStore<string> _profileMemoryCache;
    private protected readonly Mock<ISportEventCache> _sportEventCacheMock;
    private protected readonly XunitLoggerFactory _testLoggerFactory;
    private protected readonly IReadOnlyList<CultureInfo> _cultures;
    private protected readonly Urn _defaultMatchId = Urn.Parse("sr:match:1");

    protected ProfileCacheSetup(ITestOutputHelper outputHelper)
    {
        _testLoggerFactory = new XunitLoggerFactory(outputHelper);
        _sportEventCacheMock = new Mock<ISportEventCache>();
        _cultures = ScheduleData.Cultures3.ToList();

        var memoryCache = new MemoryCache(new MemoryCacheOptions
        {
            TrackStatistics = true,
            TrackLinkedCacheEntries = true,
            CompactionPercentage = 0.2,
            ExpirationScanFrequency = TimeSpan.FromMinutes(10)
        });

        _profileMemoryCache = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache,
                                                     memoryCache,
                                                     _testLoggerFactory.CreateLogger(typeof(Cache)),
                                                     null,
                                                     TestConfiguration.GetConfig().Cache.ProfileCacheTimeout,
                                                     1);
        _cacheManager = new CacheManager();
        _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);

        _profileCache = new ProfileCache(_profileMemoryCache, _dataRouterManager, _cacheManager, _sportEventCacheMock.Object, _testLoggerFactory);
    }

    private protected static Urn CreatePlayerUrn(int playerId)
    {
        return new Urn("sr", "player", playerId);
    }

    private protected static Urn CreateCompetitorUrn(int competitorId)
    {
        return new Urn("sr", "competitor", competitorId);
    }

    private protected static void ValidatePlayer1(PlayerProfileCacheItem player, CultureInfo culture)
    {
        Assert.NotNull(player);

        Assert.Equal("van Persie, Robin", player.GetName(culture));
        Assert.Equal("Netherlands", player.GetNationality(culture));
        Assert.Null(player.Gender);
        Assert.Equal("forward", player.Type);
        Assert.Equal(183, player.Height);
        Assert.Equal(71, player.Weight);
        Assert.Equal("NLD", player.CountryCode);
        Assert.NotNull(player.DateOfBirth);
        Assert.Equal("1983-08-06", player.DateOfBirth.Value.ToString("yyyy-MM-dd"));
    }

    private protected IReadOnlyCollection<CultureInfo> FilterLanguages(int languageIndex = 0)
    {
        return [_cultures[languageIndex]];
    }

    private protected void PrepareSportEventCacheMockForMatchSummaryFetch(Urn sportEventId)
    {
        var mockSportEvent = new Mock<ISportEventCacheItem>();
        mockSportEvent.Setup(s => s.GetNamesAsync(It.IsAny<IEnumerable<CultureInfo>>()))
                      .ReturnsAsync((IEnumerable<CultureInfo> cultures) =>
                                    {
                                        foreach (var culture in cultures)
                                        {
                                            _dataRouterManager.GetSportEventSummaryAsync(sportEventId, culture, null).GetAwaiter().GetResult();
                                        }
                                        return new Mock<IReadOnlyDictionary<CultureInfo, string>>().Object;
                                    });
        _sportEventCacheMock.Setup(s => s.GetEventCacheItem(It.IsAny<Urn>())).Returns(mockSportEvent.Object);
    }

    private protected async Task PopulateCache()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[1], null);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[2], null);

        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), _cultures, true);
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(2), _cultures, true);

        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(5, 10).ToArray();
        var dto = new TournamentInfoDto(api);
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
    }
}
