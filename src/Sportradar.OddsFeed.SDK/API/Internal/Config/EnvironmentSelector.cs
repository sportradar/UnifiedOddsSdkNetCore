// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Class EnvironmentSelector
    /// </summary>
    /// <seealso cref="IEnvironmentSelector" />
    internal class EnvironmentSelector : IEnvironmentSelector
    {
        private readonly IUofConfigurationSectionProvider _sectionProvider;

        private readonly Func<IUofConfiguration, IBookmakerDetailsProvider> _bookmakerDetailsProviderFactory;

        private readonly Func<IUofConfiguration, IProducersProvider> _producersProviderFactory;

        private readonly UofClientAuthentication.IPrivateKeyJwtData _privateKeyJwtData;

        private readonly UofConfiguration _configuration;

        /// <summary>
        /// Constructs a new instance of the <see cref="EnvironmentSelector"/> class
        /// </summary>
        /// <param name="configuration">Current <see cref="UofConfiguration"/></param>
        /// <param name="sectionProvider">A <see cref="IUofConfigurationSectionProvider"/> used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProviderFactory">Factory to create the bookmaker details provider</param>
        /// <param name="producersProviderFactory">Factory to create the producers provider</param>
        /// <param name="privateKeyJwtData">JWT private key data.</param>
        internal EnvironmentSelector(UofConfiguration configuration,
                                     IUofConfigurationSectionProvider sectionProvider,
                                     Func<IUofConfiguration, IBookmakerDetailsProvider> bookmakerDetailsProviderFactory,
                                     Func<IUofConfiguration, IProducersProvider> producersProviderFactory,
                                     UofClientAuthentication.IPrivateKeyJwtData privateKeyJwtData = null)
        {
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            Guard.Argument(sectionProvider, nameof(sectionProvider)).NotNull();

            _configuration = configuration;
            _sectionProvider = sectionProvider;
            _bookmakerDetailsProviderFactory = bookmakerDetailsProviderFactory;
            _producersProviderFactory = producersProviderFactory;
            _privateKeyJwtData = privateKeyJwtData;
        }

        public ICustomConfigurationBuilder SelectCustom()
        {
            _configuration.Authentication = (PrivateKeyJwt)_privateKeyJwtData;
            return new CustomConfigurationBuilder(_configuration, _sectionProvider, _bookmakerDetailsProviderFactory, _producersProviderFactory);
        }

        public IConfigurationBuilder SelectEnvironment(SdkEnvironment ufEnvironment)
        {
            if (ufEnvironment == SdkEnvironment.Custom)
            {
                throw new InvalidOperationException("Use SelectCustom() for custom environment.");
            }

            _configuration.Authentication = _privateKeyJwtData == null
                                                ? null
                                                : ConfigureAuthenticationHostForEnvironment(ufEnvironment);

            return new ConfigurationBuilder(_configuration, _sectionProvider, ufEnvironment, _bookmakerDetailsProviderFactory, _producersProviderFactory);

        }

        public IConfigurationBuilder SelectEnvironmentFromConfigFile()
        {
            var section = _sectionProvider.GetSection();
            if (section == null)
            {
                throw new InvalidOperationException("Missing configuration section");
            }
            return SelectEnvironment(section.Environment);
        }

        private PrivateKeyJwt ConfigureAuthenticationHostForEnvironment(SdkEnvironment ufEnvironment)
        {
            var privateKeyJwt = new PrivateKeyJwt(_privateKeyJwtData.SigningKeyId, _privateKeyJwtData.ClientId, _privateKeyJwtData.PrivateKey);
            privateKeyJwt.SetHost(EnvironmentManager.GetAuthenticationHost(ufEnvironment));
            privateKeyJwt.SetPort(EnvironmentManager.DefaultApiSslPort);
            privateKeyJwt.SetUseSsl(true);
            return privateKeyJwt;
        }
    }
}
