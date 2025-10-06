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

        private readonly IBookmakerDetailsProvider _bookmakerDetailsProvider;
        private readonly IProducersProvider _producersProvider;

        private readonly UofConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenSetter"/> class
        /// </summary>
        /// <param name="uofConfigurationSectionProvider">A <see cref="IUofConfigurationSectionProvider"/> instance used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProvider">Provider for bookmaker details</param>
        /// <param name="producersProvider">Provider for available producers</param>
        internal TokenSetter(IUofConfigurationSectionProvider uofConfigurationSectionProvider, IBookmakerDetailsProvider bookmakerDetailsProvider, IProducersProvider producersProvider)
        {
            Guard.Argument(uofConfigurationSectionProvider, nameof(uofConfigurationSectionProvider)).NotNull();

            _uofConfigurationSectionProvider = uofConfigurationSectionProvider;
            _bookmakerDetailsProvider = bookmakerDetailsProvider;
            _producersProvider = producersProvider;
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
            return new EnvironmentSelector(_configuration, _uofConfigurationSectionProvider, _bookmakerDetailsProvider, _producersProvider);
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

            return new EnvironmentSelector(_configuration, _uofConfigurationSectionProvider, _bookmakerDetailsProvider, _producersProvider)
                  .SelectEnvironment(_configuration.Environment)
                  .Build();
        }
    }
}
