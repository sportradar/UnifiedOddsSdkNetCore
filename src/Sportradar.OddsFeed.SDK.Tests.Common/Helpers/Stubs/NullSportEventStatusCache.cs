// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers.Stubs;

internal class NullSportEventStatusCache : ISportEventStatusCache
{
    public static readonly NullSportEventStatusCache Instance = new NullSportEventStatusCache();

    public IEnumerable<DtoType> RegisteredDtoTypes => new List<DtoType> { };

    public string CacheName => nameof(NullSportEventStatusCache);

    public void RegisterCache()
    {
        throw new NotImplementedException();
    }

    public void SetDtoTypes()
    {
        throw new NotImplementedException();
    }

    public Task<bool> CacheAddDtoAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
    {
        throw new NotImplementedException();
    }

    public void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
    {
        throw new NotImplementedException();
    }

    public void CacheDeleteItem(string id, CacheItemType cacheItemType)
    {
        throw new NotImplementedException();
    }

    public bool CacheHasItem(Urn id, CacheItemType cacheItemType)
    {
        throw new NotImplementedException();
    }

    public bool CacheHasItem(string id, CacheItemType cacheItemType)
    {
        throw new NotImplementedException();
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<SportEventStatusCacheItem> GetSportEventStatusAsync(Urn eventId)
    {
        throw new NotImplementedException();
    }

    public void AddEventIdForTimelineIgnore(Urn eventId, int producerId, Type messageType)
    {
        throw new NotImplementedException();
    }
}
