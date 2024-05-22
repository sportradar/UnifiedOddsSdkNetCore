// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.Markets;

public class NameProviderFactoryTests
{
    private readonly Mock<IMarketCacheProvider> _marketCacheProviderMock;
    private readonly Mock<IProfileCache> _profileCacheMock;
    private readonly Mock<INameExpressionFactory> _nameExpressionFactoryMock;

    public NameProviderFactoryTests()
    {
        _marketCacheProviderMock = new Mock<IMarketCacheProvider>();
        _profileCacheMock = new Mock<IProfileCache>();
        _nameExpressionFactoryMock = new Mock<INameExpressionFactory>();
    }

    [Fact]
    public void ConstructorInitialized()
    {
        var factory = new NameProviderFactory(_marketCacheProviderMock.Object, _profileCacheMock.Object, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw);

        Assert.NotNull(factory);
    }

    [Fact]
    public void ConstructorWhenMissingMarketCacheProviderThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new NameProviderFactory(null, _profileCacheMock.Object, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw));
    }

    [Fact]
    public void ConstructorWhenMissingProfileCacheThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new NameProviderFactory(_marketCacheProviderMock.Object, null, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw));
    }

    [Fact]
    public void ConstructorWhenMissingNameExpressionFactoryThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new NameProviderFactory(_marketCacheProviderMock.Object, _profileCacheMock.Object, null, ExceptionHandlingStrategy.Throw));
    }

    [Fact]
    public void BuildNameProviderWhenCallCorrectly()
    {
        var factory = new NameProviderFactory(_marketCacheProviderMock.Object, _profileCacheMock.Object, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw);

        var nameProvider = factory.BuildNameProvider(new Mock<ISportEvent>().Object, 1, new Dictionary<string, string>());

        Assert.NotNull(nameProvider);
    }

    [Fact]
    public void BuildNameProviderWhenMissingSportEventThrows()
    {
        var factory = new NameProviderFactory(_marketCacheProviderMock.Object, _profileCacheMock.Object, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw);

        Assert.Throws<ArgumentNullException>(() => factory.BuildNameProvider(null, 1, new Dictionary<string, string>()));
    }

    [Fact]
    public void BuildNameProviderAcceptZeroMarketId()
    {
        var factory = new NameProviderFactory(_marketCacheProviderMock.Object, _profileCacheMock.Object, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw);

        var nameProvider = factory.BuildNameProvider(new Mock<ISportEvent>().Object, 0, new Dictionary<string, string>());

        Assert.NotNull(nameProvider);
    }

    [Fact]
    public void BuildNameProviderAcceptNegativeMarketId()
    {
        var factory = new NameProviderFactory(_marketCacheProviderMock.Object, _profileCacheMock.Object, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw);

        var nameProvider = factory.BuildNameProvider(new Mock<ISportEvent>().Object, -1, new Dictionary<string, string>());

        Assert.NotNull(nameProvider);
    }

    [Fact]
    public void BuildNameProviderAcceptNullSpecifiers()
    {
        var factory = new NameProviderFactory(_marketCacheProviderMock.Object, _profileCacheMock.Object, _nameExpressionFactoryMock.Object, ExceptionHandlingStrategy.Throw);

        var nameProvider = factory.BuildNameProvider(new Mock<ISportEvent>().Object, 1, null);

        Assert.NotNull(nameProvider);
    }
}
