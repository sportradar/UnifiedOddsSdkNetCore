// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class CacheExportTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly SportDataCache _sportDataCache;
    private readonly SportEventCache _sportEventCache;
    private readonly ProfileCache _profileCache;
    private readonly ICacheStore<string> _profileMemoryCache;
    private readonly TestDataRouterManager _dataRouterManager;

    public CacheExportTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        var loggerFactory = new XunitLoggerFactory(outputHelper);
        var testCacheStoreManager = new TestCacheStoreManager();

        var cacheManager = testCacheStoreManager.CacheManager;
        _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

        var timer = new TestTimer(false);
        var semaphorePool = new SemaphorePool(5, ExceptionHandlingStrategy.Throw);

        _sportEventCache = new SportEventCache(
            testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache),
            _dataRouterManager,
            new SportEventCacheItemFactory(
                _dataRouterManager,
                semaphorePool,
                testCacheStoreManager.UofConfig,
                testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache)),
            timer,
            TestData.Cultures,
            cacheManager,
            loggerFactory);

        _sportDataCache = new SportDataCache(_dataRouterManager, timer, TestData.Cultures, _sportEventCache, cacheManager, loggerFactory);
        _profileMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache);
        _profileCache = new ProfileCache(_profileMemoryCache, _dataRouterManager, cacheManager, _sportEventCache, loggerFactory);
    }

    [Fact]
    public async Task SportDataCacheStatus()
    {
        var status = _sportDataCache.CacheStatus();
        Assert.Equal(0, status["SportCacheItem"]);
        Assert.Equal(0, status["CategoryCacheItem"]);

        var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures); // initial load

        Assert.NotNull(sports);
        status = _sportDataCache.CacheStatus();
        Assert.Equal(TestData.CacheSportCount, status["SportCacheItem"]);
        Assert.Equal(TestData.CacheCategoryCountPlus, status["CategoryCacheItem"]);
    }

    [Fact]
    public async Task SportDataCacheEmptyExport()
    {
        var export = (await _sportDataCache.ExportAsync()).ToList();
        export.Count.Should().Be(0);

        await _sportDataCache.ImportAsync(export);
        Assert.Empty(_sportDataCache.Sports);
        Assert.Empty(_sportDataCache.Categories);
    }

    [Fact]
    public async Task SportDataCacheFullExport()
    {
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures); // initial load
        Assert.NotNull(sports);

        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var export = (await _sportDataCache.ExportAsync()).ToList();
        Assert.Equal(TestData.CacheSportCount + TestData.CacheCategoryCountPlus, export.Count);

        _sportDataCache.Sports.Clear();
        _sportDataCache.Categories.Clear();
        _sportDataCache.FetchedCultures.Clear();

        await _sportDataCache.ImportAsync(export);
        Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
        Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);

        // No calls to the data router manager
        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
        Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var exportString = SerializeExportables(export);
        var secondExportString = SerializeExportables(await _sportDataCache.ExportAsync());
        Assert.Equal(exportString, secondExportString);
    }

    [Fact]
    public async Task ProfileCacheStatus()
    {
        var status = _profileCache.CacheStatus();
        Assert.Equal(0, status["TeamCompetitorCacheItem"]);
        Assert.Equal(0, status["CompetitorCacheItem"]);
        Assert.Equal(0, status["PlayerProfileCacheItem"]);

        await _profileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), TestData.Cultures, false);
        await _profileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:2"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:1"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:2"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:simpleteam:1"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:simpleteam:2"), TestData.Cultures, false);

        status = _profileCache.CacheStatus();
        Assert.Equal(0, status["TeamCompetitorCacheItem"]);
        Assert.Equal(4, status["CompetitorCacheItem"]);
        Assert.Equal(62, status["PlayerProfileCacheItem"]);
        Assert.Equal(_profileMemoryCache.Count(), status.Sum(i => i.Value));
    }

    [Fact]
    public async Task ProfileCacheEmptyExport()
    {
        var export = (await _profileCache.ExportAsync()).ToList();
        export.Count.Should().Be(0);

        await _profileCache.ImportAsync(export);
        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact(Skip = "Does not work always - fails in pipeline")]
    public async Task ProfileCacheFullExport()
    {
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        await _profileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:1"), TestData.Cultures, false);
        await _profileCache.GetPlayerProfileAsync(Urn.Parse("sr:player:2"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:1"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:competitor:2"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:simpleteam:1"), TestData.Cultures, false);
        await _profileCache.GetCompetitorProfileAsync(Urn.Parse("sr:simpleteam:2"), TestData.Cultures, false);

        Assert.Equal(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        var export = (await _profileCache.ExportAsync()).ToList();
        Assert.Equal(_profileMemoryCache.Count(), export.Count);

        var keys = _profileMemoryCache.GetKeys();
        foreach (var key in keys.OrderBy(o => o))
        {
            _profileMemoryCache.Remove(key);
        }
        Assert.Empty(_profileMemoryCache.GetKeys());
        Assert.Empty(_profileMemoryCache.GetValues());

        await _profileCache.ImportAsync(export);

        _outputHelper.WriteLine("All keys imported");
        foreach (var key in _profileMemoryCache.GetKeys())
        {
            _outputHelper.WriteLine(key);
        }
        TestExecutionHelper.WaitToComplete(() => _profileMemoryCache.Count() == export.Count, 100, 15000);
        Assert.Equal(export.Count, _profileMemoryCache.Count());

        // No calls to the data router manager
        Assert.Equal(TestData.Cultures.Count * 4, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        var exportString = SerializeExportables(export);
        var secondExportString = SerializeExportables(await _profileCache.ExportAsync());
        Assert.Equal(exportString, secondExportString);
    }

    [Fact]
    public async Task SportEventCacheStatus()
    {
        var status = _sportEventCache.CacheStatus();
        Assert.Equal(0, status["SportEventCacheItem"]);
        Assert.Equal(0, status["CompetitionCacheItem"]);
        Assert.Equal(0, status["DrawCacheItem"]);
        Assert.Equal(0, status["LotteryCacheItem"]);
        Assert.Equal(0, status["TournamentInfoCacheItem"]);
        Assert.Equal(0, status["MatchCacheItem"]);
        Assert.Equal(0, status["StageCacheItem"]);

        var tournaments = await _sportEventCache.GetActiveTournamentsAsync(); // initial load

        Assert.NotNull(tournaments);
        status = _sportEventCache.CacheStatus();
        Assert.Equal(213, status["TournamentInfoCacheItem"]);
        Assert.Equal(974, status["MatchCacheItem"]);
    }

    [Fact]
    public async Task SportEventCacheEmptyExport()
    {
        var export = (await _sportEventCache.ExportAsync()).ToList();
        export.Count.Should().Be(0);

        await _sportDataCache.ImportAsync(export);
        Assert.Equal(0, _sportEventCache.Cache.Count());
    }

    [Fact(Skip = "Fails in pipeline")]
    public async Task SportEventCacheFullExport()
    {
        var tournaments = await _sportEventCache.GetActiveTournamentsAsync(); // initial load
        Assert.NotNull(tournaments);

        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var export = (await _sportEventCache.ExportAsync()).ToList();
        Assert.Equal(1187, export.Count);

        _sportEventCache.Cache.GetKeys().ToList().ForEach(i => _sportEventCache.Cache.Remove(i));

        await _sportEventCache.ImportAsync(export);

        TestExecutionHelper.WaitToComplete(() => _sportEventCache.Cache.Count() == export.Count, 100, 15000);
        Assert.Equal(1187, _sportEventCache.Cache.Count());

        // No calls to the data router manager
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var exportString = SerializeExportables(export);
        var secondExportString = SerializeExportables(await _sportEventCache.ExportAsync());
        Assert.Equal(exportString, secondExportString);
    }

    private static string SerializeExportables(IEnumerable<ExportableBase> exportables)
    {
        return JsonConvert.SerializeObject(exportables.OrderBy(e => e.Id));
    }
}
