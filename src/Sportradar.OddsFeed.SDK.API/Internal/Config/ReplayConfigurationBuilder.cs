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

            var config = new OddsFeedConfiguration(AccessToken,
                                                   SdkEnvironment.Replay,
                                                   DefaultLocale,
                                                   SupportedLocales,
                                                   SdkInfo.ReplayHost,
                                                   null,
                                                   SdkInfo.DefaultHostPort,
                                                   null,
                                                   null,
                                                   null,
                                                   true,
                                                   true,
                                                   SdkInfo.MaxInactivitySeconds,
                                                   SdkInfo.MaxRecoveryExecutionInSeconds,
                                                   NodeId,
                                                   DisabledProducers,
                                                   ExceptionHandlingStrategy,
                                                   false,
                                                   Section);

            return config;
        }
    }
}
