// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class LockManagerTests
{
    private const string Id = "some_id";
    private readonly LockManager _lockManager;
    private readonly XUnitLogger _logger;
    private readonly ConcurrentDictionary<string, DateTime> _uniqueItems;
    private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _lockSleep = TimeSpan.FromMilliseconds(100);

    public LockManagerTests(ITestOutputHelper outputHelper)
    {
        _logger = new XUnitLogger(typeof(LockManager), outputHelper);
        _uniqueItems = new ConcurrentDictionary<string, DateTime>();
        _lockManager = new LockManager(_uniqueItems, _lockTimeout, _lockSleep, _logger);
    }

    [Fact]
    public void WaitWhenForAllThenEmptyUniqueItems()
    {
        _lockManager.Wait();

        Assert.Empty(_uniqueItems.Keys);
    }

    [Fact]
    public void WaitWhenForIdThenAddsToUniqueItems()
    {
        _lockManager.Wait(Id);

        Assert.Contains(Id, _uniqueItems.Keys);
    }

    [Fact]
    public void WaitExecutesFast()
    {
        var stopWatch = Stopwatch.StartNew();

        _lockManager.Wait(Id);

        Assert.True(stopWatch.ElapsedMilliseconds < 10, $"Expected less than 10ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public void WaitWhenRequestingForSameIdThenWaitsTillTimeout()
    {
        var stopWatch = Stopwatch.StartNew();
        _lockManager.Wait(Id);

        _lockManager.Wait(Id);

        Assert.True(stopWatch.ElapsedMilliseconds >= _lockTimeout.TotalMilliseconds, $"Expected at least {_lockTimeout.TotalMilliseconds}ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task WaitWhenReleaseAsyncThenIsReleased()
    {
        // ReSharper disable once MethodHasAsyncOverload
        _lockManager.Wait(Id);

        await _lockManager.ReleaseAsync(Id);

        Assert.Empty(_uniqueItems.Keys);
    }

    [Fact]
    public void WaitWhenWaitForAllAndThenRequestingForSomeIdThenWaitsTillTimeout()
    {
        var stopWatch = Stopwatch.StartNew();
        _lockManager.Wait();

        _lockManager.Wait(Id);

        Assert.True(stopWatch.ElapsedMilliseconds >= _lockTimeout.TotalMilliseconds, $"Expected at least {_lockTimeout.TotalMilliseconds}ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task WaitAsyncWhenReleaseThenIsReleased()
    {
        await _lockManager.WaitAsync(Id);

        // ReSharper disable once MethodHasAsyncOverload
        _lockManager.Release(Id);

        Assert.Empty(_uniqueItems.Keys);
    }

    [Fact]
    public void WaitAsyncWhenForAllThenEmptyUniqueItems()
    {
        _lockManager.Wait();

        Assert.Empty(_uniqueItems.Keys);
    }

    [Fact]
    public async Task WaitAsyncWhenForIdThenAddsToUniqueItems()
    {
        await _lockManager.WaitAsync(Id);

        Assert.Contains(Id, _uniqueItems.Keys);
    }

    [Fact]
    public async Task WaitAsyncExecutesFast()
    {
        var stopWatch = Stopwatch.StartNew();

        await _lockManager.WaitAsync(Id);

        Assert.True(stopWatch.ElapsedMilliseconds < 10, $"Expected less than 10ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task WaitAsyncWhenRequestingForSameIdThenWaitsTillTimeout()
    {
        var stopWatch = Stopwatch.StartNew();
        await _lockManager.WaitAsync(Id);

        await _lockManager.WaitAsync(Id);

        Assert.True(stopWatch.ElapsedMilliseconds >= _lockTimeout.TotalMilliseconds, $"Expected at least {_lockTimeout.TotalMilliseconds}ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task WaitAsyncWhenWaitForAllAndThenRequestingForSomeIdThenWaitsTillTimeout()
    {
        var stopWatch = Stopwatch.StartNew();
        _lockManager.Wait();

        await _lockManager.WaitAsync(Id);

        Assert.True(stopWatch.ElapsedMilliseconds >= _lockTimeout.TotalMilliseconds, $"Expected at least {_lockTimeout.TotalMilliseconds}ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public void ReleaseThenRemoveFromUniqueItems()
    {
        _lockManager.Wait(Id);
        Assert.Contains(Id, _uniqueItems.Keys);

        _lockManager.Release(Id);

        Assert.DoesNotContain(Id, _uniqueItems.Keys);
    }

    [Fact]
    public async Task ReleaseAsyncThenRemoveFromUniqueItems()
    {
        // ReSharper disable once MethodHasAsyncOverload
        _lockManager.Wait(Id);
        Assert.Contains(Id, _uniqueItems.Keys);

        await _lockManager.ReleaseAsync(Id);

        Assert.DoesNotContain(Id, _uniqueItems.Keys);
    }

    [Fact]
    public void ReleaseExecutesFast()
    {
        _lockManager.Wait(Id);
        var stopWatch = Stopwatch.StartNew();

        _lockManager.Release(Id);

        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task ReleaseAsyncThenRemoveToUniqueItems()
    {
        await _lockManager.WaitAsync(Id);
        Assert.Contains(Id, _uniqueItems.Keys);

        await _lockManager.ReleaseAsync(Id);

        Assert.DoesNotContain(Id, _uniqueItems.Keys);
    }

    [Fact]
    public async Task ReleaseAsyncExecutesFast()
    {
        await _lockManager.WaitAsync(Id);
        var stopWatch = Stopwatch.StartNew();

        await _lockManager.ReleaseAsync(Id);

        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public void ReleaseAsyncWhenNoKeyThenExecuteFast()
    {
        var stopWatch = Stopwatch.StartNew();
        _lockManager.Wait();

        _lockManager.Release();

        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task CleanWhenItemDidNotExpireThenDoNotRemoveToUniqueItems()
    {
        await _lockManager.WaitAsync(Id);
        Assert.Contains(Id, _uniqueItems.Keys);

        await _lockManager.CleanAsync();

        Assert.Contains(Id, _uniqueItems.Keys);
    }

    [Fact]
    public async Task CleanWhenItemDidExpireThenRemoveToUniqueItems()
    {
        await _lockManager.WaitAsync(Id);
        Assert.Contains(Id, _uniqueItems.Keys);

        await Task.Delay(_lockTimeout.Add(TimeSpan.FromMilliseconds(100)));
        await _lockManager.CleanAsync();

        Assert.DoesNotContain(Id, _uniqueItems.Keys);
    }

    [Fact]
    public async Task CleanExecutesFast()
    {
        await _lockManager.WaitAsync(Id);
        var stopWatch = Stopwatch.StartNew();

        await _lockManager.CleanAsync();

        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task CleanWhenItemDidExpireThenRemoveItFast()
    {
        await _lockManager.WaitAsync(Id);
        Assert.Contains(Id, _uniqueItems.Keys);
        await Task.Delay(_lockTimeout.Add(TimeSpan.FromMilliseconds(100)));
        var stopWatch = Stopwatch.StartNew();

        await _lockManager.CleanAsync();

        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public void WaitRepeatThenExecuteFast()
    {
        var stopWatch = Stopwatch.StartNew();

        for (var i = 0; i < 10; i++)
        {
            _lockManager.Wait(Id);
            _lockManager.Release(Id);
        }

        stopWatch.Stop();
        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public async Task WaitAsyncRepeatThenExecuteFast()
    {
        var stopWatch = Stopwatch.StartNew();

        for (var i = 0; i < 10; i++)
        {
            await _lockManager.WaitAsync(Id);
            await _lockManager.ReleaseAsync(Id);
        }

        stopWatch.Stop();
        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.ElapsedMilliseconds}");
    }

    [Fact]
    public void WaitRepeatThenAllReleased()
    {
        for (var i = 0; i < 100; i++)
        {
            _lockManager.Wait(Id);
            _lockManager.Release(Id);
        }

        Assert.Empty(_uniqueItems);
    }

    [Fact]
    public async Task WaitAsyncRepeatThenAllReleased()
    {
        for (var i = 0; i < 100; i++)
        {
            await _lockManager.WaitAsync(Id);
            await _lockManager.ReleaseAsync(Id);
        }

        Assert.Empty(_uniqueItems);
    }

    [Fact]
    public void RepeatedRelease()
    {
        var stopWatch = Stopwatch.StartNew();

        _lockManager.Wait(Id);
        _logger.LogInformation("Time 1: {Time:0}", stopWatch.ElapsedMilliseconds);
        _lockManager.Release(Id);
        _logger.LogInformation("Time 2: {Time:0}", stopWatch.ElapsedMilliseconds);
        _lockManager.Release(Id);
        _logger.LogInformation("Time 3: {Time:0}", stopWatch.ElapsedMilliseconds);

        stopWatch.Stop();
        Assert.True(stopWatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [Fact(Timeout = 15000)]
    public async Task WaitAsync()
    {
        var stopWatch = Stopwatch.StartNew();
        var minTime = _lockTimeout.Add(TimeSpan.FromMilliseconds(100));

        await _lockManager.WaitAsync(Id);
        _logger.LogInformation("Time 1: {Time:0}", stopWatch.ElapsedMilliseconds);
        TestExecutionHelper.Sleep(minTime);
        _logger.LogInformation("Time 2: {Time:0}", stopWatch.ElapsedMilliseconds);
        await _lockManager.ReleaseAsync(Id);
        _logger.LogInformation("Time 3: {Time:0}", stopWatch.ElapsedMilliseconds);
        Assert.True(stopWatch.Elapsed >= _lockTimeout, $"{stopWatch.ElapsedMilliseconds} >= {minTime.TotalMilliseconds}");
        Assert.True(stopWatch.Elapsed < minTime.Multiply(2), $"{stopWatch.ElapsedMilliseconds} < {minTime.Multiply(2).TotalMilliseconds}");
        await _lockManager.WaitAsync(Id);
        _logger.LogInformation("Time 4: {Time:0}", stopWatch.ElapsedMilliseconds);
        await _lockManager.ReleaseAsync(Id);
        stopWatch.Stop();
        _logger.LogInformation("Time 5: {Time:0}", stopWatch.ElapsedMilliseconds);

        Assert.True(stopWatch.Elapsed >= _lockTimeout, $"{stopWatch.ElapsedMilliseconds} >= {minTime.TotalMilliseconds}");
    }

    [Fact]
    public void WaitWhenNewWaitRequestThenWaitTillFirstTimeout()
    {
        var stopWatch = Stopwatch.StartNew();

        _lockManager.Wait(Id);
        _logger.LogInformation("Time 1: {Time:0}", stopWatch.ElapsedMilliseconds);
        _lockManager.Wait(Id);
        _logger.LogInformation("Time 2: {Time:0}", stopWatch.ElapsedMilliseconds);
        _lockManager.Release(Id);
        _logger.LogInformation("Time 3: {Time:0}", stopWatch.ElapsedMilliseconds);

        stopWatch.Stop();
        Assert.True(stopWatch.Elapsed >= _lockTimeout, $"{stopWatch.ElapsedMilliseconds} >= {_lockTimeout.TotalMilliseconds}");
    }

    [Fact(Timeout = 20000)]
    public async Task CleanOneRespectsLockTimeout()
    {
        var stopWatch = Stopwatch.StartNew();

        await _lockManager.WaitAsync(Id);
        _logger.LogInformation("Time 1: {Time:0}", stopWatch.ElapsedMilliseconds);
        TestExecutionHelper.Sleep(_lockTimeout.Add(TimeSpan.FromMilliseconds(100)));
        _logger.LogInformation("Time 2: {Time:0}", stopWatch.ElapsedMilliseconds);
        await _lockManager.CleanAsync();
        _logger.LogInformation("Time 3: {Time:0}", stopWatch.ElapsedMilliseconds);
        await _lockManager.WaitAsync(Id);
        _logger.LogInformation("Time 4: {Time:0}", stopWatch.ElapsedMilliseconds);
        stopWatch.Stop();

        Assert.True(stopWatch.Elapsed >= _lockTimeout, $"{stopWatch.ElapsedMilliseconds} >= {_lockTimeout.TotalMilliseconds}");
    }

    [Fact(Timeout = 20000)]
    public async Task CleanOneWithZeroTimeout()
    {
        var stopWatch = Stopwatch.StartNew();

        await _lockManager.WaitAsync(Id);
        _logger.LogInformation("Time 1: {Time:0}", stopWatch.ElapsedMilliseconds);
        await _lockManager.CleanAsync(_uniqueItems, TimeSpan.Zero);
        _logger.LogInformation("Time 2: {Time:0}", stopWatch.ElapsedMilliseconds);
        await _lockManager.WaitAsync(Id);
        _logger.LogInformation("Time 3: {Time:0}", stopWatch.ElapsedMilliseconds);

        stopWatch.Stop();
        Assert.True(stopWatch.ElapsedMilliseconds < 100, $"Expected less than 100ms, got {stopWatch.Elapsed}");
    }
}
