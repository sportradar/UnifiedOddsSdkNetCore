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

public class CacheStoreTests
{
    private const string CacheStoreName = "TestCache";
    private readonly CacheStore<string> _cacheStoreString;
    private readonly CacheStore<Urn> _cacheStoreUrn;
    private readonly XUnitLogger _testLogger;

    public CacheStoreTests(ITestOutputHelper outcomeHelper)
    {
        _testLogger = new XUnitLogger(typeof(Cache), outcomeHelper);
        IMemoryCache memoryCacheString = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true });
        IMemoryCache memoryCacheUrn = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true });

        _cacheStoreString = new CacheStore<string>(CacheStoreName, memoryCacheString, _testLogger, null, TimeSpan.FromHours(1), 20);
        _cacheStoreUrn = new CacheStore<Urn>(CacheStoreName, memoryCacheUrn, _testLogger, null, TimeSpan.FromHours(1), 20);
    }

    [Fact]
    public void CacheStoreInitialized()
    {
        Assert.NotNull(_cacheStoreString);
        VerifyCacheStore(0, (string)null, null);
    }

    [Fact]
    public void CacheStoreImplementsDisposable()
    {
        Assert.IsAssignableFrom<IDisposable>(_cacheStoreString);
    }

    [Fact]
    public void CacheStoreWhenDisposedThenCanNotAdd()
    {
        _cacheStoreString.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _cacheStoreString.Add("key", new CacheItem(Urn.Parse("sr:player:1"), "name", CultureInfo.CurrentCulture)));
    }

    [Fact]
    public void CacheStoreWhenDisposedThenMemoryCacheObjectIsDisposed()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true });
        var cacheStore = new CacheStore<string>(CacheStoreName, memoryCache, _testLogger, null, TimeSpan.FromHours(1), 20);

        cacheStore.Dispose();

        Assert.Throws<ObjectDisposedException>(() => memoryCache.Set("key", new CacheItem(Urn.Parse("sr:player:1"), "name", CultureInfo.CurrentCulture)));
    }

    [Fact]
    public void AddingCacheItemWithoutKeyIsIgnored()
    {
        var myCacheItem = GenerateCacheItem(1);
        _cacheStoreUrn.Add(null, myCacheItem);
        VerifyCacheStore(0, (Urn)null, null);
    }

    [Fact]
    public void AddingCacheItemWithoutValueIsIgnored()
    {
        var myCacheItem = GenerateCacheItem(1);
        _cacheStoreUrn.Add(myCacheItem.Id, null);
        VerifyCacheStore(0, (Urn)null, null);
    }

    [Fact]
    public void GetNonExistingCacheItemsReturnsNull()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = _cacheStoreUrn.Get(myCacheItem.Id);
        Assert.Null(returnedCacheItem);
        VerifyCacheStore(0, (Urn)null, null);
    }

    [Fact]
    public void MemoryCacheWithoutExpiration()
    {
        IMemoryCache memoryCacheUrn = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true });
        var cacheStore = new CacheStore<Urn>(CacheStoreName, memoryCacheUrn, _testLogger, null, TimeSpan.Zero, 20);
        Assert.NotNull(cacheStore);

        var myCacheItem = GenerateCacheItem(1);
        cacheStore.Add(myCacheItem.Id, myCacheItem);

        var storeKeys = cacheStore.GetKeys();
        _ = Assert.Single(storeKeys);
        Assert.Equal(myCacheItem.Id, storeKeys.First());

        var storeValues = cacheStore.GetValues();
        _ = Assert.Single(storeValues);
        Assert.Equal(myCacheItem, storeValues.First());

        Assert.Equal(1, cacheStore.Count());
        Assert.Equal(1, cacheStore.Size());
    }

    [Fact]
    public void MemoryCacheWithoutTrackStatistics()
    {
        IMemoryCache memoryCacheUrn = new MemoryCache(new MemoryCacheOptions { TrackStatistics = false });
        var cacheStore = new CacheStore<Urn>(CacheStoreName, memoryCacheUrn, _testLogger, null, TimeSpan.FromHours(1), 20);
        Assert.NotNull(cacheStore);

        var myCacheItem = GenerateCacheItem(1);
        cacheStore.Add(myCacheItem.Id, myCacheItem);

        var storeKeys = cacheStore.GetKeys();
        _ = Assert.Single(storeKeys);
        Assert.Equal(myCacheItem.Id, storeKeys.First());

        var storeValues = cacheStore.GetValues();
        _ = Assert.Single(storeValues);
        Assert.Equal(myCacheItem, storeValues.First());

        Assert.Equal(1, cacheStore.Count());
        Assert.Equal(1, cacheStore.Size());
    }

    [Fact]
    public async Task MemoryCacheWithSmallSlidingExpirationEvictsCacheItem()
    {
        var memoryCacheUrn = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true, ExpirationScanFrequency = TimeSpan.FromMilliseconds(100) });
        var cacheStore = new CacheStore<Urn>(CacheStoreName, memoryCacheUrn, _testLogger, null, TimeSpan.FromMilliseconds(100), 20);
        Assert.NotNull(cacheStore);

        var myCacheItem = GenerateCacheItem(1);
        cacheStore.Add(myCacheItem.Id, myCacheItem);

        var getSuccess = memoryCacheUrn.TryGetValue(myCacheItem.Id, out var expiredCacheItem);
        Assert.True(getSuccess);
        Assert.NotNull(expiredCacheItem);
        var storeKeys = cacheStore.GetKeys();
        var storeValues = cacheStore.GetValues();
        _ = Assert.Single(storeKeys);
        Assert.Equal(myCacheItem.Id, storeKeys.First());
        _ = Assert.Single(storeValues);
        Assert.Equal(myCacheItem, storeValues.First());
        Assert.Equal(1, cacheStore.Count());
        Assert.Equal(1, cacheStore.Size());

        await Task.Delay(150);
        getSuccess = memoryCacheUrn.TryGetValue(myCacheItem.Id, out expiredCacheItem);
        Assert.False(getSuccess);
        Assert.Null(expiredCacheItem);

        _ = TestExecutionHelper.WaitToComplete(() =>
                                               {
                                                   storeKeys = cacheStore.GetKeys();
                                                   Assert.Empty(storeKeys);
                                                   return true;
                                               });

        storeKeys = cacheStore.GetKeys();
        storeValues = cacheStore.GetValues();
        Assert.Empty(storeKeys);
        Assert.Empty(storeValues);
        Assert.Equal(0, cacheStore.Count());
        Assert.Equal(0, cacheStore.Size());
    }

    // [Fact(Skip = "Sometimes fails in pipeline")]
    // public async Task MemoryCacheWithSmallSlidingExpirationProlongsOnGetItem()
    // {
    //     IMemoryCache memoryCacheUrn = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true, ExpirationScanFrequency = TimeSpan.FromMilliseconds(100) });
    //     var cacheStore = new CacheStore<Urn>(CacheStoreName, memoryCacheUrn, _testLogger, null, TimeSpan.FromMilliseconds(500));
    //     Assert.NotNull(cacheStore);
    //
    //     var myCacheItem = GenerateCacheItem(1);
    //     cacheStore.Add(myCacheItem.Id, myCacheItem);
    //     var getSuccess = memoryCacheUrn.TryGetValue(myCacheItem.Id, out var expiredCacheItem);
    //     Assert.True(getSuccess);
    //     Assert.NotNull(expiredCacheItem);
    //     var storeKeys = cacheStore.GetKeys();
    //     var storeValues = cacheStore.GetValues();
    //     _ = Assert.Single(storeKeys);
    //     Assert.Equal(myCacheItem.Id, storeKeys.First());
    //     _ = Assert.Single(storeValues);
    //     Assert.Equal(myCacheItem, storeValues.First());
    //     Assert.Equal(1, cacheStore.Count());
    //     Assert.Equal(1, cacheStore.Size());
    //
    //     var i = 0;
    //     while (i < 5)
    //     {
    //         i++;
    //         _outcomeHelper.WriteLine($"Iteration {i}");
    //         await Task.Delay(150);
    //         var returnedCacheItem = cacheStore.Get(myCacheItem.Id);
    //         Assert.NotNull(returnedCacheItem);
    //         storeKeys = cacheStore.GetKeys();
    //         storeValues = cacheStore.GetValues();
    //         _ = Assert.Single(storeKeys);
    //         Assert.Equal(myCacheItem.Id, storeKeys.First());
    //         _ = Assert.Single(storeValues);
    //         Assert.Equal(myCacheItem, storeValues.First());
    //         Assert.Equal(1, cacheStore.Count());
    //         Assert.Equal(1, cacheStore.Size());
    //     }
    //
    //     await Task.Delay(700);
    //     getSuccess = memoryCacheUrn.TryGetValue(myCacheItem.Id, out expiredCacheItem);
    //     Assert.False(getSuccess);
    //     Assert.Null(expiredCacheItem);
    //     _ = TestExecutionHelper.WaitToComplete(() =>
    //     {
    //         storeKeys = cacheStore.GetKeys();
    //         Assert.Empty(storeKeys);
    //         return true;
    //     });
    //
    //     storeKeys = cacheStore.GetKeys();
    //     storeValues = cacheStore.GetValues();
    //     Assert.Empty(storeKeys);
    //     Assert.Empty(storeValues);
    //     Assert.Equal(0, cacheStore.Count());
    //     Assert.Equal(0, cacheStore.Size());
    // }

    [Fact]
    public void CacheStoreAddItem()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = AddCacheItem(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id.ToString(), myCacheItem);
        VerifyCacheItem(myCacheItem.Id.ToString(), returnedCacheItem.Name.Values.First());
    }

    [Fact]
    public void CacheStoreAddItemMultipleTimesSameItem()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = AddCacheItem(100, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id.ToString(), returnedCacheItem);
        VerifyCacheItem(myCacheItem.Id.ToString(), returnedCacheItem.Name.Values.First());
    }

    [Fact]
    public void CacheStoreAddMultipleDifferentItems()
    {
        for (var i = 0; i < 100; i++)
        {
            var myCacheItem = GenerateCacheItem(i + 1, $"Sport Entity Name {i + 1}");
            var returnedCacheItem = AddCacheItem(1, myCacheItem);
            VerifyCacheStore(i + 1, (string)null, null);
            VerifyCacheItem(myCacheItem.Id.ToString(), returnedCacheItem.Name.Values.First());
        }
    }

    [Fact]
    public void CacheStoreAddItemMultipleTimesParallel()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var returnedCacheItem = AddCacheItem(1, myCacheItem);
        _ = Parallel.For(0, 100, (_, _) => returnedCacheItem = AddCacheItem(1, myCacheItem));
        VerifyCacheStore(1, myCacheItem.Id.ToString(), returnedCacheItem);
    }

    [Fact]
    public void CacheStoreRemove()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var returnedCacheItem = AddCacheItem(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id.ToString(), returnedCacheItem);

        _cacheStoreString.Remove(myCacheItem.Id.ToString());
        VerifyCacheStore(0, (string)null, null);
    }

    [Fact]
    public void CacheStoreWhenRemoveThenLogEvictionMessage()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        _ = AddCacheItem(1, myCacheItem);

        _cacheStoreString.Remove(myCacheItem.Id.ToString());

        TestExecutionHelper.WaitToComplete(() => !_testLogger.Messages.IsEmpty);

        _ = Assert.Single(_testLogger.Messages);
        _ = Assert.Single(_testLogger.Messages.Where(w => w.Contains("evicted cache item", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void CacheStoreUrnAddItem()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = AddCacheItemUrn(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id, myCacheItem);
        VerifyCacheItem(myCacheItem.Id, returnedCacheItem.Name.Values.First());
    }

    [Fact]
    public void CacheStoreUrnRemove()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var returnedCacheItem = AddCacheItemUrn(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id, returnedCacheItem);

        _cacheStoreUrn.Remove(myCacheItem.Id);
        VerifyCacheStore(0, (Urn)null, null);
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
            _cacheStoreString.Add(cacheItem.Id.ToString(), cacheItem);
        }

        return cacheItem;
    }

    private CacheItem AddCacheItemUrn(int numberOfTimes = 1, CacheItem cacheItem = null)
    {
        cacheItem ??= GenerateCacheItem(1);
        for (var i = 0; i < numberOfTimes; i++)
        {
            _cacheStoreUrn.Add(cacheItem.Id, cacheItem);
        }

        return cacheItem;
    }

    private void VerifyCacheItem(string id, string name)
    {
        var returnedCacheItem = (CacheItem)_cacheStoreString.Get(id);
        Assert.Equal(id, returnedCacheItem.Id.ToString());
        Assert.Equal(name, returnedCacheItem.Name.Values.First());
    }

    private void VerifyCacheItem(Urn id, string name)
    {
        var returnedCacheItem = (CacheItem)_cacheStoreUrn.Get(id);
        Assert.Equal(id, returnedCacheItem.Id);
        Assert.Equal(name, returnedCacheItem.Name.Values.First());
    }

    private void VerifyCacheStore(int numberOfItems, string firstKey, CacheItem firstValue)
    {
        var storeKeys = _cacheStoreString.GetKeys();
        Assert.Equal(numberOfItems, storeKeys.Count);
        if (firstKey != null)
        {
            Assert.Equal(firstKey, storeKeys.First());
            Assert.True(_cacheStoreString.Contains(firstKey));
        }

        var storeValues = _cacheStoreString.GetValues();
        Assert.Equal(numberOfItems, storeValues.Count);
        if (firstValue != null)
        {
            Assert.Equal(firstValue, storeValues.First());
        }

        Assert.Equal(numberOfItems, _cacheStoreString.Count());
        Assert.True(numberOfItems == 0 ? _cacheStoreString.Size() == 0 : _cacheStoreString.Size() > 0);
    }

    private void VerifyCacheStore(int numberOfItems, Urn firstKey, CacheItem firstValue)
    {
        var storeKeys = _cacheStoreUrn.GetKeys();
        Assert.Equal(numberOfItems, storeKeys.Count);
        if (firstKey != null)
        {
            Assert.Equal(firstKey, storeKeys.First());
            Assert.True(_cacheStoreUrn.Contains(firstKey));
        }

        var storeValues = _cacheStoreUrn.GetValues();
        Assert.Equal(numberOfItems, storeValues.Count);
        if (firstValue != null)
        {
            Assert.Equal(firstValue, storeValues.First());
        }

        Assert.Equal(numberOfItems, _cacheStoreUrn.Count());
        Assert.True(numberOfItems == 0 ? _cacheStoreUrn.Size() == 0 : _cacheStoreUrn.Size() > 0);
    }
}
