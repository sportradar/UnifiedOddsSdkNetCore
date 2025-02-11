// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class SportDataProviderBuilderExtensions
{
    public static SportDataProviderBuilder WithMockedSportEntityFactory(this SportDataProviderBuilder builder)
    {
        var mockSportEntityFactory = new Mock<ISportEntityFactory>();
        return builder.WithSportEntityFactory(mockSportEntityFactory.Object);
    }

    public static SportDataProviderBuilder WithMockedSportEventCache(this SportDataProviderBuilder builder)
    {
        var mockSportEventCache = new Mock<ISportEventCache>();
        return builder.WithSportEventCache(mockSportEventCache.Object);
    }

    public static SportDataProviderBuilder WithMockedSportEventStatusCache(this SportDataProviderBuilder builder)
    {
        var mockSportEventStatusCache = new Mock<ISportEventStatusCache>();
        return builder.WithSportEventStatusCache(mockSportEventStatusCache.Object);
    }

    public static SportDataProviderBuilder WithMockedProfileCache(this SportDataProviderBuilder builder)
    {
        var mockProfileCache = new Mock<IProfileCache>();
        return builder.WithProfileCache(mockProfileCache.Object);
    }

    public static SportDataProviderBuilder WithMockedSportDataCache(this SportDataProviderBuilder builder)
    {
        var mockSportDataCache = new Mock<ISportDataCache>();
        return builder.WithSportDataCache(mockSportDataCache.Object);
    }

    public static SportDataProviderBuilder WithMockedDefaultCultures(this SportDataProviderBuilder builder)
    {
        var mockDefaultCultures = new List<CultureInfo> { new("en-US"), new("es-ES") };
        return builder.WithDefaultCultures(mockDefaultCultures);
    }

    public static SportDataProviderBuilder WithMockedCacheManager(this SportDataProviderBuilder builder)
    {
        var mockCacheManager = new Mock<ICacheManager>();
        return builder.WithCacheManager(mockCacheManager.Object);
    }

    public static SportDataProviderBuilder WithMockedMatchStatusCache(this SportDataProviderBuilder builder)
    {
        var mockMatchStatusCache = new Mock<ILocalizedNamedValueCache>();
        return builder.WithMatchStatusCache(mockMatchStatusCache.Object);
    }

    public static SportDataProviderBuilder WithMockedDataRouterManager(this SportDataProviderBuilder builder)
    {
        var mockDataRouterManager = new Mock<IDataRouterManager>();
        return builder.WithDataRouterManager(mockDataRouterManager.Object);
    }

    public static SportDataProviderBuilder WithAllMockedDependencies(this SportDataProviderBuilder builder)
    {
        return builder
              .WithMockedSportEntityFactory()
              .WithMockedSportEventCache()
              .WithMockedSportEventStatusCache()
              .WithMockedProfileCache()
              .WithMockedSportDataCache()
              .WithMockedDefaultCultures()
              .WithExceptionStrategy(ExceptionHandlingStrategy.Catch)
              .WithMockedCacheManager()
              .WithMockedMatchStatusCache()
              .WithMockedDataRouterManager();
    }
}
