// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class CacheStoreValueTests
{
    private const string CacheStoreName = "TestCache";
    private readonly CacheStore<long> _cacheStore;
    private readonly XUnitLogger _testLogger;

    public CacheStoreValueTests(ITestOutputHelper outcomeHelper)
    {
        _testLogger = new XUnitLogger(typeof(Cache), outcomeHelper);
        IMemoryCache memoryCacheUrn = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true });

        _cacheStore = new CacheStore<long>(CacheStoreName, memoryCacheUrn, _testLogger, null, TimeSpan.FromHours(1), 20);
    }

    [Fact]
    public void CacheStoreInitialized()
    {
        Assert.NotNull(_cacheStore);
        VerifyCacheStore(0, 0, null);
    }

    [Fact]
    public void CacheStoreImplementsDisposable()
    {
        Assert.IsType<IDisposable>(_cacheStore, false);
    }

    [Fact]
    public void CacheStoreWhenDisposedThenCanNotAdd()
    {
        var cacheItem = GenerateCacheItem(1);
        _cacheStore.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _cacheStore.Add(cacheItem.Id.Id, cacheItem));
    }

    [Fact]
    public void CacheStoreWhenDisposedThenMemoryCacheObjectIsDisposed()
    {
        var cacheItem = GenerateCacheItem(1);
        var memoryCache = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true });
        var cacheStore = new CacheStore<Urn>(CacheStoreName, memoryCache, _testLogger, null, TimeSpan.FromHours(1), 20);

        cacheStore.Dispose();

        Assert.Throws<ObjectDisposedException>(() => memoryCache.Set(cacheItem.Id, cacheItem));
    }

    [Fact]
    public void AddWithoutKeyIsIgnored()
    {
        var myCacheItem = GenerateCacheItem(1);
        _cacheStore.Add(0, myCacheItem);

        VerifyCacheStore(0, 0, null);
    }

    [Fact]
    public void AddWithoutValueIsIgnored()
    {
        var myCacheItem = GenerateCacheItem(1);
        _cacheStore.Add(myCacheItem.Id.Id, null);

        VerifyCacheStore(0, 0, null);
    }

    [Fact]
    public void GetWhenNullKeyThenReturnNull()
    {
        var returnedCacheItem = _cacheStore.Get(0);

        Assert.Null(returnedCacheItem);
        VerifyCacheStore(0, 0, null);
    }

    [Fact]
    public void GetNonExistingCacheItemsReturnsNull()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = _cacheStore.Get(myCacheItem.Id.Id);

        Assert.Null(returnedCacheItem);
        VerifyCacheStore(0, 0, null);
    }

    [Fact]
    public void AddItem()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = AddCacheItem(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id.Id, myCacheItem);
        VerifyCacheItem(myCacheItem.Id.Id, returnedCacheItem.Name.Values.First());
    }

    [Fact]
    public void AddItemMultipleTimesSameItem()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = AddCacheItem(100, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id.Id, returnedCacheItem);
        VerifyCacheItem(myCacheItem.Id.Id, returnedCacheItem.Name.Values.First());
    }

    [Fact]
    public void AddMultipleDifferentItems()
    {
        for (var i = 0; i < 100; i++)
        {
            var myCacheItem = GenerateCacheItem(i + 1, $"Sport Entity Name {i + 1}");
            var returnedCacheItem = AddCacheItem(1, myCacheItem);
            VerifyCacheStore(i + 1, 0, null);
            VerifyCacheItem(myCacheItem.Id.Id, returnedCacheItem.Name.Values.First());
        }
    }

    [Fact]
    public void AddItemMultipleTimesParallel()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var returnedCacheItem = AddCacheItem(1, myCacheItem);

        _ = Parallel.For(0, 100, (_, _) => returnedCacheItem = AddCacheItem(1, myCacheItem));

        VerifyCacheStore(1, myCacheItem.Id.Id, returnedCacheItem);
    }

    [Fact]
    public void Remove()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var returnedCacheItem = AddCacheItem(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id.Id, returnedCacheItem);

        _cacheStore.Remove(myCacheItem.Id.Id);
        VerifyCacheStore(0, 0, null);
    }

    [Fact]
    public void RemoveThenLogEvictionMessage()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        _ = AddCacheItem(1, myCacheItem);

        _cacheStore.Remove(myCacheItem.Id.Id);

        TestExecutionHelper.WaitToComplete(() => !_testLogger.Messages.IsEmpty);

        _ = Assert.Single(_testLogger.Messages);
        _ = Assert.Single(_testLogger.Messages, w => w.Contains("evicted cache item", StringComparison.InvariantCultureIgnoreCase));
    }

    private static CacheItem GenerateCacheItem(int id, string name = null)
    {
        var cacheItemName = name.IsNullOrEmpty() ? $"Item name {id}" : name;
        return new CacheItem(Urn.Parse($"sr:player:{id}"), cacheItemName, CultureInfo.CurrentCulture);
    }

    private CacheItem AddCacheItem(int numberOfTimes = 1, CacheItem cacheItem = null)
    {
        cacheItem ??= GenerateCacheItem(1);
        for (var i = 0; i < numberOfTimes; i++)
        {
            _cacheStore.Add(cacheItem.Id.Id, cacheItem);
        }

        return cacheItem;
    }

    private void VerifyCacheItem(long id, string name)
    {
        var returnedCacheItem = (CacheItem)_cacheStore.Get(id);
        Assert.Equal(id, returnedCacheItem.Id.Id);
        Assert.Equal(name, returnedCacheItem.Name.Values.First());
    }

    private void VerifyCacheStore(int numberOfItems, long firstKey, CacheItem firstValue)
    {
        var storeKeys = _cacheStore.GetKeys();
        Assert.Equal(numberOfItems, storeKeys.Count);
        if (firstKey != 0)
        {
            Assert.Equal(firstKey, storeKeys.First());
            Assert.True(_cacheStore.Contains(firstKey));
        }

        var storeValues = _cacheStore.GetValues();
        Assert.Equal(numberOfItems, storeValues.Count);
        if (firstValue != null)
        {
            Assert.Equal(firstValue, storeValues.First());
        }

        Assert.Equal(numberOfItems, _cacheStore.Count());
        Assert.True(numberOfItems == 0 ? _cacheStore.Size() == 0 : _cacheStore.Size() > 0);
    }
}
