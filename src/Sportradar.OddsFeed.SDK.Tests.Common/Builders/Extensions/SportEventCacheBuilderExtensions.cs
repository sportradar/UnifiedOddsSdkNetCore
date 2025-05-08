// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class SportEventCacheBuilderExtensions
{
    public static SportEventCacheBuilder WithMockedCache(this SportEventCacheBuilder builder)
    {
        return builder.WithCache(new Mock<ICacheStore<string>>().Object);
    }

    public static SportEventCacheBuilder WithMockedDataRouterManager(this SportEventCacheBuilder builder)
    {
        return builder.WithDataRouterManager(new Mock<IDataRouterManager>().Object);
    }

    public static SportEventCacheBuilder WithMockedCacheItemFactory(this SportEventCacheBuilder builder)
    {
        return builder.WithCacheItemFactory(new Mock<ISportEventCacheItemFactory>().Object);
    }

    public static SportEventCacheBuilder WithMockedTimer(this SportEventCacheBuilder builder)
    {
        return builder.WithTimer(new Mock<ISdkTimer>().Object);
    }

    public static SportEventCacheBuilder WithDefaultCultures(this SportEventCacheBuilder builder)
    {
        return builder.WithCultures(new List<CultureInfo> { CultureInfo.InvariantCulture });
    }

    public static SportEventCacheBuilder WithMockedCacheManager(this SportEventCacheBuilder builder)
    {
        return builder.WithCacheManager(new Mock<ICacheManager>().Object);
    }

    public static SportEventCacheBuilder WithMockedLoggerFactory(this SportEventCacheBuilder builder)
    {
        return builder.WithLoggerFactory(new Mock<ILoggerFactory>().Object);
    }

    public static SportEventCacheBuilder WithAllMockedDependencies(this SportEventCacheBuilder builder)
    {
        return builder
              .WithMockedCache()
              .WithMockedDataRouterManager()
              .WithMockedCacheItemFactory()
              .WithMockedTimer()
              .WithDefaultCultures()
              .WithMockedCacheManager()
              .WithMockedLoggerFactory();
    }
}
