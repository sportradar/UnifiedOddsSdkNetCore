// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

internal class SportDataProviderBuilder
{
    private ICacheManager _cacheManager;
    private IDataRouterManager _dataRouterManager;
    private IEnumerable<CultureInfo> _defaultCultures;
    private ExceptionHandlingStrategy _exceptionStrategy;
    private ILocalizedNamedValueCache _matchStatusCache;
    private IProfileCache _profileCache;
    private ISportDataCache _sportDataCache;
    private ISportEntityFactory _sportEntityFactory;
    private ISportEventCache _sportEventCache;
    private ISportEventStatusCache _sportEventStatusCache;

    public SportDataProviderBuilder WithSportEntityFactory(ISportEntityFactory sportEntityFactory)
    {
        _sportEntityFactory = sportEntityFactory;
        return this;
    }

    public SportDataProviderBuilder WithSportEventCache(ISportEventCache sportEventCache)
    {
        _sportEventCache = sportEventCache;
        return this;
    }

    public SportDataProviderBuilder WithSportEventStatusCache(ISportEventStatusCache sportEventStatusCache)
    {
        _sportEventStatusCache = sportEventStatusCache;
        return this;
    }

    public SportDataProviderBuilder WithProfileCache(IProfileCache profileCache)
    {
        _profileCache = profileCache;
        return this;
    }

    public SportDataProviderBuilder WithSportDataCache(ISportDataCache sportDataCache)
    {
        _sportDataCache = sportDataCache;
        return this;
    }

    public SportDataProviderBuilder WithDefaultCultures(IEnumerable<CultureInfo> defaultCultures)
    {
        _defaultCultures = defaultCultures;
        return this;
    }

    public SportDataProviderBuilder WithExceptionStrategy(ExceptionHandlingStrategy exceptionStrategy)
    {
        _exceptionStrategy = exceptionStrategy;
        return this;
    }

    public SportDataProviderBuilder WithCacheManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        return this;
    }

    public SportDataProviderBuilder WithMatchStatusCache(ILocalizedNamedValueCache matchStatusCache)
    {
        _matchStatusCache = matchStatusCache;
        return this;
    }

    public SportDataProviderBuilder WithDataRouterManager(IDataRouterManager dataRouterManager)
    {
        _dataRouterManager = dataRouterManager;
        return this;
    }

    public ISportDataProvider Build()
    {
        return new SportDataProvider(_sportEntityFactory,
                                     _sportEventCache,
                                     _sportEventStatusCache,
                                     _profileCache,
                                     _sportDataCache,
                                     _defaultCultures,
                                     _exceptionStrategy,
                                     _cacheManager,
                                     _matchStatusCache,
                                     _dataRouterManager);
    }
}
