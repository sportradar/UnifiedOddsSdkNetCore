// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.MockLog;

public class XUnitLogger : ILogger
{
    private string LoggerName { get; }
    private readonly ITestOutputHelper _output;
    internal ConcurrentBag<string> Messages { get; }

    public XUnitLogger(string loggerName, ITestOutputHelper output)
    {
        LoggerName = loggerName;
        _output = output;
        Messages = new ConcurrentBag<string>();
    }

    public XUnitLogger(Type loggerName, ITestOutputHelper output)
    {
        LoggerName = loggerName.FullName;
        _output = output;
        Messages = new ConcurrentBag<string>();
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public int CountByLevel(LogLevel logLevel)
    {
        return Messages.Count(w => w.StartsWith($"({logLevel})", StringComparison.InvariantCultureIgnoreCase));
    }

    public int CountBySearchTerm(string searchTerm)
    {
        return Messages.Count(w => w.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var msg = $"({logLevel}) {LoggerName}: {formatter(state, exception)}";
        Messages.Add(msg);
        try
        {
            _output?.WriteLine(msg);
        }
        catch
        {
            // ignore
        }
    }
}
