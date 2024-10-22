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

public class VariantMarketDescriptionCacheTests
{
    private readonly VariantMarketDescriptionCache _variantMarketDescriptionCache;

    private readonly TestDataRouterManager _dataRouterManager;
    private readonly ICacheStore<string> _variantMarketDescriptionMemoryCache;
    private readonly CacheManager _cacheManager;
    private readonly IReadOnlyList<CultureInfo> _cultures;
    private readonly MarketDescriptionDto _exampleVariantMarketDescriptionDtoEn;
    private const int MarketIdFor534 = 534;
    private const string VariantValueFor534 = "pre:markettext:168883";
    private readonly XunitLoggerFactory _testLoggerFactory;
    private readonly desc_market _apiMd1;

    public VariantMarketDescriptionCacheTests(ITestOutputHelper outputHelper)
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

        _variantMarketDescriptionMemoryCache = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache, memoryCache, _testLoggerFactory.CreateLogger(typeof(Cache)), null, TestConfiguration.GetConfig().Cache.VariantMarketDescriptionCacheTimeout, 1);

        IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();
        _cacheManager = new CacheManager();
        _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);

        _variantMarketDescriptionCache = new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, _cacheManager, _testLoggerFactory);

        var mdXml1 = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
                        <market_descriptions response_code='OK'>
                            <market id='768' name='Player points (incl. overtime)' variant='pre:playerprops:35432179:608000'>
                                <outcomes>
                                    <outcome id='pre:playerprops:35432179:608000:22' name='Kyle Anderson 22+'/>
                                    <outcome id='pre:playerprops:35432179:608000:23' name='Kyle Anderson 23+'/>
                                    <outcome id='pre:playerprops:35432179:608000:24' name='Kyle Anderson 24+'/>
                                    <outcome id='pre:playerprops:35432179:608000:10' name='Kyle Anderson 10+'/>
                                    <outcome id='pre:playerprops:35432179:608000:11' name='Kyle Anderson 11+'/>
                                    <outcome id='pre:playerprops:35432179:608000:12' name='Kyle Anderson 12+'/>
                                    <outcome id='pre:playerprops:35432179:608000:13' name='Kyle Anderson 13+'/>
                                    <outcome id='pre:playerprops:35432179:608000:14' name='Kyle Anderson 14+'/>
                                    <outcome id='pre:playerprops:35432179:608000:15' name='Kyle Anderson 15+'/>
                                    <outcome id='pre:playerprops:35432179:608000:16' name='Kyle Anderson 16+'/>
                                    <outcome id='pre:playerprops:35432179:608000:17' name='Kyle Anderson 17+'/>
                                    <outcome id='pre:playerprops:35432179:608000:18' name='Kyle Anderson 18+'/>
                                    <outcome id='pre:playerprops:35432179:608000:19' name='Kyle Anderson 19+'/>
                                    <outcome id='pre:playerprops:35432179:608000:7' name='Kyle Anderson 7+'/>
                                    <outcome id='pre:playerprops:35432179:608000:8' name='Kyle Anderson 8+'/>
                                    <outcome id='pre:playerprops:35432179:608000:9' name='Kyle Anderson 9+'/>
                                    <outcome id='pre:playerprops:35432179:608000:20' name='Kyle Anderson 20+'/>
                                    <outcome id='pre:playerprops:35432179:608000:21' name='Kyle Anderson 21+'/>
                                </outcomes>
                            </market>
                        </market_descriptions>";

        _apiMd1 = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(mdXml1).market[0];
        _exampleVariantMarketDescriptionDtoEn = new MarketDescriptionDto(_apiMd1);
    }

    [Fact]
    public void InitialCallsDone()
    {
        Assert.Equal(3, _cultures.Count);
        Assert.NotNull(_variantMarketDescriptionCache);
        Assert.Empty(_variantMarketDescriptionMemoryCache.GetKeys());
        Assert.NotNull(_dataRouterManager);
        Assert.NotNull(_cacheManager);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsAssignableFrom<IMarketDescriptionCache>(_variantMarketDescriptionCache);
        _ = Assert.IsAssignableFrom<ISdkCache>(_variantMarketDescriptionCache);
        _ = Assert.IsAssignableFrom<IHealthStatusProvider>(_variantMarketDescriptionCache);
    }

    [Fact]
    public void ImplementationRegisteredToConsumeSingleDto()
    {
        _ = Assert.Single(_variantMarketDescriptionCache.RegisteredDtoTypes);
        Assert.Equal(DtoType.MarketDescription, _variantMarketDescriptionCache.RegisteredDtoTypes.First());
    }

    [Fact]
    public void ConstructorWhenNullCacheStoreThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantMarketDescriptionCache(null, _dataRouterManager, new MappingValidatorFactory(), _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullDataRouterManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, null, new MappingValidatorFactory(), _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullMappingValidatorFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _dataRouterManager, null, _cacheManager, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullCacheManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _dataRouterManager, new MappingValidatorFactory(), null, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLoggerFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _dataRouterManager, new MappingValidatorFactory(), _cacheManager, null));
    }

    [Fact]
    public void GenerateCacheKeyRequiresIdAndVariant()
    {
        const int marketId = 123;
        const string variant = "some-variant";

        var cacheKey = VariantMarketDescriptionCache.GenerateCacheKey(marketId, variant);

        Assert.Equal($"{marketId}_{variant}", cacheKey);
    }

    [Fact]
    public void GenerateCacheKeyWhenIdZeroThenDoNotThrow()
    {
        const int marketId = 0;
        const string variant = "some-variant";

        var cacheKey = VariantMarketDescriptionCache.GenerateCacheKey(marketId, variant);

        Assert.Equal($"{marketId}_{variant}", cacheKey);
    }

    [Fact]
    public void GenerateCacheKeyWhenIdNegativeThenDoNotThrow()
    {
        const int marketId = -10;
        const string variant = "some-variant";

        var cacheKey = VariantMarketDescriptionCache.GenerateCacheKey(marketId, variant);

        Assert.Equal($"{marketId}_{variant}", cacheKey);
    }

    [Fact]
    public void GenerateCacheKeyWhenVariantEmptyThenThrow()
    {
        var anyMarketId = Random.Shared.Next();
        var variant = string.Empty;

        _ = Assert.Throws<ArgumentException>(() => VariantMarketDescriptionCache.GenerateCacheKey(anyMarketId, variant));
    }

    [Fact]
    public void GenerateCacheKeyWhenVariantNullThenThrow()
    {
        var anyMarketId = Random.Shared.Next();

        _ = Assert.Throws<ArgumentNullException>(() => VariantMarketDescriptionCache.GenerateCacheKey(anyMarketId, null));
    }

    [Fact]
    public async Task UpdateCacheItemDoesNotUpdateSource()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        mdCacheItem.SetFetchInfo("some-source");
        var initialSource = mdCacheItem.SourceCache;

        _variantMarketDescriptionCache.UpdateCacheItem((int)_exampleVariantMarketDescriptionDtoEn.Id, _exampleVariantMarketDescriptionDtoEn.Variant);

        Assert.NotEqual(initialSource, mdCacheItem.SourceCache);
        Assert.Equal(_variantMarketDescriptionCache.CacheName, mdCacheItem.SourceCache);
    }

    [Fact]
    public async Task UpdateCacheItemWhenDisposedThenIgnores()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        mdCacheItem.SetFetchInfo("some-source");
        var initialSource = mdCacheItem.SourceCache;
        _variantMarketDescriptionCache.Dispose();

        _variantMarketDescriptionCache.UpdateCacheItem((int)_exampleVariantMarketDescriptionDtoEn.Id, _exampleVariantMarketDescriptionDtoEn.Variant);

        Assert.Equal(initialSource, mdCacheItem.SourceCache);
    }

    [Fact]
    public void Disposed()
    {
        _variantMarketDescriptionCache.Dispose();

        Assert.NotNull(_variantMarketDescriptionCache);
        Assert.True(_variantMarketDescriptionCache.IsDisposed());
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Test designed for this")]
    public void DisposingTwiceDoesNotThrow()
    {
        _variantMarketDescriptionCache.Dispose();
        _variantMarketDescriptionCache.Dispose();

        Assert.NotNull(_variantMarketDescriptionCache);
        Assert.True(_variantMarketDescriptionCache.IsDisposed());
    }

    [Fact]
    public void DisposingWhenMultipleParallelCallsThenDoesNotThrow()
    {
        Parallel.For(1, 10, (_) => _variantMarketDescriptionCache.Dispose());

        Assert.NotNull(_variantMarketDescriptionCache);
        Assert.True(_variantMarketDescriptionCache.IsDisposed());
    }

    [Fact]
    public async Task CacheDeleteItemWithUrnWhenCallingThenNothingIsDeleted()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var marketUrn = Urn.Parse($"sr:market:{mdCacheItem.Id}");

        _variantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.True(_variantMarketDescriptionCache.CacheHasItem(VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant), CacheItemType.All));
    }

    [Fact]
    public async Task CacheDeleteItemWithUrnWhenCallingThenDeletingIsIgnored()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var marketUrn = Urn.Parse($"sr:market:{mdCacheItem.Id}");
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantMarketDescriptionCache));

        _variantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.All);

        Assert.Empty(logCache.Messages);
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithUrnWhenCallingWithRegisteredDtoTypeThenNothingIsDeleted()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var marketUrn = Urn.Parse($"sr:market:{mdCacheItem.Id}");

        _variantMarketDescriptionCache.CacheDeleteItem(marketUrn, CacheItemType.MarketDescription);

        Assert.True(_variantMarketDescriptionCache.CacheHasItem(VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant), CacheItemType.All));
    }

    [Fact]
    public async Task CacheDeleteItemWithUrnWhenCallingWithUnregisteredDtoTypeThenNothingIsDeleted()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var marketUrn = Urn.Parse($"sr:market:{mdCacheItem.Id}");

        _variantMarketDescriptionCache.CacheDeleteItem(marketUrn, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription));

        Assert.True(_variantMarketDescriptionCache.CacheHasItem(VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant), CacheItemType.All));
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyThenCacheItemIsDeleted()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);

        _variantMarketDescriptionCache.CacheDeleteItem(cacheItemKey, CacheItemType.All);

        Assert.False(_variantMarketDescriptionCache.CacheHasItem(cacheItemKey, CacheItemType.All));
        Assert.Empty(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingThenLogIsCreated()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantMarketDescriptionCache));

        _variantMarketDescriptionCache.CacheDeleteItem(cacheItemKey, CacheItemType.All);

        _ = Assert.Single(logCache.Messages, w => w.Contains("delete variant market", StringComparison.InvariantCultureIgnoreCase));
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndRegisteredDtoTypeThenCacheItemIsDeleted()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);

        _variantMarketDescriptionCache.CacheDeleteItem(cacheItemKey, CacheItemType.MarketDescription);

        Assert.False(_variantMarketDescriptionCache.CacheHasItem(VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant), CacheItemType.All));
        Assert.Empty(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndUnregisteredDtoTypeThenNothingIsDeleted()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);

        _variantMarketDescriptionCache.CacheDeleteItem(cacheItemKey, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription));

        Assert.True(_variantMarketDescriptionCache.CacheHasItem(VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant), CacheItemType.All));
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWithStringIdWhenCallingWithValidKeyAndRegisteredDtoTypeThenFetchedVariantIsDeleted()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);
        var fetchedVariant = $"{mdCacheItem.Id}_{mdCacheItem.Variant}_{_cultures[0].TwoLetterISOLanguageName}";
        _ = Assert.Contains(fetchedVariant, _variantMarketDescriptionCache.FetchedVariants);

        _variantMarketDescriptionCache.CacheDeleteItem(cacheItemKey, CacheItemType.MarketDescription);

        Assert.DoesNotContain(fetchedVariant, _variantMarketDescriptionCache.FetchedVariants);
    }

    [Fact]
    public async Task CacheDeleteItemWhenNullIdThenNothingIsDeleted()
    {
        _ = await LoadDefaultExampleEn();
        _ = Assert.Single(_variantMarketDescriptionCache.FetchedVariants);
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());

        _variantMarketDescriptionCache.CacheDeleteItem((Urn)null, CacheItemType.MarketDescription);

        _ = Assert.Single(_variantMarketDescriptionCache.FetchedVariants);
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingThenAlwaysFalse()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var marketUrn = Urn.Parse($"sr:market:{mdCacheItem.Id}");

        Assert.False(_variantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithRegisteredDtoTypeThenAlwaysFalse()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var marketUrn = Urn.Parse($"sr:market:{mdCacheItem.Id}");

        Assert.False(_variantMarketDescriptionCache.CacheHasItem(marketUrn, CacheItemType.MarketDescription));
    }

    [Fact]
    public async Task CacheHasItemWithUrnWhenCallingWithUnregisteredDtoTypeThenAlwaysFalse()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var marketUrn = Urn.Parse($"sr:market:{mdCacheItem.Id}");

        Assert.False(_variantMarketDescriptionCache.CacheHasItem(marketUrn, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription)));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyThenReturnTrue()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);

        Assert.True(_variantMarketDescriptionCache.CacheHasItem(cacheItemKey, CacheItemType.All));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyAndRegisteredDtoTypeThenReturnTrue()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);

        Assert.True(_variantMarketDescriptionCache.CacheHasItem(cacheItemKey, CacheItemType.MarketDescription));
    }

    [Fact]
    public async Task CacheHasItemWithStringIdWhenCallingWithValidKeyAndUnregisteredDtoTypeThenReturnFalse()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        var cacheItemKey = VariantMarketDescriptionCache.GenerateCacheKey(mdCacheItem.Id, mdCacheItem.Variant);

        Assert.False(_variantMarketDescriptionCache.CacheHasItem(cacheItemKey, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.MarketDescription)));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionFromSinglePreOutcomeTextCallsApiEndpoint()
    {
        PrepareDataRouterManagerToGetMarket534();

        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());

        Assert.NotNull(marketDescription);
        Assert.Equal(MarketIdFor534, marketDescription.Id);
        Assert.Equal(9, marketDescription.Outcomes.Count());
        Assert.Null(marketDescription.Mappings);
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[1]));
        Assert.NotEqual(marketDescription.GetName(_cultures[0]), marketDescription.GetName(_cultures[2]));
        Assert.NotEqual(marketDescription.GetName(_cultures[1]), marketDescription.GetName(_cultures[2]));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionWhenVariantMissingThenThrow()
    {
        PrepareDataRouterManagerToGetMarket534();

        _ = await Assert.ThrowsAsync<ArgumentException>(() => _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, null, _cultures));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionWhenApiCallFailsThenThrow()
    {
        PrepareDataRouterManagerToGetMarket534();
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("single_variant_description.xml", new CommunicationException("Not found", "http://local/someurl", HttpStatusCode.NotFound, null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionWhenDeserializationFailsThenThrow()
    {
        PrepareDataRouterManagerToGetMarket534();
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("single_variant_description.xml", new DeserializationException("Not found", null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionWhenMappingFailsThenThrow()
    {
        PrepareDataRouterManagerToGetMarket534();
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("single_variant_description.xml", new MappingException("Not found", "some-property", "property-value", nameof(market_descriptions), null)));

        _ = await Assert.ThrowsAsync<CacheItemNotFoundException>(() => _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures));
    }

    [Fact]
    public async Task GetVariantMarketDescriptionWhenUnhandledExceptionIsThrownThenOriginalExceptionIsThrown()
    {
        PrepareDataRouterManagerToGetMarket534();
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("single_variant_description.xml", new UriFormatException("Not found")));

        _ = await Assert.ThrowsAsync<UriFormatException>(() => _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures));
    }

    [Fact]
    public async Task PrefetchedVariantMarketDescriptionWhenRequestedReturnsCachedVersion()
    {
        PrepareDataRouterManagerToGetMarket534();
        _ = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures);
        _dataRouterManager.ResetMethodCall();

        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures);

        Assert.NotNull(marketDescription);
        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WithOneLanguagePrefetchedWhenRequestedThenMakesApiCallsForMissingLanguages()
    {
        PrepareDataRouterManagerToGetMarket534();
        _ = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] });
        _dataRouterManager.ResetMethodCall();

        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures);

        Assert.NotNull(marketDescription);
        Assert.Equal(2, _dataRouterManager.TotalRestCalls);
        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenMultipleConcurrentRequestsThenMakesApiCallsForMissingLanguagesJustOnce()
    {
        PrepareDataRouterManagerToGetMarket534();
        _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(100));

        var tasks = new List<Task>
        {
            _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] }),
            _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] }),
            _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] })
        };
        await Task.WhenAll(tasks);

        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenMultipleConcurrentRequestsForMultipleLanguagesThenMakesApiCallsForMissingLanguagesJustOnce()
    {
        PrepareDataRouterManagerToGetMarket534();
        _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(100));

        var tasks = new List<Task>
        {
            _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures),
            _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures),
            _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures)
        };
        await Task.WhenAll(tasks);

        Assert.Equal(_cultures.Count, _dataRouterManager.TotalRestCalls);
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenApiThrowsForMultipleRequestsThenMakesApiCallsJustOnce()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("single_variant_description.xml", new InvalidOperationException()));

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] }));
        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] });

        Assert.Null(marketDescription);
        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenApiThrowsForMultipleLanguageRequestsThenMakesApiCallsJustOnce()
    {
        _dataRouterManager.UriExceptions.Add(new Tuple<string, Exception>("single_variant_description.xml", new InvalidOperationException()));

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures));
        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, _cultures);

        Assert.Null(marketDescription);
        Assert.Equal(3, _dataRouterManager.TotalRestCalls);
        Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenFetchingIsBlockedThenNoApiCallIsMade()
    {
        PrepareDataRouterManagerToGetMarket534();
        var fetchedVariant = $"{MarketIdFor534}_{VariantValueFor534}_{_cultures[0].TwoLetterISOLanguageName}";
        _variantMarketDescriptionCache.FetchedVariants[fetchedVariant] = DateTime.Now;

        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] });

        Assert.Null(marketDescription);
        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task WhenFetchingIsCheckedThenAlsoDeleteOldFetchedEntries()
    {
        PrepareDataRouterManagerToGetMarket534();
        for (var i = 0; i < 1100; i++)
        {
            var fetchedDate = DateTime.Now.AddSeconds(-Random.Shared.Next(100, 1000));
            var previousFetchedVariant = $"{Random.Shared.Next(1, 1000)}_pre:markettext:{Random.Shared.Next(1000, 9999)}_{_cultures[0].TwoLetterISOLanguageName}";
            _variantMarketDescriptionCache.FetchedVariants[previousFetchedVariant] = fetchedDate;
        }
        Assert.True(_variantMarketDescriptionCache.FetchedVariants.Count > 1000);

        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] });

        Assert.True(_variantMarketDescriptionCache.FetchedVariants.Count < 1000);
        Assert.NotNull(marketDescription);
    }

    [Fact]
    public async Task WhenFetchingIsCheckedThenAllowRequestThatAreBeforeThreshold()
    {
        PrepareDataRouterManagerToGetMarket534();
        var fetchedDate = DateTime.Now.AddSeconds(-Random.Shared.Next(100, 1000));
        var previousFetchedVariant = $"{MarketIdFor534}_{VariantValueFor534}_{_cultures[0].TwoLetterISOLanguageName}";
        _variantMarketDescriptionCache.FetchedVariants[previousFetchedVariant] = fetchedDate;
        _dataRouterManager.ResetMethodCall();

        var marketDescription = await _variantMarketDescriptionCache.GetMarketDescriptionAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] });

        Assert.NotNull(marketDescription);
        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
    }

    [Fact]
    public async Task GetMarketInternalWhenPrefetchedThenNoApiCallNeeded()
    {
        PrepareDataRouterManagerToGetMarket534();
        _ = await _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, _cultures);
        _dataRouterManager.ResetMethodCall();

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] });

        Assert.NotNull(mdCacheItem);
        Assert.NotEmpty(mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetMarketInternalWhenConcurrentCallsThenOnlyOneApiCallIsMade()
    {
        PrepareDataRouterManagerToGetMarket534();
        _dataRouterManager.ResetMethodCall();
        _dataRouterManager.AddDelay(TimeSpan.FromMilliseconds(100));
        var task1 = Task.Run(() => _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, _cultures));
        var task2 = Task.Run(() => _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, _cultures));

        _ = await Task.WhenAll(task1, task2);

        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetMarketInternalWhenNothingPrefetchedThenGetTheLanguage()
    {
        PrepareDataRouterManagerToGetMarket534();

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0] });

        Assert.NotNull(mdCacheItem);
        Assert.NotEmpty(mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task GetMarketInternalWhenNothingPrefetchedThenGetAllTheLanguages()
    {
        PrepareDataRouterManagerToGetMarket534();

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, _cultures);

        Assert.NotNull(mdCacheItem);
        Assert.NotEmpty(mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(_cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task GetMarketInternalWhenSomePrefetchedAndOneMissingThenFetchedTheMissing()
    {
        PrepareDataRouterManagerToGetMarket534();
        _ = await _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, new[] { _cultures[0], _cultures[1] });
        _dataRouterManager.ResetMethodCall();

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(MarketIdFor534, VariantValueFor534, _cultures);

        Assert.NotNull(mdCacheItem);
        Assert.NotEmpty(mdCacheItem.GetName(_cultures[2]));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task GetMarketInternalWhenMissingMarketNameAndRecentlyFetchedThenTheLanguageIsNotFetchedAgain()
    {
        _apiMd1.name = null;
        await LoadApiMd1(DateTime.Now, _cultures[0]);

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(_apiMd1.id, _apiMd1.variant, new[] { _cultures[0] });

        Assert.NotNull(mdCacheItem);
        Assert.Null(mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task GetMarketInternalWhenMissingMarketNameAndNotRecentlyFetchedThenTheLanguageIsFetchedAgain()
    {
        _apiMd1.name = null;
        await LoadApiMd1(DateTime.Now.AddMinutes(-1), _cultures[0]);

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(_apiMd1.id, _apiMd1.variant, new[] { _cultures[0] });

        Assert.NotNull(mdCacheItem);
        Assert.Null(mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetMarketInternalWhenMissingOutcomeNameAndRecentlyFetchedThenTheLanguageIsNotFetchedAgain()
    {
        _apiMd1.outcomes[3].name = null;
        await LoadApiMd1(DateTime.Now, _cultures[0]);

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(_apiMd1.id, _apiMd1.variant, new[] { _cultures[0] });

        Assert.NotNull(mdCacheItem);
        Assert.Null(mdCacheItem.Outcomes.ElementAt(3).GetName(_cultures[0]));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
    }

    [Fact]
    public async Task GetMarketInternalWhenMissingOutcomeNameAndNotRecentlyFetchedThenTheLanguageIsFetchedAgain()
    {
        _apiMd1.outcomes[3].name = null;
        await LoadApiMd1(DateTime.Now.AddMinutes(-1), _cultures[0]);

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(_apiMd1.id, _apiMd1.variant, new[] { _cultures[0] });

        Assert.NotNull(mdCacheItem);
        Assert.Null(mdCacheItem.Outcomes.ElementAt(3).GetName(_cultures[0]));
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetMarketInternalWhenMissingMarketNameAndRecentlyFetchedThenTheLanguageIsNotFetchedAgainTheRestAre()
    {
        _apiMd1.name = null;
        await LoadApiMd1(DateTime.Now, _cultures[0]);

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(_apiMd1.id, _apiMd1.variant, _cultures);

        Assert.Null(mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task GetMarketInternalWhenMissingMarketNameAndNotRecentlyFetchedThenTheLanguageIsFetchedAgainWithAllTheRest()
    {
        _apiMd1.name = null;
        await LoadApiMd1(DateTime.Now.AddMinutes(-1), _cultures[0]);

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync(_apiMd1.id, _apiMd1.variant, _cultures);

        Assert.Null(mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointVariantMarketDescription));
    }

    [Fact]
    public async Task HealthCheckExists()
    {
        var healthCheck = await _variantMarketDescriptionCache.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, healthCheck.Status);
    }

    private void PrepareDataRouterManagerToGetMarket534()
    {
        _dataRouterManager.UriReplacements.Add(new Tuple<string, string>($"{MarketIdFor534}_{VariantValueFor534}", $"variant_market_description_{MarketIdFor534}_culture.xml"));

        Assert.Equal(0, _variantMarketDescriptionMemoryCache.Count());
        Assert.Equal(0, _dataRouterManager.TotalRestCalls);
    }

    private async Task<MarketDescriptionCacheItem> LoadDefaultExampleEn()
    {
        _ = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0], DtoType.MarketDescription, null);

        var fetchedVariant = $"{_exampleVariantMarketDescriptionDtoEn.Id}_{_exampleVariantMarketDescriptionDtoEn.Variant}_{_cultures[0].TwoLetterISOLanguageName}";
        _variantMarketDescriptionCache.FetchedVariants[fetchedVariant] = DateTime.Now;

        return await _variantMarketDescriptionCache.GetMarketInternalAsync((int)_exampleVariantMarketDescriptionDtoEn.Id, _exampleVariantMarketDescriptionDtoEn.Variant, new[] { _cultures[0] }).ConfigureAwait(false);
    }

    private async Task LoadApiMd1(DateTime fetchedDate, CultureInfo language)
    {
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_apiMd1.id}"), new MarketDescriptionDto(_apiMd1), language, DtoType.MarketDescription, null);
        var fetchedVariant = $"{_apiMd1.id}_{_apiMd1.variant}_{_cultures[0].TwoLetterISOLanguageName}";
        _variantMarketDescriptionCache.FetchedVariants[fetchedVariant] = fetchedDate;
        Assert.Equal(1, _variantMarketDescriptionMemoryCache.Count());
        Assert.True(isSuccess);
    }
}
