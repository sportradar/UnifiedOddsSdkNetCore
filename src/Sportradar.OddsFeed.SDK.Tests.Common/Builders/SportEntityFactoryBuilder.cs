// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

internal class SportEntityFactoryBuilder
{
    private ISportDataCache _sportDataCache = new Mock<ISportDataCache>().Object;
    private ISportEventCache _sportEventCache = new Mock<ISportEventCache>().Object;
    private ISportEventStatusCache _eventStatusCache = new Mock<ISportEventStatusCache>().Object;
    private ILocalizedNamedValueCache _matchStatusCache = new Mock<ILocalizedNamedValueCache>().Object;
    private IProfileCache _profileCache = new Mock<IProfileCache>().Object;

    public SportEntityFactoryBuilder WithSportDataCache(ISportDataCache sportDataCache)
    {
        _sportDataCache = sportDataCache;
        return this;
    }

    public SportEntityFactoryBuilder WithSportEventCache(ISportEventCache sportEventCache)
    {
        _sportEventCache = sportEventCache;
        return this;
    }

    public SportEntityFactoryBuilder WithEventStatusCache(ISportEventStatusCache eventStatusCache)
    {
        _eventStatusCache = eventStatusCache;
        return this;
    }

    public SportEntityFactoryBuilder WithMatchStatusCache(ILocalizedNamedValueCache matchStatusCache)
    {
        _matchStatusCache = matchStatusCache;
        return this;
    }

    public SportEntityFactoryBuilder WithProfileCache(IProfileCache profileCache)
    {
        _profileCache = profileCache;
        return this;
    }

    public SportEntityFactory Build()
    {
        return new SportEntityFactory(
                                      _sportDataCache,
                                      _sportEventCache,
                                      _eventStatusCache,
                                      _matchStatusCache,
                                      _profileCache
                                     );
    }
}
