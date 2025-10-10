// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;

public class XunitLoggerFactory : ILoggerFactory
{
    public ILogger LastLogger { get; private set; }
    private readonly ITestOutputHelper _outputHelper;
    private readonly LogLevel _minimumLogLevel;
    private readonly ConcurrentDictionary<string, ILogger> _registeredLoggers;

    public XunitLoggerFactory(ITestOutputHelper outputHelper, LogLevel minimumLogLevel = LogLevel.Trace)
    {
        _outputHelper = outputHelper;
        _minimumLogLevel = minimumLogLevel;
        _registeredLoggers = new ConcurrentDictionary<string, ILogger>();
    }

    public void Dispose()
    {
        LastLogger = null;
        GC.SuppressFinalize(this);
    }

    public ILogger CreateLogger(string categoryName)
    {
        categoryName = FormatLoggerName(categoryName);
        if (_registeredLoggers.TryGetValue(categoryName, out var existingLogger))
        {
            return existingLogger;
        }

        var logger = new XUnitLogger(categoryName, _outputHelper, _minimumLogLevel);
        LastLogger = logger;
        _registeredLoggers[categoryName] = logger;
        return logger;
    }

    public ILogger CreateLogger(Type loggerType)
    {
        return CreateLogger(loggerType.FullName ?? loggerType.Name);
    }

    public XUnitLogger GetOrCreateLogger(Type loggerType)
    {
        return (XUnitLogger)CreateLogger(loggerType);
    }

    public XUnitLogger GetRegisteredLogger(string categoryName)
    {
        categoryName = FormatLoggerName(categoryName);
        return _registeredLoggers.TryGetValue(categoryName, out var logger) ? (XUnitLogger)logger : null;
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }

    private static string FormatLoggerName(string start)
    {
        return start.Replace("+", ".");
    }
}
