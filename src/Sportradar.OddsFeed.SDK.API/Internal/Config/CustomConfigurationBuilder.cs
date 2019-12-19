/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal.Config
{
    /// <summary>
    /// Class CustomConfigurationBuilder
    /// </summary>
    /// <seealso cref="RecoveryConfigurationBuilder{ICustomConfigurationBuilder}" />
    /// <seealso cref="ICustomConfigurationBuilder" />
    internal class CustomConfigurationBuilder : RecoveryConfigurationBuilder<ICustomConfigurationBuilder>, ICustomConfigurationBuilder
    {
        /// <summary>
        /// Specifies the host name of the message broker
        /// </summary>
        private string _messagingHost;

        /// <summary>
        /// Specifies the port of the message broker
        /// </summary>
        private int? _messagingPort;

        /// <summary>
        /// Specifies the user name used for authentication with messaging broker
        /// </summary>
        private string _messagingUsername;

        /// <summary>
        /// Specifies the password used for authentication with messaging broker
        /// </summary>
        private string _messagingPassword;

        /// <summary>
        /// Specifies the virtual host of the messaging broker
        /// </summary>
        private string _virtualHost;

        /// <summary>
        /// The host name of the Sports API
        /// </summary>
        private string _apiHost;

        /// <summary>
        /// Value specifying whether SSL should be used when connecting to messaging broker
        /// </summary>
        private bool _useMessagingSsl;

        /// <summary>
        /// Value specifying whether SSL should be used when connecting to Sports API
        /// </summary>
        private bool _useApiSsl;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomConfigurationBuilder"/> class.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="sectionProvider"></param>
        internal CustomConfigurationBuilder(string accessToken, IConfigurationSectionProvider sectionProvider)
            : base(accessToken, sectionProvider)
        {
            //_messagingHost = SdkInfo.IntegrationHost;
            //_messagingPort = SdkInfo.DefaultHostPort;
            //_apiHost = SdkInfo.IntegrationApiHost;
            _useMessagingSsl = true;
            _useApiSsl = true;
        }

        /// <summary>
        /// Check the properties values before build the configuration and throws an exception is invalid values are found
        /// </summary>
        /// <exception cref="InvalidOperationException">The value of one or more properties is not correct</exception>

        protected override void PreBuildCheck()
        {
            base.PreBuildCheck();
            if (string.IsNullOrEmpty(_messagingHost))
            {
                throw new InvalidOperationException("MessagingHost is missing");
            }
            if (_messagingHost.ToLower().StartsWith("http://") || _messagingHost.ToLower().StartsWith("https://"))
            {
                throw new InvalidOperationException($"messagingHost must not contain protocol specification. Value={_messagingHost}");
            }
            if (string.IsNullOrEmpty(_apiHost))
            {
                throw new InvalidOperationException("ApiHost is missing");
            }
            if (_apiHost.ToLower().StartsWith("http://") || _apiHost.ToLower().StartsWith("https://"))
            {
                throw new InvalidOperationException($"apiHost must not contain protocol specification. Value={_apiHost}");
            }
        }

        /// <summary>
        /// Sets the host name of the AMQP server
        /// </summary>
        /// <param name="host">The host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingHost(string host)
        {
            Guard.Argument(host).NotNull().NotEmpty();

            _messagingHost = host;
            return this;
        }

        /// <summary>
        /// Sets the port used to connect to the AMQP server
        /// </summary>
        /// <param name="port">The port used to connect to the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingPort(int port)
        {
            _messagingPort = port;
            return this;
        }

        /// <summary>
        /// Sets the username used to authenticate with the messaging server
        /// </summary>
        /// <param name="username">The username used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingUsername(string username)
        {
            Guard.Argument(username).NotNull().NotEmpty();

            _messagingUsername = username;
            return this;
        }

        /// <summary>
        /// Sets the password used to authenticate with the messaging server
        /// </summary>
        /// <param name="password">The password used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingPassword(string password)
        {
            Guard.Argument(password).NotNull().NotEmpty();

            _messagingPassword = password;
            return this;
        }

        /// <summary>
        /// Sets the virtual host name of the AMQP server
        /// </summary>
        /// <param name="virtualHost">The virtual host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetVirtualHost(string virtualHost)
        {
            Guard.Argument(virtualHost).NotNull().NotEmpty();

            _virtualHost = virtualHost;
            return this;
        }

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with the messaging server
        /// </summary>
        /// <param name="useMessagingSsl">The value specifying whether SSL should be used to communicate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder UseMessagingSsl(bool useMessagingSsl)
        {
            _useMessagingSsl = useMessagingSsl;
            return this;
        }

        /// <summary>
        /// Set the host name of the Sports API server
        /// </summary>
        /// <param name="apiHost">The host name of the Sports API server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetApiHost(string apiHost)
        {
            Guard.Argument(apiHost).NotNull().NotEmpty();

            _apiHost = apiHost;
            return this;
        }

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with Sports API
        /// </summary>
        /// <param name="useApiSsl">The value specifying whether SSL should be used to communicate with Sports API</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder UseApiSsl(bool useApiSsl)
        {
            _useApiSsl = useApiSsl;
            return this;
        }

        /// <summary>
        /// Sets the custom configuration properties to values read from configuration file. Only value which can be set
        /// through <see cref="ICustomConfigurationBuilder" /> methods are set.
        /// Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <param name="section">A <see cref="IOddsFeedConfigurationSection"/> from which to load the config</param>
        /// <returns>T.</returns>
        internal override void LoadFromConfigFile(IOddsFeedConfigurationSection section)
        {
            base.LoadFromConfigFile(section);

            if (!string.IsNullOrEmpty(section.Host))
            {
                _messagingHost = section.Host;
            }
            if (section.Port > 0)
            {
                _messagingPort = section.Port;
            }
            if (!string.IsNullOrEmpty(section.Username))
            {
                _messagingUsername = section.Username;
            }
            if (!string.IsNullOrEmpty(section.Username))
            {
                _messagingPassword = section.Password;
            }
            if (!string.IsNullOrEmpty(section.Username))
            {
                _virtualHost = section.VirtualHost;
            }
            if (!string.IsNullOrEmpty(section.Username))
            {
                _apiHost = section.ApiHost;
            }
            _useMessagingSsl = section.UseSSL;
            _useApiSsl = section.UseApiSSL;
        }

        /// <summary>
        /// Builds and returns a <see cref="IOddsFeedConfiguration" /> instance
        /// </summary>
        /// <returns>The constructed <see cref="IOddsFeedConfiguration" /> instance</returns>
        public override IOddsFeedConfiguration Build()
        {
            PreBuildCheck();

            var config = new OddsFeedConfiguration(AccessToken,
                                                   SdkEnvironment.Custom,
                                                   DefaultLocale,
                                                   SupportedLocales,
                                                   _messagingHost,
                                                   _virtualHost,
                                                   _messagingPort ?? (_useMessagingSsl ? SdkInfo.DefaultHostPort : SdkInfo.DefaultHostPort + 1),
                                                   string.IsNullOrEmpty(_messagingUsername) ? AccessToken : _messagingUsername,
                                                   _messagingPassword,
                                                   _apiHost,
                                                   _useMessagingSsl,
                                                   _useApiSsl,
                                                   InactivitySeconds ?? SdkInfo.MinInactivitySeconds,
                                                   MaxRecoveryTimeInSeconds ?? SdkInfo.MaxRecoveryExecutionInSeconds,
                                                   NodeId,
                                                   DisabledProducers,
                                                   ExceptionHandlingStrategy,
                                                   AdjustAfterAge ?? false,
                                                   Section);

            return config;
        }
    }
}
