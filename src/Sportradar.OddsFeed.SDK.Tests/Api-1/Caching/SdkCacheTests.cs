// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

public class SdkCacheTests
{
    private readonly TestSdkCache _sdkCache;
    private readonly XunitLoggerFactory _loggerFactory;
    private readonly CacheManager _cacheManager;

    public SdkCacheTests(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new XunitLoggerFactory(outputHelper);

        _cacheManager = new CacheManager();

        _sdkCache = new TestSdkCache(_cacheManager, _loggerFactory);
    }

    [Fact]
    public void ConstructorWhenAllPresentThenSucceed()
    {
        var sdkCache = new TestSdkCache(new CacheManager(), _loggerFactory);

        Assert.NotNull(sdkCache);
    }

    [Fact]
    public void ConstructorWhenAllPresentThenCacheNameIsDefined()
    {
        var sdkCache = new TestSdkCache(new CacheManager(), _loggerFactory);

        Assert.NotNull(sdkCache.CacheName);
        Assert.Equal(sdkCache.GetType().Name, sdkCache.CacheName);
    }

    [Fact]
    public void ConstructorWhenSucceedThenLoggerAreCreated()
    {
        Assert.NotNull(_sdkCache.ExecutionLog);
        Assert.NotNull(_sdkCache.CacheLog);
    }

    [Fact]
    public void ConstructorWhenSucceedThenCacheIsAssociatedWithDto()
    {
        Assert.NotNull(_sdkCache.RegisteredDtoTypes);
        Assert.NotEmpty(_sdkCache.RegisteredDtoTypes);
    }

    [Fact]
    public void ConstructorWhenSucceedThenCacheIsRegisteredInCacheManager()
    {
        var mockCacheManager = new Mock<ICacheManager>();

        var sdkCache = new TestSdkCache(mockCacheManager.Object, _loggerFactory);

        Assert.NotNull(sdkCache);
        mockCacheManager.Verify(v => v.RegisterCache(It.IsAny<string>(), It.Is<TestSdkCache>(match => match.Equals(sdkCache))), Times.Once);
    }

    [Fact]
    public void ConstructorWhenNullCacheManagerThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new TestSdkCache(null, _loggerFactory));
    }

    [Fact]
    public void ConstructorWhenNullLoggerFactoryThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => new TestSdkCache(new CacheManager(), null));
    }

    [Fact]
    public void ConstructorWhenNullDtoRegistrationThenThrow()
    {
        _ = Assert.Throws<InvalidOperationException>(() => new TestSdkCacheWithNullRegisteredDto(new CacheManager(), _loggerFactory));
    }

    [Fact]
    public void ConstructorWhenEmptyDtoRegistrationThenThrow()
    {
        _ = Assert.Throws<InvalidOperationException>(() => new TestSdkCacheWithEmptyRegisteredDto(new CacheManager(), _loggerFactory));
    }

    [Fact]
    public async Task CacheAddDtoWhenNullIdThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _sdkCache.CacheAddDtoAsync(null, new object(), ScheduleData.CultureEn, DtoType.SportEventSummary, null));
    }

    [Fact]
    public async Task CacheAddDtoWhenNullObjectThenThrow()
    {
        _ = await Assert.ThrowsAsync<ArgumentNullException>(() => _sdkCache.CacheAddDtoAsync(ScheduleData.MatchId, null, ScheduleData.CultureEn, DtoType.SportEventSummary, null));
    }

    [Fact]
    public async Task CacheAddDtoWhenSuccessThenCallCacheAddDtoItem()
    {
        var anyDto = new CarDto(new car { name = "some-car-name" });

        var result = await _sdkCache.CacheAddDtoAsync(ScheduleData.MatchId, anyDto, ScheduleData.CultureEn, DtoType.SportEventSummary, null);

        Assert.True(result);
        Assert.True(_sdkCache.IsAddDtoItemCalled);
    }

    [Fact]
    public void CacheDeleteWhenSuccessThenCallCacheDeleteItem()
    {
        _sdkCache.CacheDeleteItem(ScheduleData.MatchId.ToString(), CacheItemType.All);

        Assert.True(_sdkCache.IsDeleteCalled);
    }

    [Fact]
    public void CacheHasWhenSuccessThenCallCacheHasItem()
    {
        var result = _sdkCache.CacheHasItem(ScheduleData.MatchId.ToString(), CacheItemType.All);

        Assert.True(result);
        Assert.True(_sdkCache.IsHasItemCalled);
    }

    [Fact]
    public async Task CacheAddDtoWhenThrowsThenIsPropagated()
    {
        var anyDto = new CarDto(new car { name = "some-car-name" });
        var throwSdkCache = new TestSdkCacheWithExceptions(_cacheManager, _loggerFactory);

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => throwSdkCache.CacheAddDtoAsync(ScheduleData.MatchId, anyDto, ScheduleData.CultureEn, DtoType.SportEventSummary, null));

        Assert.True(throwSdkCache.IsAddDtoItemCalled);
    }

    [Fact]
    public void CacheDeleteWhenThrowsThenIsHandled()
    {
        var throwSdkCache = new TestSdkCacheWithExceptions(_cacheManager, _loggerFactory);

        throwSdkCache.CacheDeleteItem(ScheduleData.MatchId.ToString(), CacheItemType.All);

        Assert.True(throwSdkCache.IsDeleteCalled);
    }

    [Fact]
    public void CacheHasWhenThrowsThenIsHandled()
    {
        var throwSdkCache = new TestSdkCacheWithExceptions(_cacheManager, _loggerFactory);

        var result = throwSdkCache.CacheHasItem(ScheduleData.MatchId.ToString(), CacheItemType.All);

        Assert.False(result);
        Assert.True(throwSdkCache.IsHasItemCalled);
    }

    [Fact]
    public void LogSavingDtoConflictLogsToExecutionLog()
    {
        var testSdkCacheExecutionLogger = _loggerFactory.GetOrCreateLogger(typeof(TestSdkCache));

        _sdkCache.LogSavingDtoConflict(ScheduleData.MatchId, typeof(EntityList<VariantDescriptionDto>), GetType());

        Assert.Equal(1, testSdkCacheExecutionLogger.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, testSdkCacheExecutionLogger.CountBySearchTerm("Invalid data"));
    }

    private class TestSdkCache : SdkCache
    {
        public bool IsDeleteCalled;
        public bool IsHasItemCalled;
        public bool IsAddDtoItemCalled;

        public TestSdkCache(ICacheManager cacheManager, ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
            IsDeleteCalled = false;
            IsHasItemCalled = false;
            IsAddDtoItemCalled = false;
        }

        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new[] { DtoType.SportEventSummary };
        }

        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            IsDeleteCalled = true;
        }

        public override bool CacheHasItem(Urn id, CacheItemType cacheItemType)
        {
            IsHasItemCalled = true;
            return true;
        }

        protected override Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            IsAddDtoItemCalled = true;
            return Task.FromResult(true);
        }
    }

    private class TestSdkCacheWithNullRegisteredDto : TestSdkCache
    {
        public TestSdkCacheWithNullRegisteredDto(ICacheManager cacheManager, ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
        }

        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = null;
        }
    }

    private class TestSdkCacheWithEmptyRegisteredDto : TestSdkCache
    {
        public TestSdkCacheWithEmptyRegisteredDto(ICacheManager cacheManager, ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
        }

        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = Array.Empty<DtoType>();
        }
    }

    private class TestSdkCacheWithExceptions : TestSdkCache
    {
        public TestSdkCacheWithExceptions(ICacheManager cacheManager, ILoggerFactory loggerFactory)
            : base(cacheManager, loggerFactory)
        {
        }

        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            IsDeleteCalled = true;
            throw new InvalidOperationException("Can not delete");
        }

        public override bool CacheHasItem(Urn id, CacheItemType cacheItemType)
        {
            IsHasItemCalled = true;
            throw new InvalidOperationException("Can not check");
        }

        protected override Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            IsAddDtoItemCalled = true;
            throw new InvalidOperationException("Can not add item");
        }
    }
}
