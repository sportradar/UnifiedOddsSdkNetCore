using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class LockManagerTests
{
    private readonly LockManager _lockManager;

    public LockManagerTests()
    {
        _lockManager = new LockManager(new ConcurrentDictionary<string, DateTime>(), TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(200));
    }

    [Fact]
    public void WaitRepeat()
    {
        const string id = "some_id";
        var stopWatch = Stopwatch.StartNew();
        for (var i = 0; i < 10; i++)
        {
            _lockManager.Wait(id);
            _lockManager.Release(id);
        }
        stopWatch.Stop();
        Assert.True(stopWatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RepeatedRelease()
    {
        const string id = "some_id";
        var stopWatch = Stopwatch.StartNew();

        _lockManager.Wait(id);
        _lockManager.Release(id);
        _lockManager.Release(id);

        stopWatch.Stop();
        Assert.True(stopWatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    //TODO unreliable in pipeline
    //[Fact(Timeout = 15000)]
    public void Wait()
    {
        const string id = "some_id";
        var stopWatch = Stopwatch.StartNew();
        _lockManager.Wait(id);
        Task.Delay(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();
        _lockManager.Release(id);
        Assert.True(stopWatch.Elapsed > TimeSpan.FromSeconds(3));
        Assert.True(stopWatch.Elapsed < TimeSpan.FromSeconds(10));
        _lockManager.Wait(id);
        _lockManager.Release(id);
        stopWatch.Stop();
        Assert.True(stopWatch.Elapsed > TimeSpan.FromSeconds(3));
        Assert.True(stopWatch.Elapsed < TimeSpan.FromSeconds(10));
    }

    //unreliable in pipeline
    //[Fact(Timeout = 20000)]
    public void WaitTillTimeout()
    {
        const string id = "some_id";
        var stopWatch = Stopwatch.StartNew();
        _lockManager.Wait(id);
        _lockManager.Wait(id);
        _lockManager.Release(id);
        stopWatch.Stop();
        Assert.True(stopWatch.Elapsed > TimeSpan.FromSeconds(10));
        Assert.True(stopWatch.Elapsed < TimeSpan.FromSeconds(15));
    }
}
