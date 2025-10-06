// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestSportEventStatusMapper : SportEventStatusMapperBase
{
    public static ISportEventStatus GetTestEventStatus()
    {
        return new SportEventStatus(new TestSportEventStatusMapper().CreateNotStarted(), TestLocalizedNamedValueCache.CreateMatchStatusCache(TestData.Cultures3, ExceptionHandlingStrategy.Throw));
    }
}
