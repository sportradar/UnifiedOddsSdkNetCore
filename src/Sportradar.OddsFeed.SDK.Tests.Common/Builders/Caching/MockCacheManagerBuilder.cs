// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Caching;

public class MockCacheManagerBuilder
{
    private readonly Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>();

    public static MockCacheManagerBuilder Create()
    {
        return new MockCacheManagerBuilder();
    }

    public MockCacheManagerBuilder WithSaveDtoForSportEventStatus(odds_change oddsChange, CultureInfo culture)
    {
        _mockCacheManager.Setup(cm => cm.SaveDto(oddsChange.EventUrn, oddsChange.sport_event_status, culture, DtoType.SportEventStatus, null));
        return this;
    }

    internal ICacheManager Build()
    {
        return _mockCacheManager.Object;
    }
}
