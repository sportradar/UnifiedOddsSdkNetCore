// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Factory extension for DI
    /// </summary>
    internal static class GenerateClassWithDataFactoryExtension
    {
        /// <summary>
        /// Add sdk http client factory to service collection
        /// </summary>
        /// <param name="services">Base service collection</param>
        public static void AddSdkHttpClientFactory(this IServiceCollection services)
        {
            services.AddTransient<ISdkHttpClientFactory, SdkHttpClientFactory>();
            services.AddSingleton<Func<ISdkHttpClientFactory>>(x => () => x.GetService<ISdkHttpClientFactory>());
            services.AddSingleton<ISdkHttpClientFactory, SdkHttpClientFactory>();
        }
    }

    internal interface ISdkHttpClientFactory
    {
        ISdkHttpClient Create(SdkHttpClientType httpClientType);
    }

    internal class SdkHttpClientFactory : ISdkHttpClientFactory
    {
        private readonly Func<ISdkHttpClient> _factory;

        public SdkHttpClientFactory(Func<ISdkHttpClient> factory)
        {
            _factory = factory;
        }

        public ISdkHttpClient Create(SdkHttpClientType httpClientType)
        {
            return _factory();
        }
    }
}
