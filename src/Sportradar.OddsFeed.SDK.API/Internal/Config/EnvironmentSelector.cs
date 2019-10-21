/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.API.Internal.Config
{
    /// <summary>
    /// Class EnvironmentSelector
    /// </summary>
    /// <seealso cref="IEnvironmentSelector" />
    internal class EnvironmentSelector : IEnvironmentSelectorV1
    {
        /// <summary>
        /// An access token used to authenticate with the feed
        /// </summary>
        private readonly string _accessToken;

        /// <summary>
        /// A <see cref="IConfigurationSectionProvider"/> used to access <see cref="IOddsFeedConfigurationSection"/>
        /// </summary>
        private readonly IConfigurationSectionProvider _sectionProvider;

        /// <summary>
        /// Constructs a new instance of the <see cref="EnvironmentSelector"/> class
        /// </summary>
        /// <param name="accessToken">An access token used to authenticate with the feed</param>
        /// <param name="sectionProvider">A <see cref="IConfigurationSectionProvider"/> used to access <see cref="IOddsFeedConfigurationSection"/></param>
        internal EnvironmentSelector(string accessToken, IConfigurationSectionProvider sectionProvider)
        {
            Guard.Argument(accessToken).NotNull().NotEmpty();
            Guard.Argument(sectionProvider).NotNull();

            _accessToken = accessToken;
            _sectionProvider = sectionProvider;
        }

        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder" /> with properties set to values needed to access integration environment
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder" /> with properties set to values needed to access integration environment</returns>
        [Obsolete("Use SelectIntegration()")]
        public IConfigurationBuilder SelectStaging()
        {
            return SelectIntegration();
        }

        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder" /> with properties set to values needed to access integration environment
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder" /> with properties set to values needed to access integration environment</returns>
        public IConfigurationBuilder SelectIntegration()
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkEnvironment.Integration);
        }

        /// <summary>
        /// Returns a <see cref="IConfigurationBuilder" /> with properties set to values needed to access production environment
        /// </summary>
        /// <returns>A <see cref="IConfigurationBuilder" /> with properties set to values needed to access production environment</returns>
        public IConfigurationBuilder SelectProduction()
        {
            return new ConfigurationBuilder(_accessToken, _sectionProvider, SdkEnvironment.Production);
        }

        /// <summary>
        /// Returns a <see cref="IReplayConfigurationBuilder" /> with properties set to values needed to access replay server
        /// </summary>
        /// <returns>A <see cref="IReplayConfigurationBuilder" /> with properties set to values needed to access replay server</returns>
        public IReplayConfigurationBuilder SelectReplay()
        {
            return new ReplayConfigurationBuilder(_accessToken, _sectionProvider);
        }

        /// <summary>
        /// Returns a <see cref="ICustomConfigurationBuilder" /> allowing the properties to be set to custom values (useful for testing with non-standard AMQP)
        /// </summary>
        /// <returns>A <see cref="ICustomConfigurationBuilder" /> with properties set to values needed to access replay server</returns>
        public ICustomConfigurationBuilder SelectCustom()
        {
            return new CustomConfigurationBuilder(_accessToken, _sectionProvider);
        }
    }
}