// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Class TokenSetter
    /// </summary>
    /// <seealso cref="ITokenSetter" />
    internal class TokenSetter : ITokenSetter
    {
        /// <summary>
        /// A <see cref="IUofConfigurationSectionProvider"/> instance used to access <see cref="IUofConfigurationSection"/>
        /// </summary>
        private readonly IUofConfigurationSectionProvider _uofConfigurationSectionProvider;

        private readonly Func<IUofConfiguration, IBookmakerDetailsProvider> _bookmakerDetailsProviderFactory;
        private readonly Func<IUofConfiguration, IProducersProvider> _producersProviderFactory;

        private readonly UofConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenSetter"/> class
        /// </summary>
        /// <param name="uofConfigurationSectionProvider">A <see cref="IUofConfigurationSectionProvider"/> instance used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProviderFactory">Factory to create the bookmaker details provider</param>
        /// <param name="producersProviderFactory">Factory to create the producers provider</param>
        internal TokenSetter(IUofConfigurationSectionProvider uofConfigurationSectionProvider,
                             Func<IUofConfiguration, IBookmakerDetailsProvider> bookmakerDetailsProviderFactory,
                             Func<IUofConfiguration, IProducersProvider> producersProviderFactory)
        {
            Guard.Argument(uofConfigurationSectionProvider, nameof(uofConfigurationSectionProvider)).NotNull();

            _uofConfigurationSectionProvider = uofConfigurationSectionProvider;
            _bookmakerDetailsProviderFactory = bookmakerDetailsProviderFactory;
            _producersProviderFactory = producersProviderFactory;
            _configuration = new UofConfiguration(_uofConfigurationSectionProvider);
        }

        /// <summary>
        /// Sets the access token used to access feed resources (AMQP broker, Sports API, ...)
        /// </summary>
        /// <param name="accessToken">The access token used to access feed resources</param>
        /// <returns>The <see cref="IEnvironmentSelector" /> instance allowing the selection of target environment</returns>
        /// <exception cref="ArgumentException">Value cannot be a null reference or empty string - accessToken</exception>
        public IEnvironmentSelector SetAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Value cannot be a null reference or empty string", nameof(accessToken));
            }
            _configuration.AccessToken = accessToken;
            return new EnvironmentSelector(_configuration, _uofConfigurationSectionProvider, _bookmakerDetailsProviderFactory, _producersProviderFactory);
        }

        /// <summary>
        /// Sets the access token used to access feed resources (AMQP broker, Sports API, ...) to value read from configuration file
        /// </summary>
        /// <returns>The <see cref="IEnvironmentSelector" /> instance allowing the selection of target environment</returns>
        public IEnvironmentSelector SetAccessTokenFromConfigFile()
        {
            var section = _uofConfigurationSectionProvider.GetSection();
            if (section == null)
            {
                throw new InvalidOperationException("Missing configuration section");
            }
            return SetAccessToken(section.AccessToken);
        }

        /// <inheritdoc />
        public IUofConfiguration BuildFromConfigFile()
        {
            if (_uofConfigurationSectionProvider.GetSection()?.Environment == SdkEnvironment.Custom)
            {
                return this.SetAccessTokenFromConfigFile().SelectCustom().LoadFromConfigFile().Build();
            }

            _configuration.UpdateFromAppConfigSection(true);

            return new EnvironmentSelector(_configuration, _uofConfigurationSectionProvider, _bookmakerDetailsProviderFactory, _producersProviderFactory)
                  .SelectEnvironment(_configuration.Environment)
                  .Build();
        }
    }
}
