// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Pipeline format fails with primary constructor")]
public class XUnitLogger<T> : ILogger<T>
{
    public XUnitLogger InnerLogger { get; }

    public XUnitLogger(ITestOutputHelper outputHelper)
    {
        InnerLogger = new XUnitLogger(typeof(T).FullName, outputHelper);
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return InnerLogger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return InnerLogger.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        InnerLogger.Log(logLevel, eventId, state, exception, formatter);
    }
}
