// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers.Stubs;

public class NullLocalizedNamedValueCache : ILocalizedNamedValueCache
{
    public static NullLocalizedNamedValueCache Instance { get; } = new NullLocalizedNamedValueCache();

    public string CacheName => nameof(NullLocalizedNamedValueCache);

    public Task<ILocalizedNamedValue> GetAsync(int id, IEnumerable<CultureInfo> cultures = null)
    {
        throw new NotImplementedException();
    }

    public bool IsValueDefined(int id)
    {
        throw new NotImplementedException();
    }
}
