// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

internal static class SportEntityFactoryBuilderExtensions
{
    public static SportEntityFactoryBuilder WithMockedSportDataCache(this SportEntityFactoryBuilder builder)
    {
        var mock = new Mock<ISportDataCache>();
        return builder.WithSportDataCache(mock.Object);
    }

    public static SportEntityFactoryBuilder WithMockedSportEventCache(this SportEntityFactoryBuilder builder)
    {
        var mock = new Mock<ISportEventCache>();
        return builder.WithSportEventCache(mock.Object);
    }

    public static SportEntityFactoryBuilder WithMockedEventStatusCache(this SportEntityFactoryBuilder builder)
    {
        var mock = new Mock<ISportEventStatusCache>();
        return builder.WithEventStatusCache(mock.Object);
    }

    public static SportEntityFactoryBuilder WithMockedMatchStatusCache(this SportEntityFactoryBuilder builder)
    {
        var mock = new Mock<ILocalizedNamedValueCache>();
        return builder.WithMatchStatusCache(mock.Object);
    }

    public static SportEntityFactoryBuilder WithMockedProfileCache(this SportEntityFactoryBuilder builder)
    {
        var mock = new Mock<IProfileCache>();
        return builder.WithProfileCache(mock.Object);
    }

    public static SportEntityFactoryBuilder WithAllMockedDependencies(this SportEntityFactoryBuilder builder)
    {
        return builder
              .WithMockedSportDataCache()
              .WithMockedSportEventCache()
              .WithMockedEventStatusCache()
              .WithMockedMatchStatusCache()
              .WithMockedProfileCache();
    }
}
