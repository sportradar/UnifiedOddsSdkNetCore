// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class ProfileCacheBaseTests : ProfileCacheSetup
{
    public ProfileCacheBaseTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public void InitialCallsDone()
    {
        Assert.NotNull(_profileCache);
        Assert.Empty(_profileMemoryCache.GetKeys());
        Assert.NotNull(_dataRouterManager);
        Assert.NotNull(_cacheManager);
        Assert.NotNull(_sportEventCacheMock.Object);
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsAssignableFrom<IProfileCache>(_profileCache);
        _ = Assert.IsAssignableFrom<ISdkCache>(_profileCache);
        _ = Assert.IsAssignableFrom<IHealthStatusProvider>(_profileCache);
    }

    [Fact]
    public void ImplementationRegisteredToConsumeDtos()
    {
        Assert.Equal(13, _profileCache.RegisteredDtoTypes.Count());
        Assert.Contains(DtoType.Fixture, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.MatchTimeline, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.Competitor, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.CompetitorProfile, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.SimpleTeamProfile, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.PlayerProfile, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.SportEventSummary, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.SportEventSummaryList, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.TournamentInfo, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.RaceSummary, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.MatchSummary, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.TournamentInfoList, _profileCache.RegisteredDtoTypes);
        Assert.Contains(DtoType.TournamentSeasons, _profileCache.RegisteredDtoTypes);
    }

    [Fact]
    public void ConstructorWhenNullCacheStoreThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new ProfileCache(null, _dataRouterManager, _cacheManager, _sportEventCacheMock.Object, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullDataRouterManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new ProfileCache(_profileMemoryCache, null, _cacheManager, _sportEventCacheMock.Object, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLCacheManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new ProfileCache(_profileMemoryCache, _dataRouterManager, null, _sportEventCacheMock.Object, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullSportEventCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new ProfileCache(_profileMemoryCache, _dataRouterManager, _cacheManager, null, _testLoggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLoggerFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new ProfileCache(_profileMemoryCache, _dataRouterManager, _cacheManager, _sportEventCacheMock.Object, null));
    }

    [Fact]
    public void Disposed()
    {
        _profileCache.Dispose();

        Assert.NotNull(_profileCache);
        Assert.True(_profileCache.IsDisposed());
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Test designed for this")]
    public void DisposingTwiceDoesNotThrow()
    {
        _profileCache.Dispose();
        _profileCache.Dispose();

        Assert.NotNull(_profileCache);
        Assert.True(_profileCache.IsDisposed());
    }

    [Fact]
    public void DisposingWhenMultipleParallelCallsThenDoesNotThrow()
    {
        Parallel.For(1, 10, (_) => _profileCache.Dispose());

        Assert.NotNull(_profileCache);
        Assert.True(_profileCache.IsDisposed());
    }

    [Fact]
    public void CacheDeleteItemWhenCacheEmptyAndRemovePlayerWithTypeAllThenNothingIsRemoved()
    {
        _profileCache.CacheDeleteItem(CreatePlayerUrn(1), CacheItemType.All);

        Assert.Empty(_profileMemoryCache.GetKeys());
    }

    [Fact]
    public void CacheDeleteItemWhenCacheEmptyAndRemovePlayerWithTypePlayerThenNothingIsRemoved()
    {
        _profileCache.CacheDeleteItem(CreatePlayerUrn(1), CacheItemType.Player);

        Assert.Empty(_profileMemoryCache.GetKeys());
    }

    [Fact]
    public void CacheDeleteItemWhenCacheEmptyAndRemovePlayerWithTypeCompetitorThenNothingIsRemoved()
    {
        _profileCache.CacheDeleteItem(CreatePlayerUrn(1), CacheItemType.Competitor);

        Assert.Empty(_profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWhenRemovePlayerWithTypeAllThenIsRemoved()
    {
        var playerId = CreatePlayerUrn(1);
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        Assert.Contains(playerId.ToString(), _profileMemoryCache.GetKeys());

        _profileCache.CacheDeleteItem(playerId, CacheItemType.All);

        Assert.DoesNotContain(playerId.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWhenRemovePlayerWithTypePlayerThenIsRemoved()
    {
        var playerId = CreatePlayerUrn(1);
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        Assert.Contains(playerId.ToString(), _profileMemoryCache.GetKeys());

        _profileCache.CacheDeleteItem(playerId, CacheItemType.Player);

        Assert.DoesNotContain(playerId.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWhenRemovePlayerWithTypeCompetitorThenIsRemoved()
    {
        var playerId = CreatePlayerUrn(1);
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);
        Assert.Contains(playerId.ToString(), _profileMemoryCache.GetKeys());

        _profileCache.CacheDeleteItem(playerId, CacheItemType.Competitor);

        Assert.DoesNotContain(playerId.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWhenRemoveCompetitorWithTypeAllThenIsRemoved()
    {
        var competitorId = CreateCompetitorUrn(1);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        Assert.Contains(competitorId.ToString(), _profileMemoryCache.GetKeys());

        _profileCache.CacheDeleteItem(competitorId, CacheItemType.All);

        Assert.DoesNotContain(competitorId.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWhenRemoveCompetitorWithTypePlayerThenIsRemoved()
    {
        var competitorId = CreateCompetitorUrn(1);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        Assert.Contains(competitorId.ToString(), _profileMemoryCache.GetKeys());

        _profileCache.CacheDeleteItem(competitorId, CacheItemType.Player);

        Assert.DoesNotContain(competitorId.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWhenRemoveCompetitorWithTypeCompetitorThenIsRemoved()
    {
        var competitorId = CreateCompetitorUrn(1);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        Assert.Contains(competitorId.ToString(), _profileMemoryCache.GetKeys());

        _profileCache.CacheDeleteItem(competitorId, CacheItemType.Competitor);

        Assert.DoesNotContain(competitorId.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task CacheDeleteItemWhenRemoveExistingCompetitorWithUnsupportedTypeThenIsNotRemoved()
    {
        var competitorId = CreateCompetitorUrn(1);
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);
        Assert.Contains(competitorId.ToString(), _profileMemoryCache.GetKeys());

        _profileCache.CacheDeleteItem(competitorId, EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.Player, CacheItemType.Competitor));

        Assert.Contains(competitorId.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public void CacheHasItemWhenCacheEmptyAndUseTypeAllThenNotFound()
    {
        var found = _profileCache.CacheHasItem(CreatePlayerUrn(1), CacheItemType.All);

        Assert.False(found);
    }

    [Fact]
    public void CacheHasItemWhenCacheEmptyAndUseTypePlayerThenNotFound()
    {
        var found = _profileCache.CacheHasItem(CreatePlayerUrn(1), CacheItemType.Player);

        Assert.False(found);
    }

    [Fact]
    public void CacheHasItemWhenCacheEmptyAndUseTypeCompetitorThenNotFound()
    {
        var found = _profileCache.CacheHasItem(CreatePlayerUrn(1), CacheItemType.Competitor);

        Assert.False(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasPlayerAndUseTypeAllThenFound()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var found = _profileCache.CacheHasItem(CreatePlayerUrn(1), CacheItemType.All);

        Assert.True(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasPlayerAndUseTypePlayerThenFound()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var found = _profileCache.CacheHasItem(CreatePlayerUrn(1), CacheItemType.Player);

        Assert.True(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasPlayerAndUseTypeCompetitorThenFound()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var found = _profileCache.CacheHasItem(CreatePlayerUrn(1), CacheItemType.Competitor);

        Assert.True(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasPlayerAndUseTypeUnsupportedThenNotFound()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var found = _profileCache.CacheHasItem(CreatePlayerUrn(1), EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.Player, CacheItemType.Competitor));

        Assert.False(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasCompetitorAndUseTypeAllThenFound()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var found = _profileCache.CacheHasItem(CreateCompetitorUrn(1), CacheItemType.All);

        Assert.True(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasCompetitorAndUseTypePlayerThenFound()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var found = _profileCache.CacheHasItem(CreateCompetitorUrn(1), CacheItemType.Player);

        Assert.True(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasCompetitorAndUseTypeCompetitorThenFound()
    {
        await _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), FilterLanguages(), true);

        var found = _profileCache.CacheHasItem(CreateCompetitorUrn(1), CacheItemType.Competitor);

        Assert.True(found);
    }

    [Fact]
    public async Task CacheHasItemWhenCacheHasCompetitorAndUseTypeUnsupportedThenNotFound()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(_defaultMatchId, _cultures[0], null);

        var found = _profileCache.CacheHasItem(CreateCompetitorUrn(1), EnumUtilities.GetRandomCacheItemTypeExcludingSpecified(CacheItemType.All, CacheItemType.Player, CacheItemType.Competitor));

        Assert.False(found);
    }

    [Fact]
    public async Task ExportWhenCacheEmptyThenReturnEmpty()
    {
        var exported = await _profileCache.ExportAsync();

        Assert.Empty(exported);
    }

    [Fact]
    public async Task ExportWhenCachePopulatedThenExportAll()
    {
        await PopulateCache();

        var exported = (await _profileCache.ExportAsync()).ToList();

        Assert.NotEmpty(exported);
        Assert.Equal(_profileMemoryCache.Count(), exported.Count);
    }

    [Fact]
    public async Task ImportWhenCacheEmptyThenReturnEmpty()
    {
        var exported = await _profileCache.ExportAsync();

        await _profileCache.ImportAsync(exported);

        Assert.Empty(_profileMemoryCache.GetKeys());
    }

    [Fact]
    public async Task ImportWhenCachePopulatedThenImportedDataIsMerged()
    {
        await PopulateCache();
        var exported = (await _profileCache.ExportAsync()).ToList();

        await _profileCache.ImportAsync(exported);

        Assert.Equal(exported.Count, _profileMemoryCache.GetKeys().Count);
    }

    [Fact(Skip = "Does not work always - fails in pipeline")]
    public async Task ImportWhenCachePopulatedAndSomeRemovedThenImportedDataIsMerged()
    {
        await PopulateCache();
        var exported = (await _profileCache.ExportAsync()).ToList();
        for (var i = 0; i < 10; i++)
        {
            var randomIndex = Random.Shared.Next(_profileMemoryCache.Count());
            _profileCache.CacheDeleteItem(_profileMemoryCache.GetKeys().ElementAt(randomIndex), CacheItemType.All);
        }
        Assert.Equal(exported.Count - 10, _profileMemoryCache.GetKeys().Count);

        await _profileCache.ImportAsync(exported);

        Assert.Equal(exported.Count, _profileMemoryCache.GetKeys().Count);
    }

    [Fact]
    public async Task ImportWhenCachePopulatedAndCompetitorRemovedThenDeletedItemIsAdded()
    {
        await PopulateCache();
        var exported = (await _profileCache.ExportAsync()).ToList();
        var cacheItem = (CompetitorCacheItem)_profileMemoryCache.GetValues().First(f => f.GetType() == typeof(CompetitorCacheItem));
        _profileCache.CacheDeleteItem(cacheItem.Id, CacheItemType.All);
        Assert.Equal(exported.Count - 1, _profileMemoryCache.GetKeys().Count);

        await _profileCache.ImportAsync(exported);

        Assert.Equal(exported.Count, _profileMemoryCache.GetKeys().Count);
    }

    [Fact(Skip = "Does not work always - fails in pipeline")]
    public async Task ImportWhenCachePopulatedAndTeamCompetitorRemovedThenDeletedItemIsAdded()
    {
        await PopulateCache();
        var exported = (await _profileCache.ExportAsync()).ToList();
        var cacheItem = (TeamCompetitorCacheItem)_profileMemoryCache.GetValues().First(f => f.GetType() == typeof(TeamCompetitorCacheItem));
        _profileCache.CacheDeleteItem(cacheItem.Id, CacheItemType.All);
        Assert.Equal(exported.Count - 1, _profileMemoryCache.GetKeys().Count);

        await _profileCache.ImportAsync(exported);

        await TestExecutionHelper.WaitToCompleteAsync(() => exported.Count == _profileMemoryCache.GetKeys().Count, 100, 5000);

        Assert.Equal(exported.Count, _profileMemoryCache.GetKeys().Count);
    }

    [Fact]
    public async Task ImportWhenCachePopulatedAndPlayerProfileRemovedThenDeletedItemIsAdded()
    {
        await PopulateCache();
        var exported = (await _profileCache.ExportAsync()).ToList();
        var cacheItem = (PlayerProfileCacheItem)_profileMemoryCache.GetValues().First(f => f.GetType() == typeof(PlayerProfileCacheItem));
        _profileCache.CacheDeleteItem(cacheItem.Id, CacheItemType.All);
        Assert.Equal(exported.Count - 1, _profileMemoryCache.GetKeys().Count);

        await _profileCache.ImportAsync(exported);

        Assert.Equal(exported.Count, _profileMemoryCache.Count());
    }

    [Fact]
    public void CacheStatusWhenCacheEmptyThenReturnEmpty()
    {
        var status = _profileCache.CacheStatus();

        Assert.Equal(3, status.Count);
        Assert.True(status.All(a => a.Value == 0));
    }

    [Fact]
    public async Task CacheStatusWhenCachePopulatedThenReturnValues()
    {
        await PopulateCache();

        var status = _profileCache.CacheStatus();

        Assert.NotEmpty(status);
        Assert.Equal(2, status.GetValueOrDefault("TeamCompetitorCacheItem"));
        Assert.Equal(3, status.GetValueOrDefault("CompetitorCacheItem"));
        Assert.Equal(110, status.GetValueOrDefault("PlayerProfileCacheItem"));
    }

    [Fact]
    public async Task HealthCheckWhenCacheEmptyThenHealthy()
    {
        var healthCheck = await _profileCache.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, healthCheck.Status);
    }

    [Fact]
    public async Task HealthCheckWhenCachePopulatedThenHealthy()
    {
        await PopulateCache();

        var healthCheck = await _profileCache.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Equal(HealthStatus.Healthy, healthCheck.Status);
    }

    [Fact]
    public async Task HealthCheckWhenCachePopulatedThenContainsItemCount()
    {
        await PopulateCache();

        var healthCheck = await _profileCache.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        Assert.Contains("115 items", healthCheck.Description);
    }
}
