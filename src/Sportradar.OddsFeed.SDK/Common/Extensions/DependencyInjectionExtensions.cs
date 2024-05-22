// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;

namespace Sportradar.OddsFeed.SDK.Common.Extensions
{
    /// <summary>
    /// Extensions for Microsoft.Extensions.DependencyInjection
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Add all sdk services to service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">An <see cref="IUofConfiguration"/> used to configure sdk services</param>
        public static void AddUofSdk(this IServiceCollection services, IUofConfiguration configuration)
        {
            services.AddUofSdkServices(configuration);
        }
    }
}
