/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal.Config
{
    /// <summary>
    /// Class TokenSetter
    /// </summary>
    /// <seealso cref="ITokenSetter" />
    internal class TokenSetter : ITokenSetter
    {
        /// <summary>
        /// A <see cref="IConfigurationSectionProvider"/> instance used to access <see cref="IOddsFeedConfigurationSection"/>
        /// </summary>
        private readonly IConfigurationSectionProvider _configurationSectionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenSetter"/> class
        /// </summary>
        /// <param name="configurationSectionProvider">A <see cref="IConfigurationSectionProvider"/> instance used to access <see cref="IOddsFeedConfigurationSection"/></param>
        internal TokenSetter(IConfigurationSectionProvider configurationSectionProvider)
        {
            Guard.Argument(configurationSectionProvider, nameof(configurationSectionProvider)).NotNull();

            _configurationSectionProvider = configurationSectionProvider;
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
            return new EnvironmentSelector(accessToken, _configurationSectionProvider);
        }

        /// <summary>
        /// Sets the access token used to access feed resources (AMQP broker, Sports API, ...) to value read from configuration file
        /// </summary>
        /// <returns>The <see cref="IEnvironmentSelector" /> instance allowing the selection of target environment</returns>
        public IEnvironmentSelector SetAccessTokenFromConfigFile()
        {
            return new EnvironmentSelector(_configurationSectionProvider.GetSection().AccessToken, _configurationSectionProvider);
        }

        /// <inheritdoc />
        public IOddsFeedConfiguration BuildFromConfigFile()
        {
            var section = _configurationSectionProvider.GetSection();
            if (string.IsNullOrEmpty(section.AccessToken))
            {
                throw new ConfigurationErrorsException("Missing access token");
            }

            var sdkEnvironment = SdkEnvironment.GlobalIntegration;
            if (section.UfEnvironment != null)
            {
                sdkEnvironment = section.UfEnvironment.Value;
            }
            else if (!section.UseIntegrationEnvironment)
            {
                sdkEnvironment = SdkEnvironment.Production;
            }
                
            var supportedLanguages = new List<CultureInfo>();
            if (!string.IsNullOrEmpty(section.SupportedLanguages))
            {
                var langCodes = section.SupportedLanguages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                supportedLanguages = langCodes.Select(langCode => new CultureInfo(langCode.Trim())).ToList();
            }

            var defaultLanguage = supportedLanguages.Any() ? supportedLanguages.First() : null;
            if (!string.IsNullOrEmpty(section.DefaultLanguage))
            {
                defaultLanguage = new CultureInfo(section.DefaultLanguage);
                if (!supportedLanguages.Contains(defaultLanguage))
                {
                    supportedLanguages.Insert(0, defaultLanguage);
                }
            }

            if (supportedLanguages == null || !supportedLanguages.Any())
            {
                throw new InvalidOperationException("Missing supported languages");
            }

            var disabledProducers = new List<int>();
            if (!string.IsNullOrEmpty(section.DisabledProducers))
            {
                var producerIds = section.DisabledProducers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                disabledProducers = producerIds.Select(producerId => int.Parse(producerId.Trim())).ToList();
            }

             var mqHost = string.IsNullOrEmpty(section.Host)
                             ? EnvironmentManager.GetMqHost(sdkEnvironment)
                             : section.Host;
            var apiHost = string.IsNullOrEmpty(section.ApiHost)
                             ? EnvironmentManager.GetApiHost(sdkEnvironment)
                             : section.ApiHost;

            var config = new OddsFeedConfiguration(section.AccessToken,
                                                   sdkEnvironment,
                                                   defaultLanguage,
                                                   supportedLanguages,
                                                   mqHost,
                                                   section.VirtualHost,
                                                   EnvironmentManager.DefaultMqHostPort,
                                                   section.Username,
                                                   section.Password,
                                                   apiHost,
                                                   section.UseSSL,
                                                   section.UseApiSSL,
                                                   section.InactivitySeconds > 0 ? section.InactivitySeconds : SdkInfo.MinInactivitySeconds,
                                                   section.MaxRecoveryTime > 0 ? section.MaxRecoveryTime : SdkInfo.MaxRecoveryExecutionInSeconds,
                                                   section.MinIntervalBetweenRecoveryRequests > 0 ? section.MinIntervalBetweenRecoveryRequests : SdkInfo.DefaultIntervalBetweenRecoveryRequests,
                                                   section.NodeId,
                                                   disabledProducers,
                                                   section.ExceptionHandlingStrategy,
                                                   section.AdjustAfterAge,
                                                   section.HttpClientTimeout != SdkInfo.DefaultHttpClientTimeout ? section.HttpClientTimeout : SdkInfo.DefaultHttpClientTimeout,
                                                   section.RecoveryHttpClientTimeout != SdkInfo.DefaultHttpClientTimeout ? section.RecoveryHttpClientTimeout : SdkInfo.DefaultHttpClientTimeout,
                                                   section);

            return config;
        }
    }
}
