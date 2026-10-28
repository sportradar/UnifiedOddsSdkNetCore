// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Extensions
{
    internal static class UofConfigurationExtensions
    {
        internal static bool AuthenticationIsConfigured(this IUofConfiguration configuration)
        {
            return configuration.Authentication != null;
        }
    }
}
