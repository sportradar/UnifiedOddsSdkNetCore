// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class SdkTimerTests
{
    private const int TimerDueTimeInMs = 500;
    private const int TimerPeriodInMs = 500;
    private readonly SdkTimer _sdkTimer = new SdkTimer("defaultTimer", TimeSpan.FromMilliseconds(TimerDueTimeInMs), TimeSpan.FromMilliseconds(TimerPeriodInMs));
    private readonly List<string> _timerMsgs = [];

    [Fact]
    public void TimerNormalInitialization()
    {
        _sdkTimer.ShouldNotBeNull();
        _timerMsgs.ShouldBeEmpty();
    }

    [Fact]
    public void TimerWrongDueTime()
    {
        Action action = () => SdkTimer.Create("wrongTimer", TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(10));
        action.Should().Throw<ArgumentException>();
    }

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

        await PauseForTwoPeriods();

        _timerMsgs.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task TimerFailedOnTickWillNotBreak()
    {
        SetupTimerWithFailingEventHandler();
        _sdkTimer.Start();

        await PauseForTwoPeriods();

        _timerMsgs.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task TimerFailedOnTickWillContinueOnPeriod()
    {
        SetupTimerWithFailingEventHandler();
        _sdkTimer.Start();

        await PauseForTwoPeriods();

        _timerMsgs.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task TimerFireOnce()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
        sdkTimer.Elapsed += SdkTimerOnElapsed;

        sdkTimer.FireOnce(TimeSpan.Zero);
        await Task.Delay(50);

        _timerMsgs.Count.ShouldBe(1);
    }

    [Fact]
    public void TimerWhenDisposedThenReturnState()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        sdkTimer.Dispose();

        sdkTimer.IsDisposed().ShouldBeTrue();
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S3966:Objects should not be disposed more than once", Justification = "Allowed in this test")]
    public void TimerWhenDisposeTwiceThenReturnState()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        sdkTimer.Dispose();
        sdkTimer.Dispose();

        sdkTimer.IsDisposed().ShouldBeTrue();
    }

    [Fact]
    public void TimerWhenDisposedThenStartingThrows()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
        sdkTimer.Dispose();

        var action = () => sdkTimer.Start();

        action.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void TimerWhenDisposedThenStartingWithParamsThrows()
    {
        var sdkTimer = new SdkTimer("fireOnceTimer", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
        sdkTimer.Dispose();

        var action = () => sdkTimer.Start(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        action.Should().Throw<ObjectDisposedException>();
    }

    private void SdkTimerOnElapsed(object sender, EventArgs e)
    {
        _timerMsgs.Add($"{_timerMsgs.Count + 1}. message");
    }

    private void SetupTimerWithFailingEventHandler()
    {
        _sdkTimer.Elapsed += (_, _) =>
        {
            _timerMsgs.Add($"{_timerMsgs.Count + 1}. message with error");
            throw new InvalidOperationException("Some error");
        };
    }

    private static async Task PauseForTwoPeriods()
    {
        await Task.Delay(TimerDueTimeInMs + TimerPeriodInMs + (TimerPeriodInMs / 2));
    }
}
