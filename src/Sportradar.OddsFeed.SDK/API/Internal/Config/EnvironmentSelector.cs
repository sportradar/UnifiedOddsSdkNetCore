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

        private readonly IBookmakerDetailsProvider _bookmakerDetailsProvider;

        private readonly IProducersProvider _producersProvider;

        private readonly UofClientAuthentication.IPrivateKeyJwtData _privateKeyJwtData;

        private readonly UofConfiguration _configuration;

        /// <summary>
        /// Constructs a new instance of the <see cref="EnvironmentSelector"/> class
        /// </summary>
        /// <param name="configuration">Current <see cref="UofConfiguration"/></param>
        /// <param name="sectionProvider">A <see cref="IUofConfigurationSectionProvider"/> used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProvider">Provider for bookmaker details</param>
        /// <param name="producersProvider">Provider for available producers</param>
        /// <param name="privateKeyJwtData">JWT private key data.</param>
        // ReSharper disable once TooManyDependencies
        internal EnvironmentSelector(UofConfiguration configuration,
            IUofConfigurationSectionProvider sectionProvider,
            IBookmakerDetailsProvider bookmakerDetailsProvider,
            IProducersProvider producersProvider,
            UofClientAuthentication.IPrivateKeyJwtData privateKeyJwtData = null)
        {
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            Guard.Argument(sectionProvider, nameof(sectionProvider)).NotNull();

            _configuration = configuration;
            _sectionProvider = sectionProvider;
            _bookmakerDetailsProvider = bookmakerDetailsProvider;
            _producersProvider = producersProvider;
            _privateKeyJwtData = privateKeyJwtData;
        }

        public IConfigurationBuilder SelectReplay()
        {
            return SelectEnvironment(SdkEnvironment.Replay);
        }

        public ICustomConfigurationBuilder SelectCustom()
        {
            _configuration.Authentication = (PrivateKeyJwt)_privateKeyJwtData;
            return new CustomConfigurationBuilder(_configuration, _sectionProvider, _bookmakerDetailsProvider, _producersProvider);
        }

        public IConfigurationBuilder SelectEnvironment(SdkEnvironment ufEnvironment)
        {
            if (ufEnvironment == SdkEnvironment.Custom)
            {
                throw new InvalidOperationException("Use SelectCustom() for custom environment.");
            }

            if (ufEnvironment == SdkEnvironment.Replay && IsClientAuthenticationConfigured())
            {
                throw new InvalidOperationException("Authentication cannot be set when connecting to replay environment.");
            }

            _configuration.Authentication = _privateKeyJwtData == null
                                                ? null
                                                : ConfigureAuthenticationHostForEnvironment(ufEnvironment);

            return new ConfigurationBuilder(_configuration, _sectionProvider, ufEnvironment, _bookmakerDetailsProvider, _producersProvider);
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

        private bool IsClientAuthenticationConfigured()
        {
            return null != _privateKeyJwtData;
        }
    }
}
