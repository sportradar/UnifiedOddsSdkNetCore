// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Moq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

public class MockMarketCacheProviderBuilder
{
    private readonly Mock<IMarketCacheProvider> _mockMarketCacheProvider = new Mock<IMarketCacheProvider>();

    public static MockMarketCacheProviderBuilder Create()
    {
        return new MockMarketCacheProviderBuilder();
    }

    internal MockMarketCacheProviderBuilder WithReturning(IMarketDescription marketDescription)
    {
        _mockMarketCacheProvider.Setup(mcp => mcp.GetMarketDescriptionAsync((int)marketDescription.Id,
                                                                            It.IsAny<IReadOnlyDictionary<string, string>>(),
                                                                            It.IsAny<IReadOnlyCollection<CultureInfo>>(),
                                                                            false))
                                .ReturnsAsync(marketDescription);
        return this;
    }

    internal IMarketCacheProvider Build()
    {
        return _mockMarketCacheProvider.Object;
    }
}
