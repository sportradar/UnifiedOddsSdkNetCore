// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class CacheStoreRefTests
{
    private const string CacheStoreName = "TestCache";
    private readonly CacheStore<Urn> _cacheStore;
    private readonly XUnitLogger _testLogger;

    public CacheStoreRefTests(ITestOutputHelper outcomeHelper)
    {
        _testLogger = new XUnitLogger(typeof(Cache), outcomeHelper);
        IMemoryCache memoryCacheUrn = new MemoryCache(new MemoryCacheOptions { TrackStatistics = true });

        _cacheStore = new CacheStore<Urn>(CacheStoreName, memoryCacheUrn, _testLogger, null, TimeSpan.FromHours(1), 20);
    }

    [Fact]
    public void CacheStoreInitialized()
    {
        Assert.NotNull(_cacheStore);
        VerifyCacheStore(0, null, null);
    }

    [Fact]
    public void CacheStoreImplementsDisposable()
    {
        Assert.IsType<IDisposable>(_cacheStore, false);
    }

    [Fact]
    public void AddWhenDisposedThenThrow()
    {
        var cacheItem = GenerateCacheItem(1);
        _cacheStore.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _cacheStore.Add(cacheItem.Id, cacheItem));
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
        _cacheStore.Add(null, myCacheItem);

        VerifyCacheStore(0, null, null);
    }

    [Fact]
    public void AddWithoutValueIsIgnored()
    {
        var myCacheItem = GenerateCacheItem(1);
        _cacheStore.Add(myCacheItem.Id, null);

        VerifyCacheStore(0, null, null);
    }

    [Fact]
    public void GetWhenNullKeyThenReturnNull()
    {
        var returnedCacheItem = _cacheStore.Get(null);

        Assert.Null(returnedCacheItem);
        VerifyCacheStore(0, null, null);
    }

    [Fact]
    public void GetNonExistingCacheItemsReturnsNull()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = _cacheStore.Get(myCacheItem.Id);

        Assert.Null(returnedCacheItem);
        VerifyCacheStore(0, null, null);
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

        _ = await TestExecutionHelper.WaitToCompleteAsync(() =>
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

    [Fact]
    public void AddItem()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = AddCacheItem(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id, myCacheItem);
        VerifyCacheItem(myCacheItem.Id, returnedCacheItem.Name.Values.First());
    }

    [Fact]
    public void AddItemMultipleTimesSameItem()
    {
        var myCacheItem = GenerateCacheItem(1);
        var returnedCacheItem = AddCacheItem(100, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id, returnedCacheItem);
        VerifyCacheItem(myCacheItem.Id, returnedCacheItem.Name.Values.First());
    }

    [Fact]
    public void AddMultipleDifferentItems()
    {
        for (var i = 0; i < 100; i++)
        {
            var myCacheItem = GenerateCacheItem(i + 1, $"Sport Entity Name {i + 1}");
            var returnedCacheItem = AddCacheItem(1, myCacheItem);
            VerifyCacheStore(i + 1, null, null);
            VerifyCacheItem(myCacheItem.Id, returnedCacheItem.Name.Values.First());
        }
    }

    [Fact]
    public void AddItemMultipleTimesParallel()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var returnedCacheItem = AddCacheItem(1, myCacheItem);

        _ = Parallel.For(0, 100, (_, _) => returnedCacheItem = AddCacheItem(1, myCacheItem));

        VerifyCacheStore(1, myCacheItem.Id, returnedCacheItem);
    }

    [Fact]
    public void Remove()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var returnedCacheItem = AddCacheItem(1, myCacheItem);
        VerifyCacheStore(1, myCacheItem.Id, returnedCacheItem);

        _cacheStore.Remove(myCacheItem.Id);
        VerifyCacheStore(0, null, null);
    }

    [Fact]
    public async Task RemoveThenLogEvictionMessage()
    {
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        _ = AddCacheItem(1, myCacheItem);

        _cacheStore.Remove(myCacheItem.Id);

        await TestExecutionHelper.WaitToCompleteAsync(() => !_testLogger.Messages.IsEmpty);

        _ = Assert.Single(_testLogger.Messages);
        _ = Assert.Single(_testLogger.Messages, w => w.Contains("evicted cache item", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public void GetSizeForNonMemoryCache()
    {
        var mockMemoryCache = new Mock<IMemoryCache>();
        var cacheStore = new CacheStore<Urn>(CacheStoreName, mockMemoryCache.Object, _testLogger, null, TimeSpan.FromHours(1), 20);
        var myCacheItem = GenerateCacheItem(1, "Sport Entity Name 1");
        var cacheEntry = new MyCacheEntry(myCacheItem.Id);
        mockMemoryCache.Setup(s => s.CreateEntry(myCacheItem.Id)).Returns(cacheEntry);

        cacheStore.Add(myCacheItem.Id, myCacheItem);

        var size = cacheStore.Size();
        Assert.Equal(1, size);
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
            _cacheStore.Add(cacheItem.Id, cacheItem);
        }

        return cacheItem;
    }

    private void VerifyCacheItem(Urn id, string name)
    {
        var returnedCacheItem = (CacheItem)_cacheStore.Get(id);
        Assert.Equal(id, returnedCacheItem.Id);
        Assert.Equal(name, returnedCacheItem.Name.Values.First());
    }

    private void VerifyCacheStore(int numberOfItems, Urn firstKey, CacheItem firstValue)
    {
        var storeKeys = _cacheStore.GetKeys();
        Assert.Equal(numberOfItems, storeKeys.Count);
        if (firstKey != null)
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

    private class MyCacheEntry : ICacheEntry
    {

        public MyCacheEntry(Urn key)
        {
            Key = key;
            ExpirationTokens = new List<IChangeToken>();
            PostEvictionCallbacks = new List<PostEvictionCallbackRegistration>();
        }
        public void Dispose()
        {
            //ignore
        }

        public object Key
        {
            get;
        }
        public object Value
        {
            get;
            set;
        }
        public DateTimeOffset? AbsoluteExpiration
        {
            get;
            set;
        }
        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get;
            set;
        }
        public TimeSpan? SlidingExpiration
        {
            get;
            set;
        }
        public IList<IChangeToken> ExpirationTokens
        {
            get;
        }
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks
        {
            get;
        }
        public CacheItemPriority Priority
        {
            get;
            set;
        }
        public long? Size
        {
            get;
            set;
        }
    }
}
