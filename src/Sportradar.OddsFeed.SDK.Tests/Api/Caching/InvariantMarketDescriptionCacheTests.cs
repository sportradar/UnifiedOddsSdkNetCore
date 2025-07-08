// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class InvariantMarketDescriptionCacheTests
{
    private const int DefaultMarketId282 = 282;
    private readonly CacheManager _cacheManager;
    private readonly IReadOnlyList<CultureInfo> _cultures;

    private readonly TestDataRouterManager _dataRouterManager;
    private readonly InvariantMarketDescriptionCache _invariantMarketDescriptionCache;
    private readonly ICacheStore<string> _invariantMarketDescriptionMemoryCache;
    private readonly IMappingValidatorFactory _mappingValidatorFactory;
    private readonly XunitLoggerFactory _testLoggerFactory;
    private readonly SdkTimer _timer;

    public InvariantMarketDescriptionCacheTests(ITestOutputHelper outputHelper)
    {
        _testLoggerFactory = new XunitLoggerFactory(outputHelper);
        _cultures = ScheduleData.Cultures3.ToList();

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
        _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);

        _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, _cacheManager, _testLoggerFactory);
    }

    [Fact]
    public void InitialCallsDone()
    {
        Assert.Equal(3, _cultures.Count);
        Assert.NotNull(_invariantMarketDescriptionCache);
        Assert.Empty(_invariantMarketDescriptionMemoryCache.GetKeys());
        Assert.NotNull(_dataRouterManager);
        Assert.NotNull(_cacheManager);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsAssignableFrom<IMarketDescriptionsCache>(_invariantMarketDescriptionCache);
        _ = Assert.IsAssignableFrom<ISdkCache>(_invariantMarketDescriptionCache);
        _ = Assert.IsAssignableFrom<IHealthStatusProvider>(_invariantMarketDescriptionCache);
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
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(null, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullDataRouterManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, null, _mappingValidatorFactory, _timer, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullMappingValidatorFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, null, _timer, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullTimerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, null, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLanguagesThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, null, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenEmptyLanguagesThenThrow()
    {
        _ = Assert.Throws<ArgumentException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, [], _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullCacheManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, null, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLoggerFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, _cacheManager, null));
    }

    [Fact]
    public async Task UpdateCacheItemDoesUpdateSource()
    {
        var mdCacheItem = await GetCacheItem(DefaultMarketId282, [_cultures[0]]);
        mdCacheItem.SetFetchInfo("some-source");
        var initialSource = mdCacheItem.SourceCache;

        _invariantMarketDescriptionCache.UpdateCacheItem(DefaultMarketId282, "any-variant");

        Assert.NotEqual(initialSource, mdCacheItem.SourceCache);
        Assert.Equal(_invariantMarketDescriptionCache.CacheName, mdCacheItem.SourceCache);
    }

    [Fact]
    public async Task UpdateCacheItemWhenDisposedThenIgnores()
    {
        var mdCacheItem = await GetCacheItem(DefaultMarketId282, [_cultures[0]]);
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

        Assert.True(result);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
    }

    [Fact]
    public async Task LoadingAllDataCachesAllData()
    {
        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.True(result);
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWhenValidUrnThenNothingIsDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        _invariantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWhenUrnNullThenDoesNotThrow()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());

        _invariantMarketDescriptionCache.CacheDeleteItem((Urn)null, CacheItemType.MarketDescription);

        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWhenInvalidUrnThenNothingIsDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse("sr:market:123456");

        _invariantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
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

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyThenCacheItemIsNotDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All);

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKeyThenCacheItemIsNotDeleted()
    {
        const string nonExistingMarketId = "123456";
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem(nonExistingMarketId, CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKey2ThenCacheItemIsNotDeleted()
    {
        const string nonExistingMarketId = "sr:invariant:282";
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem(nonExistingMarketId, CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
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

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndUnregisteredDtoTypeThenNothingIsDeleted()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        _invariantMarketDescriptionCache.CacheDeleteItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription));

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingThenTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithRegisteredDtoTypeThenTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.MarketDescription));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithUnregisteredDtoTypeThenAlwaysFalse()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:{DefaultMarketId282}");

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription)));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithRandomUrnGroupTypeThenCanFind()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:anything:{DefaultMarketId282}");

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingUnknownIdThenFalse()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse("sr:market:123456");

        Assert.False(_invariantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyThenReturnTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyAndRegisteredDtoTypeThenReturnTrue()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.True(_invariantMarketDescriptionCache.CacheHasItem(DefaultMarketId282.ToString(CultureInfo.InvariantCulture), CacheItemType.MarketDescription));
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
        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _cultures);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenValidIdThenMarketDescriptionIsReturned()
    {
        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _cultures);

        Assert.NotNull(marketDescription);
        Assert.Equal(DefaultMarketId282, marketDescription.Id);
        Assert.Equal(2, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownIdThenNullIsReturned()
    {
        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(12345, null, _cultures);

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
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _cultures));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenDeserializationFailsThenThrow()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new DeserializationException("Not found", null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _cultures));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenMappingFailsThenThrow()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new MappingException("Not found", "some-property", "property-value", nameof(market_descriptions), null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _cultures));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnhandledExceptionIsThrownThenOriginalExceptionIsThrown()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new UriFormatException("Not found")));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _cultures));

        Assert.IsAssignableFrom<UriFormatException>(resultException.InnerException);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownLanguageIsRequestedThenReturnNull()
    {
        var languages = new List<CultureInfo>
                        {
                            new CultureInfo("na")
                        };

        var mdCi = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, languages);

        Assert.Null(mdCi);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownLanguageIsRequestedThenApiCallIsMade()
    {
        var languages = new List<CultureInfo>
                        {
                            new CultureInfo("na")
                        };
        _dataRouterManager.ResetMethodCall();

        _ = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, languages);

        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
    }

    [Fact]
    public void OnTimerWhenConstructedThenMakeApiCallForAllLanguages()
    {
        _timer.FireOnce(TimeSpan.Zero);

        var success = MakeApiCallForAllLanguagesAndWaitTillSaved();

        success.ShouldBeTrue();
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions).ShouldBe(_cultures.Count);
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(ScheduleData.InvariantListCacheCount);
    }

    [Fact]
    public async Task OnTimerWhenConstructedThenCacheItemHasAllLanguages()
    {
        _timer.FireOnce(TimeSpan.Zero);
        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));

        var marketDescription = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(DefaultMarketId282, null, _cultures);

        success.ShouldBeTrue();
        marketDescription.ShouldNotBeNull();
        _cultures.All(a => !string.IsNullOrEmpty(marketDescription.GetName(a))).ShouldBeTrue();
    }

    [Fact]
    public void OnTimerWhenDataRouterManagerDisposedThenHandleDisposedException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new ObjectDisposedException("Drm disposed")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));

        success.ShouldBeTrue();
    }

    [Fact]
    public void OnTimerWhenDataRouterManagerDisposedThenNothingIsCached()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new ObjectDisposedException("Drm disposed")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));

        success.ShouldBeTrue();
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions).ShouldBe(_cultures.Count);
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(0);
    }

    [Fact]
    public void OnTimerWhenApiCallTaskCancelledThenHandleDisposedException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new TaskCanceledException("Call not finished in time")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions), 200, 20000);

        success.ShouldBeTrue();
        _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions).ShouldBe(_cultures.Count);
    }

    [Fact]
    public void OnTimerWhenApiCallTaskCancelledThenNothingIsCached()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new TaskCanceledException("Call not finished in time")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions), 200, 20000);

        success.ShouldBeTrue();
        _invariantMarketDescriptionMemoryCache.Count().ShouldBe(0);
    }

    [Fact]
    public void OnTimerWhenCacheDisposedThenTimerIsIgnored()
    {
        _timer.Start(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5));
        _invariantMarketDescriptionCache.Dispose();

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions), 100, 1000);

        Assert.False(success);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDataReceivedThenAllLoaded()
    {
        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDataReceivedThenApiCallsAreMadeForAll()
    {
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));

        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
    }

    [Fact]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public async Task LoadMarketDescriptionsWhenCalledMultipleTimesThenLoadEachTime()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.True(result);
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
        Assert.Equal(_cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallFailsThenMethodReturnsFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null)));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallFailsThenNothingIsSaved()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null)));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDeserializationFailsThenReturnFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new DeserializationException("Not found", null)));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenMappingFailsThenReturnFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new MappingException("Not found", "some-property", "property-value", nameof(market_descriptions), null)));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenUnhandledExceptionIsThrownThenReturnFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("invariant_market_descriptions.xml", new UriFormatException("Not found")));

        var result = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenAllIsCached()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));

        var success = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.True(success);
        Assert.Equal(ScheduleData.InvariantListCacheCount, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        var useCultures = new[]
                          {
                              _cultures[0]
                          };

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(282, null, useCultures);

        Assert.NotNull(md);
        Assert.Null(md.GetName(_cultures[0]));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullNameForAllLanguages()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(282, null, _cultures);

        Assert.NotNull(md);
        Assert.Null(md.GetName(_cultures[0]));
        Assert.False(string.IsNullOrEmpty(md.GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(md.GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithEmptyName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(701, null, _cultures);

        Assert.NotNull(md);
        Assert.True(string.IsNullOrEmpty(md.GetName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(md.GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(md.GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullOutcomeName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(1084, null, _cultures);

        Assert.NotNull(md);
        Assert.NotEmpty(md.Outcomes);
        Assert.True(string.IsNullOrEmpty(md.Outcomes.First().GetName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(md.Outcomes.First().GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(md.Outcomes.First().GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithEmptyOutcomeName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(447, null, _cultures);

        Assert.NotNull(md);
        Assert.NotEmpty(md.Outcomes);
        Assert.True(string.IsNullOrEmpty(md.Outcomes.First().GetName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(md.Outcomes.First().GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(md.Outcomes.First().GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullMappingOutcomeName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(340, null, _cultures);

        Assert.NotNull(md);
        Assert.NotEmpty(md.Mappings);
        Assert.True(string.IsNullOrEmpty(md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithEmptyMappingOutcomeName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(1084, null, _cultures);

        Assert.NotNull(md);
        Assert.NotEmpty(md.Mappings);
        Assert.True(string.IsNullOrEmpty(md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(md.Mappings.First().OutcomeMappings.First().GetProducerOutcomeName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenReloadingFullDataThenCacheItemsAreUpdated()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("invariant_market_descriptions_en.xml", "invariant_market_descriptions_en_missing.xml"));
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        _dataRouterManager.UriReplacements.Clear();
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var md = await _invariantMarketDescriptionCache.GetMarketDescriptionAsync(701, null, _cultures);

        Assert.NotNull(md);
        Assert.False(string.IsNullOrEmpty(md.GetName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(md.GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(md.GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenReloadingThenNoThrottlingHappens()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions));
    }

    [Fact]
    public async Task GetAllWhenNoDataPresentThenReturnEmptyCollection()
    {
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());

        var result = await _invariantMarketDescriptionCache.GetAllInvariantMarketDescriptionsAsync(_cultures);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllWhenDataPresentThenReturnAllCollection()
    {
        _ = await _invariantMarketDescriptionCache.LoadMarketDescriptionsAsync();

        var result = await _invariantMarketDescriptionCache.GetAllInvariantMarketDescriptionsAsync(_cultures);

        Assert.Equal(ScheduleData.InvariantListCacheCount, result.Count());
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
        var apiMds = await GetInvariantXml(_cultures[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));
        var entityListMarketDescriptions = new EntityList<MarketDescriptionDto>(dtoMds);

        var result = await _invariantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse("sr:markets:123"), entityListMarketDescriptions, _cultures[0], EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(DtoType.MarketDescriptionList), null);

        Assert.False(result);
        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenDtoItemDoesNotMuchExpectedThenItIsIgnored()
    {
        var apiMds = await GetInvariantXml(_cultures[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), dtoMds, _cultures[0], DtoType.MarketDescriptionList, null);

        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenCacheDisposedThenAddingIsIgnored()
    {
        var apiMds = await GetInvariantXml(_cultures[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));
        var entityListMarketDescriptions = new EntityList<MarketDescriptionDto>(dtoMds);
        _invariantMarketDescriptionCache.Dispose();

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), entityListMarketDescriptions, _cultures[0], DtoType.MarketDescriptionList, null);

        Assert.Equal(0, _invariantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenDtoItemDoesNotMuchExpectedThenMismatchIsLogged()
    {
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(InvariantMarketDescriptionCache));
        var apiMds = await GetInvariantXml(_cultures[0]);
        var dtoMds = apiMds.market.Select(s => new MarketDescriptionDto(s));

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), dtoMds, _cultures[0], DtoType.MarketDescriptionList, null);

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
        return ScheduleData.InvariantListCacheCount == _invariantMarketDescriptionMemoryCache.Count()
               && _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointMarketDescriptions);
    }
}
