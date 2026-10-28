// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Extensions
{
    internal static class SdkEnvironmentExtensions
    {
        internal static bool IsReplayEnvironment(this SdkEnvironment environment)
        {
            return environment == SdkEnvironment.ReplayWithIntegrationCredentials
                   || environment == SdkEnvironment.ReplayWithProductionCredentials;
        }
    }
}
