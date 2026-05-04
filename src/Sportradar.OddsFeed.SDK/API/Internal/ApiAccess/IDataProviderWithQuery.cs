// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    internal interface IDataProviderWithQuery<T> : IDataProvider<T> where T : class
    {
        Task<T> GetDataAsync(IReadOnlyDictionary<string, string> queryParameters, IReadOnlyDictionary<string, string> headers);
    }
}
