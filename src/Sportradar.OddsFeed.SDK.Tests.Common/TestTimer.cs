// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class TestTimer : ISdkTimer
{
    private readonly bool _raiseEventOnStart;

    public string TimerName { get; }

    public event EventHandler Elapsed;

    public TestTimer(bool raiseEventOnStart)
    {
        _raiseEventOnStart = raiseEventOnStart;
        TimerName = raiseEventOnStart.ToString();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Start()
    {
        if (!_raiseEventOnStart)
        {
            return;
        }

        var handler = Elapsed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public void Start(TimeSpan dueTime, TimeSpan period)
    {
        if (!_raiseEventOnStart)
        {
            return;
        }

        var handler = Elapsed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public void FireOnce(TimeSpan dueTime)
    {
        var handler = Elapsed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {

    }
}
