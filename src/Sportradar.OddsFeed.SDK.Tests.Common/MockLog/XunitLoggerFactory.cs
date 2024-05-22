// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.MockLog;

public class XunitLoggerFactory : ILoggerFactory
{
    public ILogger LastLogger { get; private set; }
    private readonly ITestOutputHelper _outputHelper;
    private readonly ConcurrentDictionary<string, ILogger> _registeredLoggers;

    public XunitLoggerFactory(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
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
        if (_registeredLoggers.ContainsKey(categoryName))
        {
            return _registeredLoggers[categoryName];
        }

        var logger = new XUnitLogger(categoryName, _outputHelper);
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

    public void AddProvider(ILoggerProvider provider)
    {
    }

    private string FormatLoggerName(string start)
    {
        return start.Replace("+", ".");
    }
}
