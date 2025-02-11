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
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class ProfileCacheVirtualCompetitorsTests
{
    private readonly CacheManager _cacheManager;
    private readonly Mock<IDataProvider<CompetitorProfileDto>> _competitorProfileProviderMock;
    private readonly DataRouterManager _dataRouterManager;
    private readonly Urn _defaultMatchId = Urn.Parse("sr:match:1");
    private readonly Mock<IProducerManager> _productManagerMock;
    private readonly ProfileCache _profileCache;
    private readonly ICacheStore<string> _profileMemoryCache;
    private readonly Mock<ISportEventCache> _sportEventCacheMock;
    private readonly Mock<IDataProvider<SportEventSummaryDto>> _sportEventSummaryProviderMock;
    private readonly XunitLoggerFactory _testLoggerFactory;

    public ProfileCacheVirtualCompetitorsTests(ITestOutputHelper outputHelper)
    {
        _testLoggerFactory = new XunitLoggerFactory(outputHelper);
        _sportEventCacheMock = new Mock<ISportEventCache>();

        var memoryCache = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true, TrackLinkedCacheEntries = true, CompactionPercentage = 0.2, ExpirationScanFrequency = TimeSpan.FromMinutes(10) });

        _profileMemoryCache = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache,
                                                     memoryCache,
                                                     _testLoggerFactory.CreateLogger(typeof(CacheStore<string>)),
                                                     null,
                                                     TestConfiguration.GetConfig().Cache.ProfileCacheTimeout,
                                                     1);

        _sportEventSummaryProviderMock = new Mock<IDataProvider<SportEventSummaryDto>>();
        _competitorProfileProviderMock = new Mock<IDataProvider<CompetitorProfileDto>>();
        _productManagerMock = GetProductManagerMock();

        _cacheManager = new CacheManager();
        _dataRouterManager = new DataRouterManagerBuilder()
                            .AddMockedDependencies()
                            .WithCacheManager(_cacheManager)
                            .WithProducerManager(_productManagerMock.Object)
                            .WithSportEventSummaryProvider(_sportEventSummaryProviderMock.Object)
                            .WithCompetitorProvider(_competitorProfileProviderMock.Object)
                            .Build();

        _profileCache = new ProfileCache(_profileMemoryCache, _dataRouterManager, _cacheManager, _sportEventCacheMock.Object, _testLoggerFactory);
    }

    [Fact]
    public async Task GetSportEventSummaryWhenCalledWithVirtualCompetitorsShouldSetVirtualFlagTrue()
    {
        var matchDtoWithVirtualCompetitors = GetMatchDtoWithCompetitors(true, true);
        _sportEventSummaryProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(matchDtoWithVirtualCompetitors);

        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, TestConfiguration.GetConfig().DefaultLanguage, null);
        var profile = await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:4698"), new[] { TestConfiguration.GetConfig().DefaultLanguage }, true);

        profile.IsVirtual.Should().BeTrue();
    }

    [Fact]
    public async Task GetSportEventSummaryWhenCalledWithoutVirtualCompetitorsShouldSetVirtualFlagFalse()
    {
        var matchDtoWithNoVirtualCompetitors = GetMatchDtoWithCompetitors(false, true);
        _sportEventSummaryProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(matchDtoWithNoVirtualCompetitors);
        _competitorProfileProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(GetCompetitorProfileWithoutVirtualFlag());

        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, TestConfiguration.GetConfig().DefaultLanguage, null);
        var profile = await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:4698"), new[] { TestConfiguration.GetConfig().DefaultLanguage }, true);

        profile.IsVirtual.Should().BeFalse();
    }

    [Fact]
    public async Task GetSportEventSummaryWithEmptyVirtualFlagShouldReturnVirtualCompetitorCacheItem()
    {
        var matchDtoWithNoVirtualCompetitors = GetMatchDtoWithCompetitors(false, false);
        _sportEventSummaryProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(matchDtoWithNoVirtualCompetitors);
        _competitorProfileProviderMock.Setup(m => m.GetDataAsync(It.IsAny<string>(), It.IsAny<string>()))
                                      .ReturnsAsync(GetCompetitorProfileWithoutVirtualFlag());

        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, TestConfiguration.GetConfig().DefaultLanguage, null);
        var profile = await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:4698"), new[] { TestConfiguration.GetConfig().DefaultLanguage }, true);

        profile.IsVirtual.Should().BeFalse();
    }

    private static CompetitorProfileDto GetCompetitorProfileWithoutVirtualFlag()
    {
        return new CompetitorProfileDto(new competitorProfileEndpoint { competitor = new teamExtended { id = "sr:competitor:4698", name = "Spain", abbreviation = "ESP" } });
    }

    private static MatchDto GetMatchDtoWithCompetitors(bool isVirtual, bool virtualSpecified)
    {
        return new MatchDto(new sportEvent { id = "sr:match:1", competitors = new[] { new teamCompetitor { @virtual = isVirtual, virtualSpecified = virtualSpecified, id = "sr:competitor:4698" } } });
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
