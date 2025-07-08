// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    internal interface IExecutionPathDataProvider<TOut> where TOut : class
    {
        event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        Task<TOut> GetDataAsync(RequestOptions requestOptions, params string[] identifiers);
    }
}
