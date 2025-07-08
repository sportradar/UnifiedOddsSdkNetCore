// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Managers;

public class EventChangeManagerTests
{
    private readonly EventChangeManager _eventChangeManager;
    private readonly ISportEventCache _sportEventCache;
    private readonly ISportDataProvider _sportDataProvider;
    public EventChangeManagerTests(ITestOutputHelper outputHelper)
    {
        var testSportDataFactory = new TestSportEntityFactoryBuilder(outputHelper, TestData.Cultures);
        _sportDataProvider = new SportDataProvider(testSportDataFactory.SportEntityFactory,
                                                   testSportDataFactory.SportEventCache,
                                                   testSportDataFactory.EventStatusCache,
                                                   testSportDataFactory.ProfileCache,
                                                   testSportDataFactory.SportDataCache,
                                                   testSportDataFactory.Cultures,
                                                   ExceptionHandlingStrategy.Throw,
                                                   testSportDataFactory.CacheManager,
                                                   testSportDataFactory.MatchStatusCache,
                                                   testSportDataFactory.DataRouterManager);
        _sportEventCache = testSportDataFactory.SportEventCache;

        _eventChangeManager = new EventChangeManager(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), _sportDataProvider, _sportEventCache, TestConfiguration.GetConfig());
    }

    [Fact]
    public void ImplementingCorrectInterface()
    {
        _ = Assert.IsAssignableFrom<IEventChangeManager>(_eventChangeManager);
    }

    [Fact]
    public void ConstructorWhenZeroFixtureChangeIntervalThenIntervalIs1H()
    {
        var eventChangeManager = new EventChangeManager(TimeSpan.Zero, TimeSpan.FromMinutes(1), _sportDataProvider, _sportEventCache, TestConfiguration.GetConfig());

        Assert.Equal(TimeSpan.FromHours(1), eventChangeManager.FixtureChangeInterval);
    }

    [Fact]
    public void ConstructorWhenZeroResultChangeIntervalThenIntervalIs1H()
    {
        var eventChangeManager = new EventChangeManager(TimeSpan.FromMinutes(1), TimeSpan.Zero, _sportDataProvider, _sportEventCache, TestConfiguration.GetConfig());

        Assert.Equal(TimeSpan.FromHours(1), eventChangeManager.ResultChangeInterval);
    }

    [Fact]
    public void ConstructorWhenNullSportDataProviderThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new EventChangeManager(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), null, _sportEventCache, TestConfiguration.GetConfig()));
    }

    [Fact]
    public void ConstructorWhenNullSportEventCacheThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new EventChangeManager(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), _sportDataProvider, null, TestConfiguration.GetConfig()));
    }

    [Fact]
    public void ConstructorWhenNullUofConfigurationThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new EventChangeManager(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), _sportDataProvider, _sportEventCache, null));
    }

    [Fact]
    public void EventChangeManagerImplementsDisposable()
    {
        Assert.IsAssignableFrom<IDisposable>(_eventChangeManager);
    }

    [Fact]
    public void EventChangeManagerWhenDisposedThenCanNotAdd()
    {
        _eventChangeManager.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _eventChangeManager.Start());
    }

    [Fact]
    public void EventChangeManagerWhenDisposedThenGetEventCacheItemDoesNotThrow()
    {
        _eventChangeManager.Dispose();

        var ci = _sportEventCache.GetEventCacheItem(Urn.Parse("sr:match:1"));

        Assert.Null(ci);
    }

    [Fact]
    public async Task EventChangeManagerWhenDisposedThenGetActiveTournamentThrows()
    {
        _eventChangeManager.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => _sportEventCache.GetActiveTournamentsAsync(TestData.Culture));
    }
}
