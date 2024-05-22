// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Tests.Common.MockLog;
using Xunit.Abstractions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedVariable

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class TestSportEntityFactoryBuilder
{
    private readonly ITestOutputHelper _outputHelper;
    internal readonly List<CultureInfo> Cultures;
    internal readonly CacheManager CacheManager;
    internal readonly SportEventCache SportEventCache;
    internal readonly SportDataCache SportDataCache;
    internal readonly ISportEventStatusCache EventStatusCache;
    internal readonly IProfileCache ProfileCache;
    internal readonly SportEntityFactory SportEntityFactory;
    internal readonly TestDataRouterManager DataRouterManager;
    internal readonly TestDataRouterManagerCached DataRouterManagerCached;
    internal readonly ICacheStore<string> SportEventMemoryCache;
    internal readonly ICacheStore<string> ProfileMemoryCache;
    internal readonly ICacheStore<string> SportEventStatusMemoryCache;
    internal readonly ICacheStore<string> IgnoreTimelineMemoryCache;
    internal readonly ExceptionHandlingStrategy ThrowingStrategy;
    internal readonly XunitLoggerFactory LoggerFactory;

    public ICompetition Competition;
    public ITournament Tournament;
    public ISeason Season;
    public ISport Sport;
    public List<ISport> Sports;

    public TestSportEntityFactoryBuilder(ITestOutputHelper outputHelper, IReadOnlyCollection<CultureInfo> cultures = null, bool useCachedDataRouterManager = false)
        : this(outputHelper, cultures?.ToList(), useCachedDataRouterManager)
    {
    }

    public TestSportEntityFactoryBuilder(ITestOutputHelper outputHelper, List<CultureInfo> cultures = null, bool useCachedDataRouterManager = false)
    {
        _outputHelper = outputHelper;
        LoggerFactory = new XunitLoggerFactory(outputHelper);
        ThrowingStrategy = ExceptionHandlingStrategy.Throw;
        var sdkConfig = TestConfiguration.GetConfig();
        var testCacheStoreManager = new TestCacheStoreManager();
        Cultures = cultures ?? testCacheStoreManager.UofConfig.Languages;
        var timer = new TestTimer(false);
        SportEventMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache);
        ProfileMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache);
        SportEventStatusMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventStatusCache);
        IgnoreTimelineMemoryCache = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForIgnoreEventsTimelineCache);
        var fixtureTimestampCacheStore = testCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache);

        CacheManager = testCacheStoreManager.CacheManager;
        DataRouterManager = new TestDataRouterManager(CacheManager, _outputHelper);
        DataRouterManagerCached = new TestDataRouterManagerCached(CacheManager, Cultures, _outputHelper);

        var selectedDataRouterManager = useCachedDataRouterManager ? (IDataRouterManager)DataRouterManagerCached : DataRouterManager;

        var sportEventCacheItemFactory = new SportEventCacheItemFactory(selectedDataRouterManager,
            new SemaphorePool(5, ThrowingStrategy),
            sdkConfig,
            fixtureTimestampCacheStore);

        SportEventCache = new SportEventCache(SportEventMemoryCache, selectedDataRouterManager, sportEventCacheItemFactory, timer, Cultures, CacheManager, LoggerFactory);
        ProfileCache = new ProfileCache(ProfileMemoryCache, selectedDataRouterManager, CacheManager, SportEventCache, LoggerFactory);
        SportDataCache = new SportDataCache(selectedDataRouterManager, timer, Cultures, SportEventCache, CacheManager, LoggerFactory);
        var sportEventStatusCache = TestLocalizedNamedValueCache.CreateMatchStatusCache(Cultures, ThrowingStrategy);
        var namedValuesProviderMock = new Mock<INamedValuesProvider>();
        namedValuesProviderMock.Setup(args => args.MatchStatuses).Returns(sportEventStatusCache);
        EventStatusCache = new SportEventStatusCache(SportEventStatusMemoryCache, new SportEventStatusMapperFactory(), SportEventCache, CacheManager, IgnoreTimelineMemoryCache, TestConfiguration.GetConfig(), LoggerFactory);
        SportEntityFactory = new SportEntityFactory(SportDataCache, SportEventCache, EventStatusCache, sportEventStatusCache, ProfileCache, SdkInfo.SoccerSportUrns);
    }

    public async Task InitializeSportEntities()
    {
        var watch = Stopwatch.StartNew();
        if (SportEntityFactory == null)
        {
            return;
        }
        var time = watch.Elapsed.TotalMilliseconds;
        _outputHelper.WriteLine($"Init time: {time}");
        Competition = SportEntityFactory.BuildSportEvent<ICompetition>(TestData.EventMatchId, Urn.Parse("sr:sport:3"), Cultures, ThrowingStrategy);
        _outputHelper.WriteLine($"Competition time: {watch.Elapsed.TotalMilliseconds - time}");
        time = watch.Elapsed.TotalMilliseconds;
        Sport = await SportEntityFactory.BuildSportAsync(TestData.SportId, Cultures, ThrowingStrategy);
        _outputHelper.WriteLine($"Sport time: {watch.Elapsed.TotalMilliseconds - time}");
        time = watch.Elapsed.TotalMilliseconds;
        Sports = (await SportEntityFactory.BuildSportsAsync(Cultures, ThrowingStrategy)).ToList();
        _outputHelper.WriteLine($"Sports time: {watch.Elapsed.TotalMilliseconds - time}");
        time = watch.Elapsed.TotalMilliseconds;
        Tournament = SportEntityFactory.BuildSportEvent<ITournament>(TestData.TournamentId, TestData.SportId, Cultures, ThrowingStrategy);
        _outputHelper.WriteLine($"Tournament time: {watch.Elapsed.TotalMilliseconds - time}");
        time = watch.Elapsed.TotalMilliseconds;
        Season = SportEntityFactory.BuildSportEvent<ISeason>(TestData.SeasonId, TestData.SportId, Cultures, ThrowingStrategy);
        _outputHelper.WriteLine($"Season time: {watch.Elapsed.TotalMilliseconds - time}");
    }

    public async Task LoadTournamentMissingValues()
    {
        _ = await Tournament.GetScheduledTimeAsync();
        _ = await Tournament.GetScheduledEndTimeAsync();
        _ = await Tournament.GetCategoryAsync();
        _ = await Tournament.GetTournamentCoverage();
        _ = await Tournament.GetCurrentSeasonAsync();
    }

    public async Task LoadSeasonMissingValues()
    {
        _ = await Season.GetScheduledTimeAsync();
        _ = await Season.GetScheduledEndTimeAsync();
        _ = await Season.GetTournamentCoverage();
        _ = await Season.GetCurrentRoundAsync();
        _ = await Season.GetSeasonCoverageAsync();
        _ = await Season.GetScheduleAsync();
        _ = await Season.GetGroupsAsync();
        _ = await Season.GetYearAsync();
    }

    public ITournament GetNewTournament(long id = 0, long sportId = 0)
    {
        if (id > 0)
        {
            return SportEntityFactory.BuildSportEvent<ITournament>(Urn.Parse($"sr:tournament:{id}"), Urn.Parse($"sr:sport:{sportId}"), Cultures, ThrowingStrategy);
        }
        return Tournament;
    }

    public IMatch GetMatch(long id = 0, long sportId = 0)
    {
        return SportEntityFactory.BuildSportEvent<IMatch>(Urn.Parse($"sr:match:{id}"), Urn.Parse($"sr:sport:{sportId}"), Cultures, ThrowingStrategy);
    }

    public void WriteProfileMemoryKeys()
    {
        foreach (var key in ProfileMemoryCache.GetKeys())
        {
            _outputHelper.WriteLine($"ProfileMemory: {key}");
        }
    }

    public void WriteDrmMethodsCalls()
    {
        foreach (var method in DataRouterManager.RestMethodCalls)
        {
            _outputHelper.WriteLine($"Drm method: {method.Key} [{method.Value}]");
        }
    }

    public void WriteDrmUrlCalls()
    {
        foreach (var url in DataRouterManager.RestUrlCalls)
        {
            _outputHelper.WriteLine($"Drm url: {url}");
        }
    }
}
