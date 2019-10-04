/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    [Obsolete]
    internal class OddsFeedConfigurationBuilder : IConfigurationAccessTokenSetter, IConfigurationInactivitySecondsSetter, IOddsFeedConfigurationBuilder
    {
        /// <summary>
        /// The <see cref="OddsFeedConfigurationSection"/> containing data read from the config file
        /// </summary>
        private readonly IOddsFeedConfigurationSection _section;

        /// <summary>
        /// The access token used for authentication
        /// </summary>
        private string _accessToken;

        /// <summary>
        /// Specifies the max time window between two messages before the producer is market as "down"
        /// </summary>
        private int _inactivitySeconds = SdkInfo.MinInactivitySeconds;

        /// <summary>
        /// A <see cref="HashSet{T}"/> representing default cultures / languages
        /// </summary>
        private readonly HashSet<CultureInfo> _locales = new HashSet<CultureInfo>();

        /// <summary>
        /// The URL of the API host
        /// </summary>
        private string _apiHost;

        /// <summary>
        /// The URL of the messaging host
        /// </summary>
        private string _host;

        /// <summary>
        /// The name of the virtual host configured on messaging server
        /// </summary>
        private string _virtualHost;

        /// <summary>
        /// Value indicating whether a secure connection to the messaging host is required
        /// </summary>
        private bool _useSsl;

        /// <summary>
        /// The maximum recovery time
        /// </summary>
        private int _maxRecoveryTime;

        /// <summary>
        /// Value indicating whether the SDK should connect to integration environment
        /// </summary>
        private bool _useIntegrationEnvironment;

        /// <summary>
        /// The node identifier
        /// </summary>
        private int _nodeId;

        /// <summary>
        /// Initializes a new instance of the <see cref="OddsFeedConfigurationBuilder"/> class
        /// </summary>
        /// <param name="sectionProvider">A <see cref="IConfigurationSectionProvider"/> used to retrieve settings from config file</param>
        public OddsFeedConfigurationBuilder(IConfigurationSectionProvider sectionProvider)
        {
            _section = sectionProvider?.GetSection();
            Init();
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_locales != null);
            Contract.Invariant(_inactivitySeconds >= SdkInfo.MinInactivitySeconds && _inactivitySeconds <= SdkInfo.MaxInactivitySeconds);
        }

        /// <summary>
        /// Sets local fields to values read from the <see cref="OddsFeedConfigurationSection"/>
        /// </summary>
        private void Init()
        {
            if (_section == null)
            {
                _inactivitySeconds = SdkInfo.MinInactivitySeconds;
                _useSsl = true;
                _maxRecoveryTime = SdkInfo.MaxRecoveryExecutionInSeconds;
                _useIntegrationEnvironment = false;
                _nodeId = 0;
                return;
            }

            _accessToken = _section.AccessToken;
            _inactivitySeconds = _section.InactivitySeconds;
            _apiHost = _section.ApiHost;
            _host = _section.Host;
            _virtualHost = _section.VirtualHost;
            _useSsl = _section.UseSSL;
            _maxRecoveryTime = _section.MaxRecoveryTime;

            _locales.Clear();
            if (!string.IsNullOrEmpty(_section?.SupportedLanguages))
            {
                foreach (var langCode in _section.SupportedLanguages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    _locales.Add(new CultureInfo(langCode.Trim()));
                }
            }
            _useIntegrationEnvironment = _section.UseStagingEnvironment || _section.UseIntegrationEnvironment;
            _nodeId = _section.NodeId;
        }


        /// <summary>
        /// Sets the access token.
        /// </summary>
        /// <param name="accessToken">The access token</param>
        /// <returns>OddsFeedConfigurationBuilder</returns>
        /// <exception cref="System.ArgumentException">Value cannot be a null reference or empty string</exception>
        public IConfigurationInactivitySecondsSetter SetAccessToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Value cannot be a null reference or empty string", nameof(accessToken));
            }
            _accessToken = accessToken;
            return this;
        }

        /// <summary>
        /// Sets the inactivity seconds.
        /// </summary>
        /// <param name="inactivitySeconds">The inactivity seconds</param>
        /// <returns>OddsFeedConfigurationBuilder</returns>
        /// <exception cref="ArgumentOutOfRangeException">inactivitySeconds &lt; 20 or inactivitySeconds &gt; 180</exception>
        public IOddsFeedConfigurationBuilder SetInactivitySeconds(int inactivitySeconds)
        {
            if (inactivitySeconds < SdkInfo.MinInactivitySeconds || inactivitySeconds > SdkInfo.MaxInactivitySeconds)
            {
                throw new ArgumentOutOfRangeException(nameof(inactivitySeconds), $"Value must be between {SdkInfo.MinInactivitySeconds} and {SdkInfo.MaxInactivitySeconds}");
            }
            _inactivitySeconds = inactivitySeconds;
            return this;
        }


        /// <summary>
        /// Adds a provided <see cref="CultureInfo"/> into the list of default locales
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> representing the locale</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values</returns>
        /// <exception cref="ArgumentNullException">A <code>culture</code> is a null reference</exception>
        public IOddsFeedConfigurationBuilder AddLocale(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            _locales.Add(culture);
            return this;
        }

        /// <summary>
        /// Removes the specified <see cref="CultureInfo" /> from the list of default locales
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> representing the locale to be removed</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder" /> instance used to set additional values</returns>
        /// <exception cref="ArgumentNullException">The <code>culture</code> is a null reference</exception>
        public IOddsFeedConfigurationBuilder RemoveLocale(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            _locales.Remove(culture);
            return this;
        }

        /// <summary>
        /// Sets the API host.
        /// </summary>
        /// <param name="apiHost">The API host</param>
        /// <returns>OddsFeedConfigurationBuilder</returns>
        /// <exception cref="System.ArgumentException"><code>apiHost</code> is a a null reference or an empty string</exception>
        public IOddsFeedConfigurationBuilder SetApiHost(string apiHost)
        {
            _apiHost = apiHost;
            return this;
        }

        /// <summary>
        /// Sets the URL of the messaging host
        /// </summary>
        /// <param name="host">The URL of the messaging host</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder" /> instance used to set additional values</returns>
        /// <exception cref="System.ArgumentException"><code>host</code> is a null reference or an empty string</exception>
        public IOddsFeedConfigurationBuilder SetHost(string host)
        {
            _host = host;
            return this;
        }

        /// <summary>
        /// Sets the name of the virtual host configured on the messaging server.
        /// </summary>
        /// <param name="virtualHost">The name virtual host</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder" /> instance used to set additional values</returns>
        /// <exception cref="System.ArgumentException">Value cannot be a null reference or an empty string</exception>
        public IOddsFeedConfigurationBuilder SetVirtualHost(string virtualHost)
        {
            _virtualHost = virtualHost;
            return this;
        }

        /// <summary>
        /// Sets the value indicating whether a secure connection to the message broker should be used
        /// </summary>
        /// <param name="useSsl">True if secure connection should be used; False otherwise</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder" /> instance used to set additional values</returns>
        public IOddsFeedConfigurationBuilder SetUseSsl(bool useSsl)
        {
            _useSsl = useSsl;
            return this;
        }

        /// <summary>
        /// Sets the maximum time in seconds in which recovery must be completed (minimum 900 seconds)
        /// </summary>
        /// <param name="maxRecoveryTime">Maximum recovery time</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder" /> instance used to set additional values</returns>
        public IOddsFeedConfigurationBuilder SetMaxRecoveryTime(int maxRecoveryTime)
        {
            if (maxRecoveryTime < SdkInfo.MinRecoveryExecutionInSeconds)
            {
                throw new ArgumentException($"maxRecoveryTime must be greater than {SdkInfo.MinRecoveryExecutionInSeconds}", nameof(maxRecoveryTime));
            }
            _maxRecoveryTime = maxRecoveryTime;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the SDK should connect to the integration environment
        /// </summary>
        /// <param name="useStagingEnvironment"></param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values</returns>
        [Obsolete("Use SetUseIntegrationEnvironment(bool useIntegrationEnvironment)")]
        public IOddsFeedConfigurationBuilder SetUseStagingEnvironment(bool useStagingEnvironment)
        {
            _useIntegrationEnvironment = useStagingEnvironment;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether the SDK should connect to the integration environment
        /// </summary>
        /// <param name="useIntegrationEnvironment"></param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values</returns>
        public IOddsFeedConfigurationBuilder SetUseIntegrationEnvironment(bool useIntegrationEnvironment)
        {
            _useIntegrationEnvironment = useIntegrationEnvironment;
            return this;
        }

        /// <summary>
        /// Sets the node id for this instance of the sdk
        /// </summary>
        /// <param name="nodeId">The node id</param>
        /// <returns>The <see cref="IOddsFeedConfigurationBuilder"/> instance used to set additional values</returns>
        public IOddsFeedConfigurationBuilder SetNodeId(int nodeId)
        {
            _nodeId = nodeId;
            return this;
        }

        /// <summary>
        /// Builds and returns a <see cref="IOddsFeedConfiguration"/> instance
        /// </summary>
        /// <returns>The constructed <see cref="IOddsFeedConfiguration"/> instance</returns>
        public IOddsFeedConfiguration Build()
        {
            if (!_locales.Any())
            {
                throw new InvalidOperationException("At least one locale must be present in the default locales");
            }

            var apiHost = _apiHost;
            if (string.IsNullOrEmpty(apiHost))
            {
                apiHost = _useIntegrationEnvironment ? SdkInfo.IntegrationApiHost : SdkInfo.ProductionApiHost;
            }

            var host = _host;
            if (string.IsNullOrEmpty(host))
            {
                host = _useIntegrationEnvironment ? SdkInfo.IntegrationHost : SdkInfo.ProductionHost;
            }
            var config = new OddsFeedConfiguration(
                _accessToken,
                _inactivitySeconds,
                _locales.ToList(),
                _section?.DisabledProducers?.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(value => int.Parse(value.Trim())).ToList(),
                apiHost,
                host,
                _virtualHost,
                _useSsl,
                _maxRecoveryTime,
                _useIntegrationEnvironment,
                _nodeId,
                _section?.ExceptionHandlingStrategy ?? ExceptionHandlingStrategy.CATCH,
                false,
                _section);

            Init();
            return config;
        }
    }
}
