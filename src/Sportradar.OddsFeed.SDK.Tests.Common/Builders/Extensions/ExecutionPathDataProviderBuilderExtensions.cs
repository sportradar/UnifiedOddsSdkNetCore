// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class ExecutionPathDataProviderBuilderExtensions
{
    internal static ExecutionPathDataProvider<T> WithMockedDependencies<T>(this ExecutionPathDataProvider<T> builder)
        where T : class
    {
        var mock = new Mock<IDataProvider<T>>();
        return builder
              .WithCriticalDataProvider(mock.Object)
              .WithNonCriticalDataProvider(mock.Object);
    }
}
