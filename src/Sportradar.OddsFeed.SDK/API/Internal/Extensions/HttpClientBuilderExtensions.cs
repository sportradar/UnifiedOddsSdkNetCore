// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Extensions
{
    internal static class HttpClientBuilderExtensions
    {
        internal static IHttpClientBuilder AddHttpMessageHandlerIf<THandler>(this IHttpClientBuilder builder, Func<bool> predicate)
            where THandler : DelegatingHandler
        {
            if (predicate())
            {
                builder.AddHttpMessageHandler<THandler>();
            }

            return builder;
        }
    }
}
