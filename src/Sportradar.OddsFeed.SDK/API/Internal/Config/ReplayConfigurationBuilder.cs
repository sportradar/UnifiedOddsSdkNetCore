/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal.Config
{
    internal class ReplayConfigurationBuilder : ConfigurationBuilderBase<IReplayConfigurationBuilder>, IReplayConfigurationBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayConfigurationBuilder"/> class
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="sectionProvider"></param>
        public ReplayConfigurationBuilder(string accessToken, IConfigurationSectionProvider sectionProvider)
            : base(accessToken, sectionProvider)
        {
        }

        public override IOddsFeedConfiguration Build()
        {
            PreBuildCheck();

            var mqHost = string.IsNullOrEmpty(Section?.Host)
                             ? EnvironmentManager.GetMqHost(SdkEnvironment.Replay)
                             : Section.Host;
            var apiHost = string.IsNullOrEmpty(Section?.ApiHost)
                              ? EnvironmentManager.GetApiHost(SdkEnvironment.Replay)
                              : Section.ApiHost;

            var config = new OddsFeedConfiguration(AccessToken,
                                                   SdkEnvironment.Replay,
                                                   DefaultLocale,
                                                   SupportedLocales,
                                                   mqHost,
                                                   null,
                                                   EnvironmentManager.DefaultMqHostPort,
                                                   null,
                                                   null,
                                                   apiHost,
                                                   true,
                                                   true,
                                                   SdkInfo.MaxInactivitySeconds,
                                                   SdkInfo.MaxRecoveryExecutionInSeconds,
                                                   SdkInfo.DefaultIntervalBetweenRecoveryRequests,
                                                   NodeId,
                                                   DisabledProducers,
                                                   ExceptionHandlingStrategy,
                                                   false,
                                                   HttpClientTimeout ?? SdkInfo.DefaultHttpClientTimeout,
                                                   HttpClientTimeout ?? SdkInfo.DefaultHttpClientTimeout,
                                                   Section);

            return config;
        }
    }
}
