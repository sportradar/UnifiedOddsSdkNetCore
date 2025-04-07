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
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class VariantMarketDescriptionListCacheTests
{
    private readonly VariantDescriptionListCache _variantsListCache;

    private readonly TestDataRouterManager _dataRouterManager;
    private readonly CacheStore<string> _variantsListMemoryCache;
    private readonly CacheManager _cacheManager;
    private readonly List<CultureInfo> _cultures;
    private readonly XunitLoggerFactory _testLoggerFactory;
    private const string DefaultVariantId = "sr:correct_score:bestof:12";
    private readonly SdkTimer _timer;
    private readonly IMappingValidatorFactory _mappingValidatorFactory;

    public VariantMarketDescriptionListCacheTests(ITestOutputHelper outputHelper)
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

        _variantsListMemoryCache = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache,
                                                          memoryCache,
                                                          _testLoggerFactory.CreateLogger(typeof(Cache)),
                                                          null,
                                                          TestConfiguration.GetConfig().Cache.VariantMarketDescriptionCacheTimeout,
                                                          1);

        _timer = new SdkTimer("TimerForInvariantMarketDescriptionsCache", TimeSpan.FromSeconds(5), TimeSpan.FromHours(6));

        _mappingValidatorFactory = new MappingValidatorFactory();
        _cacheManager = new CacheManager();
        _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);

        _variantsListCache = new VariantDescriptionListCache(_variantsListMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, _cacheManager, _testLoggerFactory);
    }

    [Fact]
    public void InitialCallsDone()
    {
        Assert.Equal(3, _cultures.Count);
        Assert.NotNull(_variantsListCache);
        Assert.Empty(_variantsListMemoryCache.GetKeys());
        Assert.NotNull(_dataRouterManager);
        Assert.NotNull(_cacheManager);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsAssignableFrom<IVariantDescriptionsCache>(_variantsListCache);
        _ = Assert.IsAssignableFrom<ISdkCache>(_variantsListCache);
        _ = Assert.IsAssignableFrom<IHealthStatusProvider>(_variantsListCache);
    }

    [Fact]
    public void ImplementationRegisteredToConsumeSingleDto()
    {
        _ = Assert.Single(_variantsListCache.RegisteredDtoTypes);
        Assert.Equal(DtoType.VariantDescriptionList, _variantsListCache.RegisteredDtoTypes.First());
    }

    [Fact]
    public void ConstructorWhenNullCacheStoreThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantDescriptionListCache(null, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullDataRouterManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantDescriptionListCache(_variantsListMemoryCache, null, _mappingValidatorFactory, _timer, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullMappingValidatorFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantDescriptionListCache(_variantsListMemoryCache, _dataRouterManager, null, _timer, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullTimerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantDescriptionListCache(_variantsListMemoryCache, _dataRouterManager, _mappingValidatorFactory, null, _cultures, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLanguagesThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantDescriptionListCache(_variantsListMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, null, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenEmptyLanguagesThenThrow()
    {
        _ = Assert.Throws<ArgumentException>(() => new VariantDescriptionListCache(_variantsListMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, Array.Empty<CultureInfo>(), _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullCacheManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantDescriptionListCache(_variantsListMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, null, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLoggerFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantDescriptionListCache(_variantsListMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, _cultures, _cacheManager, null));
    }

    // [Fact]
    // public async Task UpdateCacheItemDoesUpdateSource()
    // {
    //     var mdCacheItem = await GetCacheItem(DefaultVariantId, new[] { _cultures[0] });
    //     mdCacheItem.SetFetchInfo("some-source");
    //     var initialSource = mdCacheItem.SourceCache;
    //
    //     _variantsListCache.UpdateCacheItem(DefaultVariantId, "any-variant");
    //
    //     Assert.NotEqual(initialSource, mdCacheItem.SourceCache);
    //     Assert.Equal(_variantsListCache.CacheName, mdCacheItem.SourceCache);
    // }
    //
    // [Fact]
    // public async Task UpdateCacheItemWhenDisposedThenIgnores()
    // {
    //     var mdCacheItem = await GetCacheItem(DefaultVariantId, new[] { _cultures[0] });
    //     mdCacheItem.SetFetchInfo("some-source");
    //     var initialSource = mdCacheItem.SourceCache;
    //     _variantsListCache.Dispose();
    //
    //     _variantsListCache.UpdateCacheItem(DefaultVariantId, null);
    //
    //     Assert.Equal(initialSource, mdCacheItem.SourceCache);
    // }

    [Fact]
    public void Disposed()
    {
        _variantsListCache.Dispose();

        Assert.NotNull(_variantsListCache);
        Assert.True(_variantsListCache.IsDisposed());
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Test designed for this")]
    public void DisposingTwiceDoesNotThrow()
    {
        _variantsListCache.Dispose();
        _variantsListCache.Dispose();

        Assert.NotNull(_variantsListCache);
        Assert.True(_variantsListCache.IsDisposed());
    }

    [Fact]
    public void DisposingWhenMultipleParallelCallsThenDoesNotThrow()
    {
        _ = Parallel.For(1, 10, (_) => _variantsListCache.Dispose());

        Assert.NotNull(_variantsListCache);
        Assert.True(_variantsListCache.IsDisposed());
    }

    [Fact]
    public async Task LoadingAllDataInvokesApiCallForEachLanguage()
    {
        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.True(result);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
    }

    [Fact]
    public async Task LoadingAllDataCachesAllData()
    {
        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.True(result);
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWhenValidUrnThenNothingIsDeleted()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:123");

        _variantsListCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWhenUrnNullThenDoesNotThrow()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());

        _variantsListCache.CacheDeleteItem((Urn)null, CacheItemType.VariantDescription);

        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWhenInvalidUrnThenNothingIsDeleted()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse("sr:market:123456");

        _variantsListCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.False(_variantsListCache.CacheHasItem(marketUrn, CacheItemType.All));
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWhenValidUrnThenDeletingIsNotLogged()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:1302");
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantDescriptionListCache));
        logExec.Messages.Clear();

        _variantsListCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.DoesNotContain(logCache.Messages, w => w.Contains("delete variant market", StringComparison.InvariantCultureIgnoreCase));
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithUrnWhenCallingWithUnregisteredDtoTypeThenNothingIsDeleted()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:1302");

        _variantsListCache.CacheDeleteItem(marketUrn, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.VariantDescription));

        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyThenCacheItemIsNotDeleted()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        _variantsListCache.CacheDeleteItem(DefaultVariantId.ToString(CultureInfo.InvariantCulture), CacheItemType.All);

        Assert.True(_variantsListCache.CacheHasItem(DefaultVariantId.ToString(CultureInfo.InvariantCulture), CacheItemType.All));
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKeyThenCacheItemIsNotDeleted()
    {
        const string nonExistingMarketId = "123456";
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        _variantsListCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        Assert.False(_variantsListCache.CacheHasItem(nonExistingMarketId, CacheItemType.All));
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKey2ThenCacheItemIsNotDeleted()
    {
        const string nonExistingMarketId = "sr:invariant:282";
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        _variantsListCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        Assert.False(_variantsListCache.CacheHasItem(nonExistingMarketId, CacheItemType.All));
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithInvalidKeyThenNothingIsLogged()
    {
        const string nonExistingMarketId = "sr:invariant:282";
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantDescriptionListCache));
        logExec.Messages.Clear();

        _variantsListCache.CacheDeleteItem(nonExistingMarketId, CacheItemType.All);

        Assert.DoesNotContain(logCache.Messages, w => w.Contains("delete variant market", StringComparison.InvariantCultureIgnoreCase));
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingThenLogIsNotCreated()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantDescriptionListCache));
        logExec.Messages.Clear();

        _variantsListCache.CacheDeleteItem(DefaultVariantId.ToString(CultureInfo.InvariantCulture), CacheItemType.All);

        Assert.DoesNotContain(logCache.Messages, w => w.Contains("delete variant market", StringComparison.InvariantCultureIgnoreCase));
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndRegisteredDtoTypeThenCacheItemIsNotDeleted()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        _variantsListCache.CacheDeleteItem(DefaultVariantId.ToString(CultureInfo.InvariantCulture), CacheItemType.VariantDescription);

        Assert.True(_variantsListCache.CacheHasItem(DefaultVariantId.ToString(CultureInfo.InvariantCulture), CacheItemType.All));
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndUnregisteredDtoTypeThenNothingIsDeleted()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        _variantsListCache.CacheDeleteItem(DefaultVariantId.ToString(CultureInfo.InvariantCulture),
                                           EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.VariantDescription));

        Assert.True(_variantsListCache.CacheHasItem(DefaultVariantId.ToString(CultureInfo.InvariantCulture), CacheItemType.All));
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingThenFalse()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:123");

        Assert.False(_variantsListCache.CacheHasItem(marketUrn, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithRegisteredCacheTypeThenFalse()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:123");

        Assert.False(_variantsListCache.CacheHasItem(marketUrn, CacheItemType.VariantDescription));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithUnregisteredCacheTypeThenAlwaysFalse()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse($"sr:market:123");

        Assert.False(_variantsListCache.CacheHasItem(marketUrn, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.VariantDescription)));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingUnknownIdThenFalse()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var marketUrn = Urn.Parse("sr:market:123456");

        Assert.False(_variantsListCache.CacheHasItem(marketUrn, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyThenReturnTrue()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.True(_variantsListCache.CacheHasItem(DefaultVariantId, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyAndValidCacheTypeThenReturnTrue()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.True(_variantsListCache.CacheHasItem(DefaultVariantId, CacheItemType.VariantDescription));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyAndUnregisteredCacheTypeThenReturnFalse()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(_variantsListCache.CacheHasItem(DefaultVariantId, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.VariantDescription)));
    }

    [Fact]
    public async Task CacheHasItemWithStringWhenCallingUnknownIdThenFalse()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(_variantsListCache.CacheHasItem("123456", CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithStringWhenCallingInvalidIdThenFalse()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(_variantsListCache.CacheHasItem("sr:match:282", CacheItemType.All));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenNothingIsLoadedThenApiCallsAreMade()
    {
        var marketDescription = await _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());

        Assert.NotNull(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenValidIdThenMarketDescriptionIsReturned()
    {
        var marketDescription = await _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures);

        Assert.NotNull(marketDescription);
        Assert.Equal(DefaultVariantId, marketDescription.Id);
        Assert.Equal(13, marketDescription.Outcomes.Count());
        _ = Assert.Single(marketDescription.Mappings);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownIdThenNullIsReturned()
    {
        var marketDescription = await _variantsListCache.GetVariantDescriptorAsync("unknown-id", _cultures);

        Assert.Null(marketDescription);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenNullLanguagesThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, null));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenLanguagesMissingThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentException>(() => _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, Array.Empty<CultureInfo>()));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenApiCallFailsThenThrow()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml",
                                                                          new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenDeserializationFailsThenThrow()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new DeserializationException("Not found", null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenMappingFailsThenThrow()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml",
                                                                          new MappingException("Not found", "some-property", "property-value", nameof(market_descriptions), null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures));
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnhandledExceptionIsThrownThenOriginalExceptionIsThrown()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new UriFormatException("Not found")));

        var resultException = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures));

        Assert.IsAssignableFrom<UriFormatException>(resultException.InnerException);
    }

    [Fact]
    public async Task GetMarketDescriptionWhenUnknownLanguageIsRequestedThenReturnNull()
    {
        var languages = new List<CultureInfo>
                        {
                            new CultureInfo("na")
                        };

        var mdCi = await _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, languages);

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

        _ = await _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, languages);

        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
    }

    [Fact]
    public void OnTimerWhenConstructedThenCallForAllLanguages()
    {
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));

        Assert.True(success);
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task OnTimerWhenConstructedThenCacheItemHasAllLanguages()
    {
        _timer.FireOnce(TimeSpan.Zero);
        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));

        var marketDescription = await _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures);

        Assert.True(success);
        Assert.NotNull(marketDescription);
        Assert.True(_cultures.All(culture => !string.IsNullOrEmpty(marketDescription.Outcomes.First().GetName(culture))));
    }

    [Fact]
    public void OnTimerWhenDataRouterManagerDisposedThenHandleDisposedException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new ObjectDisposedException("Drm disposed")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions), timeoutMs: 20000);

        Assert.True(success);
    }

    [Fact]
    public void OnTimerWhenDataRouterManagerDisposedThenNothingIsCached()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new ObjectDisposedException("Drm disposed")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));

        Assert.True(success);
        Assert.Equal(0, _variantsListMemoryCache.Count());
    }

    [Fact]
    public void OnTimerWhenApiCallTaskCancelledThenHandleDisposedException()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new TaskCanceledException("Call not finished in time")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));

        Assert.True(success);
    }

    [Fact]
    public void OnTimerWhenApiCallTaskCancelledThenNothingIsCached()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new TaskCanceledException("Call not finished in time")));
        _timer.FireOnce(TimeSpan.Zero);

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));

        Assert.True(success);
        Assert.Equal(0, _variantsListMemoryCache.Count());
    }

    [Fact]
    public void OnTimerWhenCacheDisposedThenTimerIsIgnored()
    {
        _timer.Start(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(5));
        _variantsListCache.Dispose();

        var success = TestExecutionHelper.WaitToComplete(() => _cultures.Count == _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions), 100, 1000);

        Assert.False(success);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDataReceivedThenAllLoaded()
    {
        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDataReceivedThenApiCallsAreMadeForAll()
    {
        Assert.Equal(0, _variantsListMemoryCache.Count());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));

        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
    }

    [Fact]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public async Task LoadMarketDescriptionsWhenCalledMultipleTimesThenLoadEachTime()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.True(result);
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
        Assert.Equal(_cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallFailsThenMethodReturnsFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml",
                                                                          new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null)));

        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallFailsThenNothingIsSaved()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml",
                                                                          new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null)));

        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
        Assert.Equal(0, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenDeserializationFailsThenReturnFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new DeserializationException("Not found", null)));

        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenMappingFailsThenReturnFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml",
                                                                          new MappingException("Not found", "some-property", "property-value", nameof(market_descriptions), null)));

        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenUnhandledExceptionIsThrownThenReturnFalse()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("variant_market_descriptions.xml", new UriFormatException("Not found")));

        var result = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenAllIsCached()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("variant_market_descriptions_en.xml", "variant_market_descriptions_en_missing.xml"));

        var success = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.True(success);
        Assert.Equal(ScheduleData.VariantListCacheCount, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketOutcomeWithNullName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("variant_market_descriptions_en.xml", "variant_market_descriptions_en_missing.xml"));
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        var md = await _variantsListCache.GetVariantDescriptorAsync("sr:correct_score:max:6",
                                                                    new[]
                                                                    {
                                                                        _cultures[0]
                                                                    });

        Assert.NotNull(md);
        var outcome = md.Outcomes.First(f => f.Id.Equals("sr:correct_score:max:6:1304", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(outcome);
        Assert.Null(outcome.GetName(_cultures[0]));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketOutcomeWithNullNameForAllLanguages()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("variant_market_descriptions_en.xml", "variant_market_descriptions_en_missing.xml"));
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        var md = await _variantsListCache.GetVariantDescriptorAsync("sr:correct_score:max:6", _cultures);

        Assert.NotNull(md);
        var outcome = md.Outcomes.First(f => f.Id.Equals("sr:correct_score:max:6:1304", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(outcome);
        Assert.Null(outcome.GetName(_cultures[0]));
        Assert.False(string.IsNullOrEmpty(outcome.GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(outcome.GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketOutcomeWithEmptyName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("variant_market_descriptions_en.xml", "variant_market_descriptions_en_missing.xml"));
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        var md = await _variantsListCache.GetVariantDescriptorAsync("sr:decided_by_extra_points:bestof:5", _cultures);

        Assert.NotNull(md);
        var outcome = md.Outcomes.First(f => f.Id.Equals("sr:decided_by_extra_points:bestof:5:54", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(outcome);
        Assert.True(string.IsNullOrEmpty(outcome.GetName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(outcome.GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(outcome.GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithNullMappingProductOutcomeName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("variant_market_descriptions_en.xml", "variant_market_descriptions_en_missing.xml"));
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        var md = await _variantsListCache.GetVariantDescriptorAsync("sr:decided_by_extra_points:bestof:5", _cultures);

        Assert.NotNull(md);
        Assert.NotEmpty(md.Mappings);
        var mappingData = md.Mappings.First(f => f.MarketId.Equals("239", StringComparison.OrdinalIgnoreCase)
                                                 && f.SportId.Equals(Urn.Parse("sr:sport:20"))
                                                 && f.ProducerIds.Contains(1));
        Assert.NotNull(mappingData);
        var outcomeMapping = mappingData.OutcomeMappings.First(f => f.ProducerOutcomeId == "220");
        Assert.True(string.IsNullOrEmpty(outcomeMapping.GetProducerOutcomeName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(outcomeMapping.GetProducerOutcomeName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(outcomeMapping.GetProducerOutcomeName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenApiCallReturnMissingThenReturnMarketWithEmptyMappingProductOutcomeName()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("variant_market_descriptions_en.xml", "variant_market_descriptions_en_missing.xml"));
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        var md = await _variantsListCache.GetVariantDescriptorAsync(DefaultVariantId, _cultures);

        Assert.NotNull(md);
        Assert.NotEmpty(md.Mappings);
        var mappingData = md.Mappings.First(f => f.MarketId.Equals("374", StringComparison.OrdinalIgnoreCase)
                                                 && f.SportId.Equals(Urn.Parse("sr:sport:22"))
                                                 && f.ProducerIds.Contains(3));
        Assert.NotNull(mappingData);
        var outcomeMapping = mappingData.OutcomeMappings.First(f => f.ProducerOutcomeId == "34");
        Assert.True(string.IsNullOrEmpty(outcomeMapping.GetProducerOutcomeName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(outcomeMapping.GetProducerOutcomeName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(outcomeMapping.GetProducerOutcomeName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenReloadingFullDataThenCacheItemsAreUpdated()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>("variant_market_descriptions_en.xml", "variant_market_descriptions_en_missing.xml"));
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        _dataRouterManager.UriReplacements.Clear();
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        var md = await _variantsListCache.GetVariantDescriptorAsync("sr:correct_score:max:6", _cultures);

        Assert.NotNull(md);
        var outcome = md.Outcomes.First(f => f.Id.Equals("sr:correct_score:max:6:1304", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(outcome);
        Assert.False(string.IsNullOrEmpty(outcome.GetName(_cultures[0])));
        Assert.False(string.IsNullOrEmpty(outcome.GetName(_cultures[1])));
        Assert.False(string.IsNullOrEmpty(outcome.GetName(_cultures[2])));
    }

    [Fact]
    public async Task LoadMarketDescriptionsWhenReloadingThenNoThrottlingHappens()
    {
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();
        _ = await _variantsListCache.LoadMarketDescriptionsAsync();

        Assert.Equal(_cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantDescriptions));
    }

    // [Fact]
    // public async Task GetAllWhenNoDataPresentThenReturnEmptyCollection()
    // {
    //     Assert.Equal(0, _variantsListMemoryCache.Count());
    //
    //     var result = await _variantsListCache.GetAllInvariantMarketDescriptionsAsync(_cultures);
    //
    //     Assert.Empty(result);
    // }
    //
    // [Fact]
    // public async Task GetAllWhenDataPresentThenReturnAllCollection()
    // {
    //     _ = await _variantsListCache.LoadMarketDescriptionsAsync();
    //
    //     var result = await _variantsListCache.GetAllInvariantMarketDescriptionsAsync(_cultures);
    //
    //     Assert.Equal(ScheduleData.VariantListCacheCount, result.Count());
    // }
    //
    // [Fact]
    // public async Task GetAllWhenCallingWithNullLanguagesThenThrow()
    // {
    //    _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _variantsListCache.GetAllInvariantMarketDescriptionsAsync(null));
    // }
    //
    // [Fact]
    // public async Task GetAllWhenCallingWithEmptyLanguagesThenThrow()
    // {
    //     _ = await Assert.ThrowsAsync<ArgumentException>(() => _variantsListCache.GetAllInvariantMarketDescriptionsAsync(ArraySegment<CultureInfo>.Empty));
    // }

    [Fact]
    public async Task WhenMarketDescriptionsReceivedButWrongDtoTypeThenItIsIgnored()
    {
        var apiMds = await GetVariantsListFromXmlFile(_cultures[0]);
        var dtoMds = apiMds.variant.Select(s => new VariantDescriptionDto(s));
        var entityListVariantDescriptions = new EntityList<VariantDescriptionDto>(dtoMds);

        var result = await _variantsListCache.CacheAddDtoAsync(Urn.Parse("sr:markets:123"),
                                                               entityListVariantDescriptions,
                                                               _cultures[0],
                                                               EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(DtoType.VariantDescriptionList),
                                                               null);

        Assert.False(result);
        Assert.Equal(0, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task WhenDtoItemDoesNotMuchExpectedThenItIsIgnored()
    {
        var apiMds = await GetVariantsListFromXmlFile(_cultures[0]);
        var dtoMds = apiMds.variant.Select(s => new VariantDescriptionDto(s));

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), dtoMds, _cultures[0], DtoType.VariantDescriptionList, null);

        Assert.Equal(0, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task WhenCacheDisposedThenAddingIsIgnored()
    {
        var apiMds = await GetVariantsListFromXmlFile(_cultures[0]);
        var dtoMds = apiMds.variant.Select(s => new VariantDescriptionDto(s));
        var entityListMarketDescriptions = new EntityList<VariantDescriptionDto>(dtoMds);
        _variantsListCache.Dispose();

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), entityListMarketDescriptions, _cultures[0], DtoType.VariantDescriptionList, null);

        Assert.Equal(0, _variantsListMemoryCache.Count());
    }

    [Fact]
    public async Task WhenDtoItemDoesNotMuchExpectedThenMismatchIsLogged()
    {
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantDescriptionListCache));
        var apiMds = await GetVariantsListFromXmlFile(_cultures[0]);
        var dtoMds = apiMds.variant.Select(s => new VariantDescriptionDto(s));

        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:123"), dtoMds, _cultures[0], DtoType.VariantDescriptionList, null);

        Assert.Empty(logCache.Messages);
        Assert.Single(logExec.Messages);
        Assert.Single(logExec.Messages, w => w.Contains("Invalid data"));
    }

    [Fact]
    public async Task HealthCheckExists()
    {
        var healthCheck = await _variantsListCache.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, healthCheck.Status);
    }

    private static async Task<variant_descriptions> GetVariantsListFromXmlFile(CultureInfo language)
    {
        var resourceName = $"variant_market_descriptions_{language.TwoLetterISOLanguageName}.xml";
        var restDeserializer = new Deserializer<variant_descriptions>();
        await using var stream = FileHelper.GetResource(resourceName);

        return stream != null
                   ? restDeserializer.Deserialize(stream)
                   : throw new InvalidOperationException("No data");
    }

    // private async Task<MarketDescriptionCacheItem> GetCacheItem(string variantId, IReadOnlyCollection<CultureInfo> languages)
    // {
    //     return await _variantsListCache.GetVariantDescriptorAsync(variantId, languages).ConfigureAwait(false);
    // }
}
