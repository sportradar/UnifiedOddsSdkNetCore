// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;

public class XUnitLogger : ILogger
{
    private string LoggerName { get; }
    private LogLevel LoggerLevel { get; }
    private readonly ITestOutputHelper _output;
    internal ConcurrentBag<string> Messages { get; }

    public XUnitLogger(string loggerName, ITestOutputHelper output, LogLevel level = LogLevel.Trace)
    {
        LoggerName = loggerName;
        _output = output;
        Messages = [];
        LoggerLevel = level;
    }

    public XUnitLogger(Type loggerName, ITestOutputHelper output, LogLevel level = LogLevel.Trace)
    {
        LoggerName = loggerName.FullName;
        _output = output;
        Messages = [];
        LoggerLevel = level;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LoggerLevel;
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
            if (exception != null)
            {
                _output?.WriteLine(exception.ToString());
            }
        }
        catch
        {
            // ignore
        }
    }
}
