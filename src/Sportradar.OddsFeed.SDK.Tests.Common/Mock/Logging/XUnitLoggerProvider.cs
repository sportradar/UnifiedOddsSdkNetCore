// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Logging;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;

public class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ILoggerFactory _loggerFactory;

    public XUnitLoggerProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;

    }
    public void Dispose()
    {
        _loggerFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggerFactory.CreateLogger(categoryName);
    }
}
