// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers.Stubs;

internal class NullSportEventCache : ISportEventCache
{
    public static NullSportEventCache Instance { get; } = new NullSportEventCache();

    public IEnumerable<DtoType> RegisteredDtoTypes => new List<DtoType> { };

    public string CacheName => nameof(NullSportEventCache);

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

    public Task<IEnumerable<ExportableBase>> ExportAsync()
    {
        throw new NotImplementedException();
    }

    public Task ImportAsync(IEnumerable<ExportableBase> items)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyDictionary<string, int> CacheStatus()
    {
        throw new NotImplementedException();
    }

    public ISportEventCacheItem GetEventCacheItem(Urn id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Tuple<Urn, Urn>>> GetEventIdsAsync(Urn tournamentId, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Tuple<Urn, Urn>>> GetEventIdsAsync(DateTime? date, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TournamentInfoCacheItem>> GetActiveTournamentsAsync(CultureInfo culture = null)
    {
        throw new NotImplementedException();
    }

    public void AddFixtureTimestamp(Urn id)
    {
        throw new NotImplementedException();
    }

    public int DeleteSportEventsFromCache(DateTime before)
    {
        throw new NotImplementedException();
    }

    public Task<Urn> GetEventSportIdAsync(Urn id)
    {
        throw new NotImplementedException();
    }
}
