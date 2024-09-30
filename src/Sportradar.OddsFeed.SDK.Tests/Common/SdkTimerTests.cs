// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class SdkTimerTests
{
    private readonly ISdkTimer _sdkTimer;
    private readonly IList<string> _timerMsgs;

    public SdkTimerTests()
    {
        ThreadPool.SetMinThreads(100, 100);
        _timerMsgs = new List<string>();
        _sdkTimer = new SdkTimer("defaultTimer", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void SdkTimerOnElapsed(object sender, EventArgs e)
    {
        _timerMsgs.Add($"{_timerMsgs.Count + 1}. message");
    }

    [Fact]
    public void TimerNormalInitialization()
    {
        Assert.NotNull(_sdkTimer);
        Assert.Empty(_timerMsgs);
    }

    [Fact]
    public void TimerWrongDueTime()
    {
        Action action = () => SdkTimer.Create("wrongTimer", TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(10));
        action.Should().Throw<ArgumentException>();
    }

    //[Fact]
    //public void TimerWrongPeriodTime()
    //{
    //    Action action = () => SdkTimer.Create(TimeSpan.FromSeconds(1), TimeSpan.Zero);
    //    action.Should().Throw<ArgumentException>();
    //}

    [Fact]
    public void TimerStartWrongDueTime()
    {
        var action = () => _sdkTimer.Start(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(10));
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TimerStartWrongPeriodTime()
    {
        var action = () => _sdkTimer.Start(TimeSpan.FromSeconds(1), TimeSpan.Zero);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task TimerNormalOperation()
    {
        _sdkTimer.Elapsed += SdkTimerOnElapsed;
        _sdkTimer.Start();
        await Task.Delay(5500);
        Assert.NotNull(_sdkTimer);
        Assert.NotEmpty(_timerMsgs);
        Assert.True(5 <= _timerMsgs.Count, $"Expected 5, actual {_timerMsgs.Count}");
    }

    // [Fact]
    // public async Task TimerFailedOnTickWillNotBreak()
    // {
    //     _sdkTimer.Elapsed += (sender, args) =>
    //     {
    //         _timerMsgs.Add($"{_timerMsgs.Count + 1}. message with error");
    //         throw new InvalidOperationException("Some error");
    //     };
    //     _sdkTimer.Start();
    //     await Task.Delay(1200);
    //     Assert.NotNull(_sdkTimer);
    //     Assert.NotEmpty(_timerMsgs);
    //     Assert.Single(_timerMsgs);
    // }

    [Fact]
    public async Task TimerFailedOnTickWillContinueOnPeriod()
    {
        _sdkTimer.Elapsed += (sender, args) =>
        {
            _timerMsgs.Add($"{_timerMsgs.Count + 1}. message with error");
            throw new InvalidOperationException("Some error");
        };
        _sdkTimer.Start();
        await Task.Delay(5200);
        Assert.NotNull(_sdkTimer);
        Assert.NotEmpty(_timerMsgs);
        Assert.True(5 <= _timerMsgs.Count, $"Expected 5, actual {_timerMsgs.Count}");
    }

    [Fact]
    public async Task TimerFireOnce()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
        sdkTimer.Elapsed += SdkTimerOnElapsed;
        sdkTimer.FireOnce(TimeSpan.Zero);
        await Task.Delay(3200);
        Assert.NotNull(_sdkTimer);
        Assert.NotEmpty(_timerMsgs);
        Assert.Single(_timerMsgs);
    }

    [Fact]
    public void TimerWhenDisposedThenReturnState()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        sdkTimer.Dispose();

        Assert.True(sdkTimer.IsDisposed());
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Allowed in this test")]
    public void TimerWhenDisposeTwiceThenReturnState()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        sdkTimer.Dispose();
        sdkTimer.Dispose();

        Assert.True(sdkTimer.IsDisposed());
    }

    [Fact]
    public void TimerWhenDisposedThenStartingThrows()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
        sdkTimer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => sdkTimer.Start());
    }

    [Fact]
    public void TimerWhenDisposedThenStartingWithParamsThrows()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
        sdkTimer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => sdkTimer.Start(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)));
    }
}
