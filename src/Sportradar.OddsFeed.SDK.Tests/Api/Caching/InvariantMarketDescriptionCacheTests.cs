// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using xRetry;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class InvariantMarketDescriptionCacheTests
{
    private const int DefaultMarketId282 = 282;
    private readonly CacheManager _cacheManager;
    private readonly IReadOnlyList<CultureInfo> _languages;

    private readonly InvariantMarketDescriptionCache _invariantMarketDescriptionCache;
    private readonly ICacheStore<string> _invariantMarketDescriptionMemoryCache;
    private readonly IMappingValidatorFactory _mappingValidatorFactory;
    private readonly XunitLoggerFactory _testLoggerFactory;
    private readonly SdkTimer _timer;
    private readonly Mock<IDataProvider<EntityList<MarketDescriptionDto>>> _invariantMdProviderMock;

    public InvariantMarketDescriptionCacheTests(ITestOutputHelper outputHelper)
    {
        _testLoggerFactory = new XunitLoggerFactory(outputHelper);
        _languages = ScheduleData.Cultures3.ToList();

        _invariantMdProviderMock = new Mock<IDataProvider<EntityList<MarketDescriptionDto>>>();

        var memoryCache = new MemoryCache(new MemoryCacheOptions
        {
            TrackStatistics = true,
            TrackLinkedCacheEntries = true,
            CompactionPercentage = 0.2,
            ExpirationScanFrequency = TimeSpan.FromMinutes(10)
        });

        _invariantMarketDescriptionMemoryCache = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache,
                                                                        memoryCache,
                                                                        _testLoggerFactory.CreateLogger(typeof(Cache)),
                                                                        null,
                                                                        TestConfiguration.GetConfig().Cache.VariantMarketDescriptionCacheTimeout,
                                                                        1);

        _timer = new SdkTimer("TimerForInvariantMarketDescriptionsCache", TimeSpan.FromSeconds(5), TimeSpan.FromHours(6));

        _mappingValidatorFactory = new MappingValidatorFactory();
        _cacheManager = new CacheManager();
        var dataRouterManager = new DataRouterManagerBuilder()
                               .AddMockedDependencies()
                               .WithCacheManager(_cacheManager)
                               .WithInvariantMarketDescriptionsProvider(_invariantMdProviderMock.Object)
                               .Build();

        _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, dataRouterManager, _mappingValidatorFactory, _timer, _languages, _cacheManager, _testLoggerFactory);

        SetupInvariantMarketDescriptionBasicData();
    }

    [Fact]
    public void InitialCallsDone()
    {
        Assert.Equal(3, _languages.Count);
        Assert.NotNull(_invariantMarketDescriptionCache);
        Assert.Empty(_invariantMarketDescriptionMemoryCache.GetKeys());
        VerifyNoAdditionalMarkerApiCallsWasMade();
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsType<IMarketDescriptionsCache>(_invariantMarketDescriptionCache, false);
        _ = Assert.IsType<ISdkCache>(_invariantMarketDescriptionCache, false);
        _ = Assert.IsType<IHealthStatusProvider>(_invariantMarketDescriptionCache, false);
    }

    [Fact]
    public void ImplementationRegisteredToConsumeSingleDto()
    {
        _ = Assert.Single(_invariantMarketDescriptionCache.RegisteredDtoTypes);
        Assert.Equal(DtoType.MarketDescriptionList, _invariantMarketDescriptionCache.RegisteredDtoTypes.First());
    }

    [Fact]
    public void ConstructorWhenNullCacheStoreThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(null, new Mock<IDataRouterManager>().Object, _mappingValidatorFactory, _timer, _languages, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullDataRouterManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, null, _mappingValidatorFactory, _timer, _languages, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullMappingValidatorFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, new Mock<IDataRouterManager>().Object, null, _timer, _languages, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullTimerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, new Mock<IDataRouterManager>().Object, _mappingValidatorFactory, null, _languages, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLanguagesThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, new Mock<IDataRouterManager>().Object, _mappingValidatorFactory, _timer, null, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenEmptyLanguagesThenThrow()
    {
        _ = Assert.Throws<ArgumentException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, new Mock<IDataRouterManager>().Object, _mappingValidatorFactory, _timer, [], _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullCacheManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, new Mock<IDataRouterManager>().Object, _mappingValidatorFactory, _timer, _languages, null, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLoggerFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, new Mock<IDataRouterManager>().Object, _mappingValidatorFactory, _timer, _languages, _cacheManager, null));
    }

    [Fact]
    public async Task UpdateCacheItemDoesUpdateSource()
    {
        var mdCacheItem = await GetCacheItem(DefaultMarketId282, [_languages[0]]);
        mdCacheItem.SetFetchInfo("some-source");
        var initialSource = mdCacheItem.SourceCache;

        _invariantMarketDescriptionCache.UpdateCacheItem(DefaultMarketId282, "any-variant");

        Assert.NotEqual(initialSource, mdCacheItem.SourceCache);
        Assert.Equal(_invariantMarketDescriptionCache.CacheName, mdCacheItem.SourceCache);
    }

    [Fact]
    public async Task UpdateCacheItemWhenDisposedThenIgnores()
    {
        var mdCacheItem = await GetCacheItem(DefaultMarketId282, [_languages[0]]);
        mdCacheItem.SetFetchInfo("some-source");
        var initialSource = mdCacheItem.SourceCache;
        _invariantMarketDescriptionCache.Dispose();

        _invariantMarketDescriptionCache.UpdateCacheItem(DefaultMarketId282, null);

        Assert.Equal(initialSource, mdCacheItem.SourceCache);
    }

    [Fact]
    public void Disposed()
    {
        _invariantMarketDescriptionCache.Dispose();

        Assert.NotNull(_invariantMarketDescriptionCache);
        Assert.True(_invariantMarketDescriptionCache.IsDisposed());
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Test designed for this")]
    public void DisposingTwiceDoesNotThrow()
    {
        _invariantMarketDescriptionCache.Dispose();
        _invariantMarketDescriptionCache.Dispose();

        Assert.NotNull(_invariantMarketDescriptionCache);
        Assert.True(_invariantMarketDescriptionCache.IsDisposed());
    }

    [Fact]
    public void DisposingWhenMultipleParallelCallsThenDoesNotThrow()
    {
        _ = Parallel.For(1, 10, _ => _invariantMarketDescriptionCache.Dispose());

        Assert.NotNull(_invariantMarketDescriptionCache);
        Assert.True(_invariantMarketDescriptionCache.IsDisposed());
    }

    [Fact]
    public async Task LoadingAllDataInvokesApiCallForEachLanguage()
    {
        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        result.ShouldBeTrue();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task LoadingAllDataCachesAllData()
    {
        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        result.ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWhenValidUrnThenNothingIsDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        _invariantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All).ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWhenUrnNullThenDoesNotThrow()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);

        _invariantMarketDescriptionCache.CacheDeleteItem((Urn)null, CacheItemType.MarketDescription);

        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWhenInvalidUrnThenNothingIsDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse("sr:market:123456");

        _invariantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All).ShouldBeFalse();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWhenValidUrnThenDeletingIsNotLogged()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(InvariantMarketDescriptionCache));
        logExec.Messages.Clear();

        _invariantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.DoesNotContain(logCache.Messages, w => w.Contains("delete invariant market", StringComparison.InvariantCultureIgnoreCase));
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithUrnWhenCallingWithUnregisteredDtoTypeThenNothingIsDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        _invariantMarketDescriptionCache.CacheDeleteItem(marketUrn, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription));

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All).ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyThenCacheItemIsNotDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All);

        _invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All).ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKeyThenCacheItemIsNotDeleted()
    {
        const string nonExistingMarketId = "123456";
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        _invariantMarketDescriptionCache.CacheHasItem(nonExistingMarketId, CacheItemType.All).ShouldBeFalse();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKey2ThenCacheItemIsNotDeleted()
    {
        const string nonExistingMarketId = "sr:invariant:282";
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        _invariantMarketDescriptionCache.CacheHasItem(nonExistingMarketId, CacheItemType.All).ShouldBeFalse();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKeyThenNothingIsLogged()
    {
        const string nonExistingMarketId = "sr:invariant:282";
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(InvariantMarketDescriptionCache));
        logExec.Messages.Clear();

        _invariantMarketDescriptionCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        Assert.DoesNotContain(logCache.Messages, w => w.Contains("delete invariant market", StringComparison.InvariantCultureIgnoreCase));
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingThenLogIsNotCreated()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(InvariantMarketDescriptionCache));
        logExec.Messages.Clear();

        _invariantMarketDescriptionCache.CacheDeleteItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All);

        Assert.DoesNotContain(logCache.Messages, w => w.Contains("delete invariant market", StringComparison.InvariantCultureIgnoreCase));
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndRegisteredDtoTypeThenCacheItemIsNotDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.MarketDescription);

        _invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All).ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndUnregisteredDtoTypeThenNothingIsDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription));

        _invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All).ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingThenTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All).ShouldBeTrue();
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithRegisteredDtoTypeThenTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.MarketDescription).ShouldBeTrue();
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithUnregisteredDtoTypeThenAlwaysFalse()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription)).ShouldBeFalse();
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithRandomUrnGroupTypeThenCanFind()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:anything:{DefaultMarketId282}");

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All).ShouldBeTrue();
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingUnknownIdThenFalse()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse("sr:market:123456");

        _invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All).ShouldBeFalse();
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyThenReturnTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All).ShouldBeTrue();
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyAndRegisteredDtoTypeThenReturnTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.MarketDescription).ShouldBeTrue();
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyAndUnregisteredDtoTypeThenReturnFalse()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription)));
    }

    [Fact]
    public async Task CacheHasItemWithStringWhenCallingUnknownIdThenFalse()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem("123456", CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithStringWhenCallingMatchIdThenFalse()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem("sr:match:282", CacheItemType.All));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenNothingIsLoadedThenApiCallsAreMade()
    {
        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _languages);

        marketDescription.ShouldNotBeNull();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenValidIdThenMarketDescriptionIsReturned()
    {
        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _languages);

        Assert.NotNull(marketDescription);
        Assert.Equal(DefaultMarketId282, marketDescription.Id);
        Assert.Equal(2, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[1]));
        Assert.NotEqual(marketDescription.GetName(_languages[0]), marketDescription.GetName(_languages[2]));
        Assert.NotEqual(marketDescription.GetName(_languages[1]), marketDescription.GetName(_languages[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownIdThenNullIsReturned()
    {
        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(12345, null, _languages);

        Assert.Null(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenNullLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, null));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenLanguagesMissingThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, []));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenApiCallFailsThenThrow()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _languages));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenDeserializationFailsThenThrow()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new DeserializationException("Not found", null));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _languages));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenMappingFailsThenThrow()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new MappingException("Not found", "some-property", "property-value", nameof(market_descriptions), null));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _languages));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnhandledExceptionIsThrownThenOriginalExceptionIsThrown()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new UriFormatException("Not found"));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _languages));

        Assert.IsType<UriFormatException>(resultException.InnerException, false);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownLanguageIsRequestedThenReturnNull()
    {
        var oneLanguage = new List<CultureInfo>
                              {
                                  new CultureInfo("na")
                              };

        var mdCi = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, oneLanguage);

        Assert.Null(mdCi);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownLanguageIsRequestedThenApiCallIsMade()
    {
        var oneLanguage = new List<CultureInfo>
                              {
                                  new CultureInfo("na")
                              };
        ResetMarketApiCalls();

        _ = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, oneLanguage);

        VerifyInvariantListApiCalls(Times.Once());
    }

    [RetryFact(3, 5000)]
    public void OnTimerWhenConstructedThenMakeApiCallForAllLanguages()
    {
        _timer.FireOnce(TimeSpan.Zero);

        var success = MakeApiCallForAllLanguagesAndWaitTillSaved();

        success.ShouldBeTrue();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [RetryFact(3, 5000)]
    public async Task OnTimerWhenConstructedThenCacheItemHasAllLanguages()
    {
        _timer.FireOnce(TimeSpan.Zero);
        var success = await TestExecutionHelper.WaitToCompleteAsync(() => _languages.Count == _invariantMdProviderMock.Invocations.Count);

        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _languages);

        success.ShouldBeTrue();
        marketDescription.ShouldNotBeNull();
        _languages.All(a => !string.IsNullOrEmpty(marketDescription.GetName(a))).ShouldBeTrue();
    }

    [Fact]
    public async Task OnTimerWhenDataRouterManagerDisposedThenHandleDisposedException()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new ObjectDisposedException("Drm disposed"));
        _timer.FireOnce(TimeSpan.Zero);

        var success = await TestExecutionHelper.WaitToCompleteAsync(() => _languages.Count == _invariantMdProviderMock.Invocations.Count);

        success.ShouldBeTrue();
    }

    [RetryFact(3, 5000)]
    public async Task OnTimerWhenDataRouterManagerDisposedThenNothingIsCached()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new ObjectDisposedException("Drm disposed"));
        _timer.FireOnce(TimeSpan.Zero);

        var success = await TestExecutionHelper.WaitToCompleteAsync(() => _languages.Count == _invariantMdProviderMock.Invocations.Count);

        success.ShouldBeTrue();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(0);
    }

    [RetryFact(3, 5000)]
    public async Task OnTimerWhenApiCallTaskCancelledThenHandleDisposedException()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new TaskCanceledException("Call not finished in time"));
        _timer.FireOnce(TimeSpan.Zero);

        var success = await TestExecutionHelper.WaitToCompleteAsync(() => _languages.Count == _invariantMdProviderMock.Invocations.Count, 200, 20000);

        success.ShouldBeTrue();
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
    }

    [Fact]
    public async Task OnTimerWhenApiCallTaskCancelledThenNothingIsCached()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new TaskCanceledException("Call not finished in time"));
        _timer.FireOnce(TimeSpan.Zero);

        var success = await TestExecutionHelper.WaitToCompleteAsync(() => _languages.Count == _invariantMdProviderMock.Invocations.Count, 200, 20000);

        success.ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(0);
    }

    [RetryFact(3, 5000)]
    public async Task OnTimerWhenCacheDisposedThenTimerIsIgnored()
    {
        _timer.Start(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5));
        _invariantMarketDescriptionCache.Dispose();

        var success = await TestExecutionHelper.WaitToCompleteAsync(() => _languages.Count == _invariantMdProviderMock.Invocations.Count, 100, 1000);

        success.ShouldBeFalse();
        VerifyInvariantListApiCalls(Times.Never());
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenDataReceivedThenAllLoaded()
    {
        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDataReceivedThenApiCallsAreMadeForAll()
    {
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
        VerifyInvariantListApiCalls(Times.Never());

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count));
    }

    [RetryFact(3, 5000)]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public async Task LoadMarketDescriptionsWhenCalledMultipleTimesThenLoadEachTime()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        result.ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count * 2));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallFailsThenMethodReturnsFalse()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallFailsThenNothingIsSaved()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDeserializationFailsThenReturnFalse()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new DeserializationException("Not found", null));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenMappingFailsThenReturnFalse()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new MappingException("Not found", "some-property", "property-value", nameof(market_descriptions), null));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenUnhandledExceptionIsThrownThenReturnFalse()
    {
        _invariantMdProviderMock.Reset();
        _invariantMdProviderMock.Setup(s => s.GetDataAsync(It.IsAny<string>()))
                                .ThrowsAsync(new UriFormatException("Not found"));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenAllIsCached()
    {
        await SetupInvariantMarketProviderToReturnMissingData();

        var success = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        success.ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(GetInvariantMarketProviderToReturnMissingDataMarketCount());
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullName()
    {
        await SetupInvariantMarketProviderToReturnMissingData();

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var useCultures = new[]
                              {
                                  _languages[0]
                              };

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(282, null, useCultures);

        md.ShouldNotBeNull();
        md.GetName(_languages[0]).ShouldBeNull();
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullNameForAllLanguages()
    {
        await SetupInvariantMarketProviderToReturnMissingData();

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(282, null, _languages);

        md.ShouldNotBeNull();
        md.GetName(_languages[0]).ShouldBeNull();
        md.GetName(_languages[1]).ShouldNotBeNullOrEmpty();
        md.GetName(_languages[2]).ShouldNotBeNullOrEmpty();
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithEmptyName()
    {
        await SetupInvariantMarketProviderToReturnMissingData();

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(701, null, _languages);

        md.ShouldNotBeNull();
        md.GetName(_languages[0]).ShouldBeNullOrEmpty();
        md.GetName(_languages[1]).ShouldNotBeNullOrEmpty();
        md.GetName(_languages[2]).ShouldNotBeNullOrEmpty();
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullOutcomeName()
    {
        await SetupInvariantMarketProviderToReturnMissingData();

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(1084, null, _languages);

        md.ShouldNotBeNull();
        Assert.NotEmpty(md.Outcomes);
        md.Outcomes.First().GetName(_languages[0]).ShouldBeNullOrEmpty();
        md.Outcomes.First().GetName(_languages[1]).ShouldNotBeNullOrEmpty();
        md.Outcomes.First().GetName(_languages[2]).ShouldNotBeNullOrEmpty();
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithEmptyOutcomeName()
    {
        await SetupInvariantMarketProviderToReturnMissingData();

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(447, null, _languages);

        md.ShouldNotBeNull();
        Assert.NotEmpty(md.Outcomes);
        md.Outcomes.First().GetName(_languages[0]).ShouldBeNullOrEmpty();
        md.Outcomes.First().GetName(_languages[1]).ShouldNotBeNullOrEmpty();
        md.Outcomes.First().GetName(_languages[2]).ShouldNotBeNullOrEmpty();
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullMappingOutcomeName()
    {
        await SetupInvariantMarketProviderToReturnMissingData();
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(340, null, _languages);

        md.ShouldNotBeNull();
        md.Mappings.ShouldNotBeEmpty();
        md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_languages[0]).ShouldBeNullOrEmpty();
        md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_languages[1]).ShouldNotBeNullOrEmpty();
        md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_languages[2]).ShouldNotBeNullOrEmpty();
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithEmptyMappingOutcomeName()
    {
        await SetupInvariantMarketProviderToReturnMissingData();

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(1084, null, _languages);

        md.ShouldNotBeNull();
        md.Mappings.ShouldNotBeEmpty();
        md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_languages[0]).ShouldBeNullOrEmpty();
        md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_languages[1]).ShouldNotBeNullOrEmpty();
        md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_languages[2]).ShouldNotBeNullOrEmpty();
    }

    [RetryFact(3, 5000)]
    public async Task LoadMarketDescriptionsWhenReloadingFullDataThenCacheItemsAreUpdated()
    {
        await SetupInvariantMarketProviderToReturnMissingData();
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        SetupInvariantMarketDescriptionBasicData();

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(701, null, _languages);

        md.ShouldNotBeNull();
        Assert.False(string.IsNullOrEmpty(md.GetName(_languages[0])));
        md.GetName(_languages[0]).ShouldNotBeNullOrEmpty();
        md.GetName(_languages[2]).ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenReloadingThenNoThrottlingHappens()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        VerifyInvariantListApiCalls(Times.Exactly(_languages.Count * 2));
    }

    [Fact]
    public async Task GetAllWhenNoDataPresentThenReturnEmptyCollection()
    {
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());

        var result = await _invariantMarketDescriptionCache.GetAllInvariantMarketDescriptionsAsync(_languages);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllWhenDataPresentThenReturnAllCollection()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var result = await _invariantMarketDescriptionCache.GetAllInvariantMarketDescriptionsAsync(_languages);

        result.Count().ShouldBe(MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length);
    }

    [Fact]
    public async Task GetAllWhenCallingWithNullLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _invariantMarketDescriptionCache.GetAllInvariantMarketDescriptionsAsync(null));
    }

    [Fact]
    public async Task GetAllWhenCallingWithEmptyLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentException>(() => _invariantMarketDescriptionCache.GetAllInvariantMarketDescriptionsAsync(ArraySegment<CultureInfo>.Empty));
    }

    [Fact]
    public async Task WhenMarketDescriptionsReceivedButWrongDtoTypeThenItIsIgnored()
    {
        var apiMds = await GetInvariantXml(_languages[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));
        var entityListMarketDescriptions = new EntityList<MarketDescriptionDto>(dtoMds);

        var result = await _invariantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse("sr:markets:123"), entityListMarketDescriptions, _languages[0], EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(DtoType.MarketDescriptionList), null);

        Assert.False(result);
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenDtoItemDoesNotMuchExpectedThenItIsIgnored()
    {
        var apiMds = await GetInvariantXml(_languages[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), dtoMds, _languages[0], DtoType.MarketDescriptionList, null);

        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenCacheDisposedThenAddingIsIgnored()
    {
        var apiMds = await GetInvariantXml(_languages[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));
        var entityListMarketDescriptions = new EntityList<MarketDescriptionDto>(dtoMds);
        _invariantMarketDescriptionCache.Dispose();

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), entityListMarketDescriptions, _languages[0], DtoType.MarketDescriptionList, null);

        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenDtoItemDoesNotMuchExpectedThenMismatchIsLogged()
    {
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(InvariantMarketDescriptionCache));
        var apiMds = await GetInvariantXml(_languages[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), dtoMds, _languages[0], DtoType.MarketDescriptionList, null);

        Assert.Empty(logCache.Messages);
        Assert.Single(logExec.Messages);
        Assert.Single(logExec.Messages, w => w.Contains("Invalid data"));
    }

    [Fact]
    public async Task HealthCheckExists()
    {
        var healthCheck = await _invariantMarketDescriptionCache.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, healthCheck.Status);
    }

    private static async Task<market_descriptions> GetInvariantXml(CultureInfo language)
    {
        var resourceName = $"invariant_market_descriptions_{language.TwoLetterISOLanguageName}.xml";
        var restDeserializer = new Deserializer<market_descriptions>();
        await using var stream = FileHelper.GetResource(resourceName);

        return stream != null
                   ? restDeserializer.Deserialize(stream)
                   : throw new InvalidOperationException("No data");
    }

    private async Task<MarketDescriptionCacheItem> GetCacheItem(int marketId, IReadOnlyCollection<CultureInfo> languages)
    {
        return await _invariantMarketDescriptionCache.GetMarketInternalAsync(marketId, languages).ConfigureAwait(false);
    }

    private bool MakeApiCallForAllLanguagesAndWaitTillSaved()
    {
        return TestExecutionHelper.WaitToComplete(ValidationCheck, 200, 30000);
    }

    private bool ValidationCheck()
    {
        return MarketDescriptionEndpoint.GetDefaultInvariantList().market.Length == _invariantMarketDescriptionMemoryCache.Count()
               && _languages.Count == _invariantMdProviderMock.Invocations.Count;
    }

    private void VerifyInvariantListApiCalls(Times times)
    {
        _invariantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>()), times);
    }

    private void ResetMarketApiCalls()
    {
        _invariantMdProviderMock.Invocations.Clear();
    }

    private void VerifyNoAdditionalMarkerApiCallsWasMade()
    {
        _invariantMdProviderMock.Verify(s => s.GetDataAsync(It.IsAny<string>()), Times.Never);
    }

    private void SetupInvariantMarketDescriptionBasicData()
    {
        _invariantMdProviderMock.Reset();
        foreach (var culture in _languages)
        {
            var invariantList = MarketDescriptionEndpoint.GetDefaultInvariantList();
            invariantList.market = invariantList.market.Select(m => m.AddSuffix(culture.TwoLetterISOLanguageName)).ToArray();
            _invariantMdProviderMock.Setup(s => s.GetDataAsync(culture.TwoLetterISOLanguageName))
                                    .ReturnsAsync(MarketDescriptionEndpoint.GetInvariantDto(invariantList.market));
        }
    }

    private async Task SetupInvariantMarketProviderToReturnMissingData()
    {
        _invariantMdProviderMock.Reset();
        foreach (var culture in _languages)
        {
            var marketDtos = culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase)
                                 ? await GetInvariantMarketDescriptionsFromFile("invariant_market_descriptions_en_missing.xml")
                                 : await GetInvariantMarketDescriptionsFromFile($"invariant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml");
            _invariantMdProviderMock.Setup(s => s.GetDataAsync(culture.TwoLetterISOLanguageName))
                                    .ReturnsAsync(marketDtos);
        }
    }

    private int GetInvariantMarketProviderToReturnMissingDataMarketCount()
    {
        var result = GetInvariantMarketDescriptionsFromFile("invariant_market_descriptions_en_missing.xml").GetAwaiter().GetResult();
        return result.Items.Count();
    }

    private static async Task<EntityList<MarketDescriptionDto>> GetInvariantMarketDescriptionsFromFile(string fileName)
    {
        var invalidBody = FileHelper.GetFileContent(fileName);

        var restDeserializer = new Deserializer<market_descriptions>();
        var mapper = new MarketDescriptionsMapperFactory();

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidBody));
        var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
        return result;
    }
}
