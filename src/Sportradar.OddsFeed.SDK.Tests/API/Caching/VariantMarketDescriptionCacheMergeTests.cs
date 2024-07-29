// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
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

public class VariantMarketDescriptionCacheMergeTests
{
    private readonly VariantMarketDescriptionCache _variantMarketDescriptionCache;

    private readonly ICacheStore<string> _variantMarketDescriptionMemoryCache;
    private readonly IReadOnlyList<CultureInfo> _cultures;
    private MarketDescriptionDto _exampleVariantMarketDescriptionDtoEn;
    private MarketDescriptionDto _exampleVariantMarketDescriptionDtoHu;
    private readonly XunitLoggerFactory _testLoggerFactory;
    private readonly desc_market _apiMdEn;
    private readonly desc_market _apiMdHu;

    public VariantMarketDescriptionCacheMergeTests(ITestOutputHelper outputHelper)
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

        _variantMarketDescriptionMemoryCache = new CacheStore<string>(UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache, memoryCache, _testLoggerFactory.CreateLogger(typeof(Cache)), null,
            TestConfiguration.GetConfig().Cache.VariantMarketDescriptionCacheTimeout, 1);

        IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();
        var cacheManager = new CacheManager();
        var dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

        _variantMarketDescriptionCache = new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, dataRouterManager, mappingValidatorFactory, cacheManager, _testLoggerFactory);

        const string mdXml1 = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
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

        const string mdXml2 = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
                    <market_descriptions response_code='OK'>
                        <market id='768' name='Játékos pontok' variant='pre:playerprops:35432179:608000'>
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

        _apiMdEn = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(mdXml1).market[0];
        _apiMdHu = DeserializerHelper.GetDeserializedApiMessage<market_descriptions>(mdXml2).market[0];
        _exampleVariantMarketDescriptionDtoEn = new MarketDescriptionDto(_apiMdEn);
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);
    }

    [Fact]
    public async Task AddDtoItem()
    {
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task AddDtoItemCreatesCacheItemWithBaseInfoCorrectlyInitialized()
    {
        var mdCacheItem = await LoadDefaultExampleEn();

        Assert.NotNull(mdCacheItem);
        _ = Assert.Single(mdCacheItem.Names.Keys);
        Assert.Equal(_cultures[0], mdCacheItem.Names.Keys.First());
        Assert.True(mdCacheItem.HasTranslationsFor(_cultures[0]));
        Assert.NotEmpty(mdCacheItem.Variant);
        Assert.False(mdCacheItem.SourceCache.IsNullOrEmpty());
    }

    [Fact]
    public async Task AddDtoItemWhenWrongDtoTypeThenDtoIsNotSaved()
    {
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0],
            DtoType.MarketDescriptionList, null);

        Assert.False(isSuccess);
        Assert.Empty(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task AddDtoItemWhenDtoObjectDoesNotMatchDtoTypeThenDtoIsNotSaved()
    {
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), new[] { _exampleVariantMarketDescriptionDtoEn },
            _cultures[0], DtoType.MarketDescription, null);

        Assert.False(isSuccess);
        Assert.Empty(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task AddDtoItemWhenMarketNameIsNullThenItIsSaved()
    {
        _apiMdEn.name = null;
        _exampleVariantMarketDescriptionDtoEn = new MarketDescriptionDto(_apiMdEn);
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task AddDtoItemWhenOutcomeNameIsNullThenItIsSaved()
    {
        _apiMdEn.outcomes[0].name = null;
        _exampleVariantMarketDescriptionDtoEn = new MarketDescriptionDto(_apiMdEn);
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task AddDtoItemWhenMarketNameIsEmptyThenItIsSaved()
    {
        _apiMdEn.name = string.Empty;
        _exampleVariantMarketDescriptionDtoEn = new MarketDescriptionDto(_apiMdEn);
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task AddDtoItemWhenOutcomeNameIsEmptyThenItIsSaved()
    {
        _apiMdEn.outcomes[0].name = string.Empty;
        _exampleVariantMarketDescriptionDtoEn = new MarketDescriptionDto(_apiMdEn);
        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        _ = Assert.Single(_variantMarketDescriptionMemoryCache.GetKeys());
    }

    [Fact]
    public async Task AddDtoItemWhenCallingWithoutLanguageThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, null, DtoType.MarketDescription, null));
    }

    [Fact]
    public async Task AddDtoItemWhenAfterMergingThrowsThenNextRequestSucceed()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, null, DtoType.MarketDescription, null));

        var success = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0],
            DtoType.MarketDescription, null);

        Assert.True(success);
    }

    [Fact]
    public async Task MergeCacheItemsForValidDtos()
    {
        var mdCacheItem = await LoadDefaultExampleEn();

        _ = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.Equal(_exampleVariantMarketDescriptionDtoEn.Id, mdCacheItem.Id);
        Assert.Equal(_exampleVariantMarketDescriptionDtoEn.Outcomes.Count, mdCacheItem.Outcomes.Count);
        Assert.Equal(_exampleVariantMarketDescriptionDtoEn.Name, mdCacheItem.GetName(_cultures[0]));
        Assert.Equal(_exampleVariantMarketDescriptionDtoHu.Name, mdCacheItem.GetName(_cultures[2]));
    }

    [Fact]
    public async Task MergeCacheItemsWhenCacheIsDisposedReturnNull()
    {
        _variantMarketDescriptionCache.Dispose();

        var mdCacheItem = await _variantMarketDescriptionCache.GetMarketInternalAsync((int)_exampleVariantMarketDescriptionDtoEn.Id, _exampleVariantMarketDescriptionDtoEn.Variant,
            new[] { _cultures[0] });

        Assert.Null(mdCacheItem);
    }

    [Fact]
    public async Task MergeExistingCacheItemsWhenCacheIsDisposedThenDoNotMerge()
    {
        var mdCacheItem = await LoadDefaultExampleEn();

        _variantMarketDescriptionCache.Dispose();

        _ = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.NotNull(mdCacheItem);
        Assert.Equal(_exampleVariantMarketDescriptionDtoEn.Id, mdCacheItem.Id);
        Assert.Equal(_exampleVariantMarketDescriptionDtoEn.Name, mdCacheItem.GetName(_cultures[0]));
        Assert.Null(mdCacheItem.GetName(_cultures[2]));
    }

    [Fact]
    public async Task MergeDtoItemWhenSuccessfulThenNoMergeProblemsAreLogged()
    {
        _ = await LoadDefaultExampleEn();
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantMarketDescriptionCache));

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.Empty(logCache.Messages);
        Assert.Empty(logExec.Messages);
    }

    [Fact]
    public async Task MergeDtoItemWhenSecondMarketNameIsNull()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        _apiMdHu.name = null;
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.NotNull(mdCacheItem);
        Assert.Equal(_apiMdEn.name, mdCacheItem.GetName(_cultures[0]));
        Assert.Null(mdCacheItem.GetName(_cultures[2]));
    }

    [Fact]
    public async Task MergeDtoItemWhenSecondOutcomeNameIsNull()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        _apiMdHu.outcomes[0].name = null;
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.NotNull(mdCacheItem);
        Assert.Equal(_apiMdEn.outcomes[0].name, mdCacheItem.Outcomes.First().GetName(_cultures[0]));
        Assert.Null(mdCacheItem.Outcomes.First().GetName(_cultures[2]));
    }

    [Fact]
    public async Task MergeDtoItemWhenSecondMarketNameIsEmpty()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        _apiMdHu.name = string.Empty;
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.NotNull(mdCacheItem);
        Assert.Equal(_apiMdEn.name, mdCacheItem.GetName(_cultures[0]));
        Assert.Empty(mdCacheItem.GetName(_cultures[2]));
    }

    [Fact]
    public async Task MergeDtoItemWhenSecondOutcomeNameIsEmpty()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        _apiMdHu.outcomes[0].name = string.Empty;
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.NotNull(mdCacheItem);
        Assert.Equal(_apiMdEn.outcomes[0].name, mdCacheItem.Outcomes.First().GetName(_cultures[0]));
        Assert.Empty(mdCacheItem.Outcomes.First().GetName(_cultures[2]));
    }

    [Fact]
    public async Task MergeWhenSecondHasDifferentOutcomeThenOutcomeIsNotAdded()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        _apiMdHu.outcomes[0].id += "test";
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.Equal(_apiMdEn.outcomes.Length, mdCacheItem.Outcomes.Count);
        Assert.DoesNotContain(mdCacheItem.Outcomes, w => w.Id.Equals(_apiMdHu.outcomes[0].id, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task MergeWhenSecondHasDifferentOutcomeThenOutcomeProblemIsReported()
    {
        await LoadDefaultExampleEn();
        _apiMdHu.outcomes[0].id += "test";
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantMarketDescriptionCache));

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.Empty(logCache.Messages);
        Assert.Equal(3, logExec.Messages.Count);
        Assert.Equal(2, logExec.CountByLevel(LogLevel.Debug));
        Assert.Equal(1, logExec.CountByLevel(LogLevel.Warning));
        _ = Assert.Single(logExec.Messages.Where(w => w.Contains("Could not merge outcome", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task MergeWhenSecondHasDifferentMappingThenMappingIsNotAdded()
    {
        var mdCacheItem = await LoadDefaultExampleEn();
        _apiMdHu.mappings = new[] { new mappingsMapping { product_id = 5, product_ids = "5", sport_id = "sr:sport:21", market_id = "61" } };
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.Null(mdCacheItem.Mappings);
    }

    [Fact]
    public async Task MergeWhenSecondHasDifferentMappingThenMappingProblemIsReported()
    {
        _ = await LoadDefaultExampleEn();
        _apiMdHu.mappings = new[] { new mappingsMapping { product_id = 5, product_ids = "5", sport_id = "sr:sport:21", market_id = "61" } };
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantMarketDescriptionCache));

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.Empty(logCache.Messages);
        Assert.Equal(3, logExec.Messages.Count);
        Assert.Equal(2, logExec.CountByLevel(LogLevel.Debug));
        Assert.Equal(1, logExec.CountByLevel(LogLevel.Warning));
        _ = Assert.Single(logExec.Messages.Where(w => w.Contains("Could not merge mapping", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task MergeWhenSecondHasDifferentOutcomeThenComparePrintIsLogged()
    {
        AddDummyDescriptionAndSpecifiersAndMappingsAndWrongOutcome();
        var mdCacheItem = await LoadDefaultExampleEn();
        var logCache = _testLoggerFactory.GetOrCreateLogger(typeof(Cache));
        var logExec = _testLoggerFactory.GetOrCreateLogger(typeof(VariantMarketDescriptionCache));

        var isSuccess = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoHu.Id}"), _exampleVariantMarketDescriptionDtoHu, _cultures[2],
            DtoType.MarketDescription, null);

        Assert.True(isSuccess);
        Assert.NotNull(mdCacheItem);
        Assert.Empty(logCache.Messages);
        Assert.Equal(3, logExec.Messages.Count);
        Assert.Equal(2, logExec.CountByLevel(LogLevel.Debug));
        Assert.Equal(1, logExec.CountByLevel(LogLevel.Warning));
        _ = Assert.Single(logExec.Messages.Where(w => w.Contains("Original Id=", StringComparison.InvariantCultureIgnoreCase)));
        _ = Assert.Single(logExec.Messages.Where(w => w.Contains("New Id=", StringComparison.InvariantCultureIgnoreCase)));
    }

    private void AddDummyDescriptionAndSpecifiersAndMappingsAndWrongOutcome()
    {
        _apiMdEn.description = "some-description";
        _apiMdEn.specifiers = new[] { new desc_specifiersSpecifier { name = "specifier1", type = "integer" } };
        _apiMdEn.mappings = new[] { new mappingsMapping { product_id = 5, product_ids = "5", sport_id = "sr:sport:21", market_id = "61" } };
        _exampleVariantMarketDescriptionDtoEn = new MarketDescriptionDto(_apiMdEn);

        _apiMdHu.specifiers = new[] { new desc_specifiersSpecifier { name = "specifier2", type = "integer" } };
        _apiMdHu.outcomes[0].id += "test";
        _exampleVariantMarketDescriptionDtoHu = new MarketDescriptionDto(_apiMdHu);
    }

    private async Task<MarketDescriptionCacheItem> LoadDefaultExampleEn()
    {
        _ = await _variantMarketDescriptionCache.CacheAddDtoAsync(Urn.Parse($"sr:markets:{_exampleVariantMarketDescriptionDtoEn.Id}"), _exampleVariantMarketDescriptionDtoEn, _cultures[0], DtoType.MarketDescription, null);

        var fetchedVariant = $"{_exampleVariantMarketDescriptionDtoEn.Id}_{_exampleVariantMarketDescriptionDtoEn.Variant}_{_cultures[0].TwoLetterISOLanguageName}";
        _variantMarketDescriptionCache.FetchedVariants[fetchedVariant] = DateTime.Now;

        return await _variantMarketDescriptionCache.GetMarketInternalAsync((int)_exampleVariantMarketDescriptionDtoEn.Id, _exampleVariantMarketDescriptionDtoEn.Variant, new[] { _cultures[0] }).ConfigureAwait(false);
    }
}
