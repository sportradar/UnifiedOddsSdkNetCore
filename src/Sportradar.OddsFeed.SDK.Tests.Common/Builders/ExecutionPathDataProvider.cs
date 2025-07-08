// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

internal class ExecutionPathDataProvider<TOut> where TOut : class
{
    private IDataProvider<TOut> _criticalDataProvider;
    private IDataProvider<TOut> _nonCriticalDataProvider;

    private ExecutionPathDataProvider() { }

    public static ExecutionPathDataProvider<T> Create<T>() where T : class
    {
        return new ExecutionPathDataProvider<T>();
    }

    public ExecutionPathDataProvider<TOut> WithCriticalDataProvider(IDataProvider<TOut> provider)
    {
        _criticalDataProvider = provider;
        return this;
    }

    public ExecutionPathDataProvider<TOut> WithNonCriticalDataProvider(IDataProvider<TOut> provider)
    {
        _nonCriticalDataProvider = provider;
        return this;
    }

    public Entities.Rest.Internal.ExecutionPathDataProvider<TOut> Build()
    {
        return new Entities.Rest.Internal.ExecutionPathDataProvider<TOut>(_criticalDataProvider, _nonCriticalDataProvider);
    }
}
