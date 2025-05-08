// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

internal class SportEventCacheBuilder
{
    private ICacheStore<string> _cache;
    private ICacheManager _cacheManager;
    private IReadOnlyCollection<CultureInfo> _langauges;
    private IDataRouterManager _dataRouterManager;
    private ILoggerFactory _loggerFactory;
    private ISportEventCacheItemFactory _sportEventCacheItemFactory;
    private ISdkTimer _timer;

    public SportEventCacheBuilder WithCache(ICacheStore<string> cache)
    {
        _cache = cache;
        return this;
    }

    public SportEventCacheBuilder WithDataRouterManager(IDataRouterManager dataRouterManager)
    {
        _dataRouterManager = dataRouterManager;
        return this;
    }

    public SportEventCacheBuilder WithCacheItemFactory(ISportEventCacheItemFactory factory)
    {
        _sportEventCacheItemFactory = factory;
        return this;
    }

    public SportEventCacheBuilder WithTimer(ISdkTimer timer)
    {
        _timer = timer;
        return this;
    }

    public SportEventCacheBuilder WithCultures(IReadOnlyCollection<CultureInfo> cultures)
    {
        _langauges = cultures;
        return this;
    }

    public SportEventCacheBuilder WithCacheManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        return this;
    }

    public SportEventCacheBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    public SportEventCacheBuilder WithLanguages(CultureInfo[] languages)
    {
        _langauges = languages;
        return this;
    }

    public SportEventCache Build()
    {
        return new SportEventCache(_cache,
                                   _dataRouterManager,
                                   _sportEventCacheItemFactory,
                                   _timer,
                                   _langauges,
                                   _cacheManager,
                                   _loggerFactory);
    }
}
