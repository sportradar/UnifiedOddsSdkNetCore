// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InSportEvent;

[SuppressMessage("Usage", "xUnit1031:Do not use blocking task operations in test method")]
public class StageCiTests
{
    private readonly TestDataRouterManager _dataRouterManager;
    private readonly ISemaphorePool _semaphorePool;
    private readonly ICacheStore<string> _fixtureTimestampCacheStore;
    private readonly Urn _stageId = Urn.Parse("sr:stage:123");

    public StageCiTests(ITestOutputHelper outputHelper)
    {
        var testCacheStoreManager = new TestCacheStoreManager();
        _fixtureTimestampCacheStore = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForFixtureChangeCache);
        _semaphorePool = new SemaphorePool(100, ExceptionHandlingStrategy.Throw);

        var cacheManager = testCacheStoreManager.CacheManager;
        _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);
    }

    [Fact]
    public void ConstructStageFromId()
    {
        var stageCi = new StageCacheItem(_stageId, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        Assert.NotNull(stageCi);
        Assert.Equal(_stageId, stageCi.Id);
        Assert.Null(stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public void ConstructStageFromStageDto()
    {
        var stageSummaryEndpoint = MessageFactoryRest.GetStageSummaryEndpoint(123);
        stageSummaryEndpoint.sport_event.id = _stageId.ToString();
        stageSummaryEndpoint.sport_event.stage_type = "race";

        var stageDto = new StageDto(stageSummaryEndpoint);
        var stageCi = new StageCacheItem(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        Assert.NotNull(stageCi);
        Assert.Equal(stageDto.Id, stageCi.Id);
        Assert.Equal(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public void ConstructStageFromFixtureDto()
    {
        var fixture = MessageFactoryRest.GetFixture(123);
        fixture.id = _stageId.ToString();
        fixture.stage_type = "race";

        var stageDto = new FixtureDto(fixture, DateTime.Now);
        var stageCi = new StageCacheItem(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        Assert.NotNull(stageCi);
        Assert.Equal(stageDto.Id, stageCi.Id);
        Assert.Equal(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public void ConstructStageFromExportableStage()
    {
        var fixture = MessageFactoryRest.GetFixture(123);
        fixture.id = _stageId.ToString();
        fixture.stage_type = "race";

        var stageDto = new FixtureDto(fixture, DateTime.Now);
        var stageCi = new StageCacheItem(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        Assert.NotNull(stageCi);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

        var exportedStage = (ExportableStage)stageCi.ExportAsync().GetAwaiter().GetResult();
        var newStageCi = new StageCacheItem(exportedStage, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        Assert.NotNull(newStageCi);
        Assert.Equal(stageDto.Id, newStageCi.Id);
        Assert.Equal(stageDto.StageType, newStageCi.GetStageTypeAsync().GetAwaiter().GetResult());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public void MergeStageToBlankFromId()
    {
        var stageCi = new StageCacheItem(_stageId, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        var fixture = MessageFactoryRest.GetFixture(123);
        fixture.id = _stageId.ToString();
        fixture.stage_type = "race";
        var fixtureDto = new FixtureDto(fixture, DateTime.Now);

        stageCi.MergeFixture(fixtureDto, CultureInfo.CurrentCulture, true);

        Assert.NotNull(stageCi);
        Assert.Equal(_stageId, stageCi.Id);
        Assert.Equal(StageType.Race, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public void MergeStageFromStageDtoWithoutStageTypeDoesNotRemovesIt()
    {
        var stageSummaryEndpoint = MessageFactoryRest.GetStageSummaryEndpoint(123);
        stageSummaryEndpoint.sport_event.id = _stageId.ToString();
        stageSummaryEndpoint.sport_event.stage_type = "race";
        var stageDto = new StageDto(stageSummaryEndpoint);
        var stageCi = new StageCacheItem(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        var stageSummaryEndpoint2 = MessageFactoryRest.GetStageSummaryEndpoint(123);
        stageSummaryEndpoint2.sport_event.id = _stageId.ToString();
        var stageDto2 = new StageDto(stageSummaryEndpoint2);

        stageCi.Merge(stageDto2, CultureInfo.CurrentCulture, true);

        Assert.NotNull(stageCi);
        Assert.Equal(stageDto.Id, stageCi.Id);
        Assert.Equal(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public void MergeStageFromFixtureDtoWithoutStageTypeDoesNotRemovesIt()
    {
        var fixture = MessageFactoryRest.GetFixture(123);
        fixture.id = _stageId.ToString();
        fixture.stage_type = "race";
        var stageDto = new FixtureDto(fixture, DateTime.Now);
        var stageCi = new StageCacheItem(stageDto, _dataRouterManager, _semaphorePool, CultureInfo.CurrentCulture, CultureInfo.CurrentCulture, _fixtureTimestampCacheStore);

        var fixture2 = MessageFactoryRest.GetFixture(123);
        fixture2.id = _stageId.ToString();
        var stageDto2 = new FixtureDto(fixture2, DateTime.Now);
        stageCi.Merge(stageDto2, CultureInfo.CurrentCulture, true);

        Assert.NotNull(stageCi);
        Assert.Equal(stageDto.Id, stageCi.Id);
        Assert.Equal(stageDto.StageType, stageCi.GetStageTypeAsync().GetAwaiter().GetResult());
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }
}
