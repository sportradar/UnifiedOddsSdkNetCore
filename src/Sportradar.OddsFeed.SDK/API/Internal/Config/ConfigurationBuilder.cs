// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal class ConfigurationBuilder : RecoveryConfigurationBuilder<IConfigurationBuilder>, IConfigurationBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBuilder"/> class
        /// </summary>
        /// <param name="configuration">Current <see cref="UofConfiguration"/></param>
        /// <param name="sectionProvider">A <see cref="IUofConfigurationSectionProvider"/> used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="environment">An <see cref="SdkEnvironment"/> specifying the selected environment</param>
        /// <param name="bookmakerDetailsProviderFactory">Factory to create the bookmaker details provider</param>
        /// <param name="producersProviderFactory">Factory to create the producers provider</param>
        public ConfigurationBuilder(UofConfiguration configuration,
                                    IUofConfigurationSectionProvider sectionProvider,
                                    SdkEnvironment environment,
                                    Func<IUofConfiguration, IBookmakerDetailsProvider> bookmakerDetailsProviderFactory,
                                    Func<IUofConfiguration, IProducersProvider> producersProviderFactory)
            : base(configuration, sectionProvider, bookmakerDetailsProviderFactory, producersProviderFactory)
        {
            UofConfiguration.UpdateSdkEnvironment(environment);
        }

        public override IUofConfiguration Build()
        {
            PreBuildCheck();

            FetchBookmakerDetails();

            FetchProducers();

            return UofConfiguration;
        }
    }
}
