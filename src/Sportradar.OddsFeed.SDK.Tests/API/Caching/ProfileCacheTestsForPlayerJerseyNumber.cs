// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class ProfileCacheTestsForPlayerJerseyNumber
{
    private readonly Mock<IDataProvider<CompetitorProfileDto>> _competitorProfileProviderMock;
    private readonly Mock<IDataProvider<PlayerProfileDto>> _playerProfileProviderMock;
    private readonly Mock<IDataProvider<SportEventSummaryDto>> _sportEventSummaryProviderMock;
    private readonly DataRouterManager _dataRouterManager;
    private readonly Urn _defaultCompetitorId = Urn.Parse("sr:competitor:1");
    private readonly Urn _defaultPlayerId = Urn.Parse("sr:player:5");
    private readonly ProfileCache _profileCache;

    public ProfileCacheTestsForPlayerJerseyNumber(ITestOutputHelper outputHelper)
    {
        var testLoggerFactory = new XunitLoggerFactory(outputHelper);
        var sportEventCacheMock = new Mock<ISportEventCache>();

        var memoryCache = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true, TrackLinkedCacheEntries = true, CompactionPercentage = 0.2, ExpirationScanFrequency = TimeSpan.FromMinutes(10) });

        ICacheStore<string> profileMemoryCache = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache,
                                                                        memoryCache,
                                                                        testLoggerFactory.CreateLogger(typeof(CacheStore<string>)),
                                                                        null,
                                                                        TestConfiguration.GetConfig().Cache.ProfileCacheTimeout,
                                                                        1);

        _competitorProfileProviderMock = new Mock<IDataProvider<CompetitorProfileDto>>();
        _playerProfileProviderMock = new Mock<IDataProvider<PlayerProfileDto>>();
        _sportEventSummaryProviderMock = new Mock<IDataProvider<SportEventSummaryDto>>();
        var productManagerMock = GetProductManagerMock();

        var cacheManager = new CacheManager();
        _dataRouterManager = new DataRouterManagerBuilder()
                            .AddMockedDependencies()
                            .WithCacheManager(cacheManager)
                            .WithPlayerProfileProvider(_playerProfileProviderMock.Object)
                            .WithCompetitorProvider(_competitorProfileProviderMock.Object)
                            .WithSportEventSummaryProvider(_sportEventSummaryProviderMock.Object)
                            .WithProducerManager(productManagerMock.Object)
                            .Build();

        _profileCache = new ProfileCache(profileMemoryCache, _dataRouterManager, cacheManager, sportEventCacheMock.Object, testLoggerFactory);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenCalledWithCompetitorContainingPlayerWithJerseyThenReturnJerseyNumber()
    {
        const int jerseyNumber = 5;
        _competitorProfileProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(GetCompetitorProfileContainingPlayerWithJerseyNumber(jerseyNumber));

        var competitorCi = await _profileCache.GetCompetitorProfileAsync(_defaultCompetitorId, TestData.Cultures1, true);

        Assert.Contains(_defaultPlayerId, competitorCi.AssociatedPlayerIds);
        Assert.Contains(_defaultPlayerId, competitorCi.AssociatedPlayersJerseyNumbers.Keys);
        competitorCi.AssociatedPlayersJerseyNumbers[_defaultPlayerId].Should().Be(jerseyNumber);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenCalledWithCompetitorContainingPlayerWithoutJerseyThenNoJerseyNumber()
    {
        _competitorProfileProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(GetCompetitorProfileContainingPlayerWithJerseyNumber(null));

        var competitorCi = await _profileCache.GetCompetitorProfileAsync(_defaultCompetitorId, TestData.Cultures1, true);

        Assert.Contains(_defaultPlayerId, competitorCi.AssociatedPlayerIds);
        Assert.DoesNotContain(_defaultPlayerId, competitorCi.AssociatedPlayersJerseyNumbers.Keys);
    }

    [Fact]
    public async Task GetPlayerProfileWhenCalledWithCompetitorAlreadyFetchedThenNotRequestDataFromPlayerProvider()
    {
        const int jerseyNumber = 5;

        _competitorProfileProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(GetCompetitorProfileContainingPlayerWithJerseyNumber(jerseyNumber));

        await _dataRouterManager.GetCompetitorAsync(_defaultCompetitorId, TestData.Culture, null);
        await _profileCache.GetPlayerProfileAsync(_defaultPlayerId, TestData.Cultures1, true);

        _playerProfileProviderMock.Verify(provider => provider.GetDataAsync(_defaultPlayerId.ToString(), TestData.Culture.TwoLetterISOLanguageName), Times.Never);
    }

    [Fact]
    public async Task GetCompetitorProfileWhenPreloadedWithMatchCompetitorsThenRequestCompetitorProfile()
    {
        var api = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(2, 0).ToArray();
        api.sport_event.competitors[0].id = _defaultCompetitorId.ToString();
        var matchDto = new MatchDto(api);

        _sportEventSummaryProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(matchDto);
        const int jerseyNumber = 5;
        _competitorProfileProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetCompetitorProfileContainingPlayerWithJerseyNumber(jerseyNumber));

        await _dataRouterManager.GetSportEventSummaryAsync(matchDto.Id, TestData.Culture, null);
        var competitorCi = await _profileCache.GetCompetitorProfileAsync(_defaultCompetitorId, TestData.Cultures1, false);
        var jerseyNumbers = competitorCi.AssociatedPlayersJerseyNumbers;

        Assert.NotEmpty(jerseyNumbers);
        _competitorProfileProviderMock.Verify(provider => provider.GetDataAsync(_defaultCompetitorId.ToString(), TestData.Culture.TwoLetterISOLanguageName), Times.Once);
        _playerProfileProviderMock.Verify(provider => provider.GetDataAsync(_defaultPlayerId.ToString(), TestData.Culture.TwoLetterISOLanguageName), Times.Never);
    }

    private CompetitorProfileDto GetCompetitorProfileContainingPlayerWithJerseyNumber(int? jerseyNumber)
    {
        return new CompetitorProfileDto(new competitorProfileEndpoint
        {
            competitor = new teamExtended { id = _defaultCompetitorId.ToString(), players = new[] { new playerCompetitor { id = _defaultPlayerId.ToString() } } }, players = new[] { GetPlayerWithJersey(jerseyNumber) }
        });
    }

    private playerExtended GetPlayerWithJersey(int? jerseyNumber)
    {
        return new playerExtended { id = _defaultPlayerId.ToString(), jersey_numberSpecified = jerseyNumber.HasValue, jersey_number = jerseyNumber.GetValueOrDefault() };
    }

    private static Mock<IProducerManager> GetProductManagerMock()
    {
        var productManagerMock = new Mock<IProducerManager>();
        var producerMock = new Mock<IProducer>();
        producerMock.Setup(m => m.IsAvailable).Returns(false);

        productManagerMock.Setup(m => m.GetProducer(It.IsAny<int>())).Returns(producerMock.Object);

        return productManagerMock;
    }
}
