// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Class CustomConfigurationBuilder
    /// </summary>
    /// <seealso cref="RecoveryConfigurationBuilder{ICustomConfigurationBuilder}" />
    /// <seealso cref="ICustomConfigurationBuilder" />
    internal class CustomConfigurationBuilder : RecoveryConfigurationBuilder<ICustomConfigurationBuilder>, ICustomConfigurationBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomConfigurationBuilder"/> class.
        /// </summary>
        /// <param name="configuration">Current <see cref="UofConfiguration"/></param>
        /// <param name="sectionProvider">A <see cref="IUofConfigurationSectionProvider"/> used to access <see cref="IUofConfigurationSection"/></param>
        /// <param name="bookmakerDetailsProvider">Provider for bookmaker details</param>
        /// <param name="producersProvider">Provider for available producers</param>
        internal CustomConfigurationBuilder(UofConfiguration configuration,
            IUofConfigurationSectionProvider sectionProvider,
            IBookmakerDetailsProvider bookmakerDetailsProvider,
            IProducersProvider producersProvider)
            : base(configuration, sectionProvider, bookmakerDetailsProvider, producersProvider)
        {
            //populate connection settings and then override only needed
            UofConfiguration.UpdateSdkEnvironment(SdkEnvironment.Integration);

            UofConfiguration.UpdateSdkEnvironment(SdkEnvironment.Custom);

            UseMessagingSsl(true);
            UseApiSsl(true);
        }

        /// <summary>
        /// Sets the host name of the AMQP server
        /// </summary>
        /// <param name="host">The host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingHost(string host)
        {
            Guard.Argument(host, nameof(host)).NotNull().NotEmpty();

            var rabbitConfig = (UofRabbitConfiguration)UofConfiguration.Rabbit;
            rabbitConfig.Host = host;
            UofConfiguration.Rabbit = rabbitConfig;
            return this;
        }

        /// <summary>
        /// Sets the port used to connect to the AMQP server
        /// </summary>
        /// <param name="port">The port used to connect to the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingPort(int port)
        {
            var rabbitConfig = (UofRabbitConfiguration)UofConfiguration.Rabbit;
            rabbitConfig.Port = port;
            UofConfiguration.Rabbit = rabbitConfig;
            return this;
        }

        /// <summary>
        /// Sets the username used to authenticate with the messaging server
        /// </summary>
        /// <param name="username">The username used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingUsername(string username)
        {
            Guard.Argument(username, nameof(username)).NotNull().NotEmpty();

            var rabbitConfig = (UofRabbitConfiguration)UofConfiguration.Rabbit;
            rabbitConfig.Username = username;
            UofConfiguration.Rabbit = rabbitConfig;
            return this;
        }

        /// <summary>
        /// Sets the password used to authenticate with the messaging server
        /// </summary>
        /// <param name="password">The password used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetMessagingPassword(string password)
        {
            Guard.Argument(password, nameof(password)).NotNull().NotEmpty();

            var rabbitConfig = (UofRabbitConfiguration)UofConfiguration.Rabbit;
            rabbitConfig.Password = password;
            UofConfiguration.Rabbit = rabbitConfig;
            return this;
        }

        /// <summary>
        /// Sets the virtual host name of the AMQP server
        /// </summary>
        /// <param name="virtualHost">The virtual host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetVirtualHost(string virtualHost)
        {
            Guard.Argument(virtualHost, nameof(virtualHost)).NotNull().NotEmpty();

            var rabbitConfig = (UofRabbitConfiguration)UofConfiguration.Rabbit;
            rabbitConfig.VirtualHost = virtualHost;
            UofConfiguration.Rabbit = rabbitConfig;
            return this;
        }

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with the messaging server
        /// </summary>
        /// <param name="useMessagingSsl">The value specifying whether SSL should be used to communicate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder UseMessagingSsl(bool useMessagingSsl)
        {
            var rabbitConfig = (UofRabbitConfiguration)UofConfiguration.Rabbit;
            rabbitConfig.UseSsl = useMessagingSsl;
            UofConfiguration.Rabbit = rabbitConfig;
            return this;
        }

        /// <summary>
        /// Set the host name of the Sports API server
        /// </summary>
        /// <param name="apiHost">The host name of the Sports API server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder SetApiHost(string apiHost)
        {
            Guard.Argument(apiHost, nameof(apiHost)).NotNull().NotEmpty();

            var apiConfig = (UofApiConfiguration)UofConfiguration.Api;
            apiConfig.Host = apiHost;
            UofConfiguration.Api = apiConfig;
            return this;
        }

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with Sports API
        /// </summary>
        /// <param name="useApiSsl">The value specifying whether SSL should be used to communicate with Sports API</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder" /> instance used to set custom config values</returns>
        public ICustomConfigurationBuilder UseApiSsl(bool useApiSsl)
        {
            var apiConfig = (UofApiConfiguration)UofConfiguration.Api;
            apiConfig.UseSsl = useApiSsl;
            UofConfiguration.Api = apiConfig;
            return this;
        }

        /// <summary>
        /// Sets the custom configuration properties to values read from configuration file. Only value which can be set
        /// through <see cref="ICustomConfigurationBuilder" /> methods are set.
        /// Any values already set by methods on the current instance are overridden
        /// </summary>
        /// <returns>T.</returns>
        public override ICustomConfigurationBuilder LoadFromConfigFile()
        {
            var section = SectionProvider.GetSection();
            base.LoadFromConfigFile();

            if (!string.IsNullOrEmpty(section.RabbitHost))
            {
                SetMessagingHost(section.RabbitHost);
            }
            if (section.RabbitPort > 0)
            {
                SetMessagingPort(section.RabbitPort);
            }
            if (!string.IsNullOrEmpty(section.RabbitUsername))
            {
                SetMessagingUsername(section.RabbitUsername);
            }
            if (!string.IsNullOrEmpty(section.RabbitPassword))
            {
                SetMessagingPassword(section.RabbitPassword);
            }
            if (!string.IsNullOrEmpty(section.RabbitVirtualHost))
            {
                SetVirtualHost(section.RabbitVirtualHost);
            }
            if (!string.IsNullOrEmpty(section.ApiHost))
            {
                SetApiHost(section.ApiHost);
            }
            UseMessagingSsl(section.RabbitUseSsl);
            UseApiSsl(section.ApiUseSsl);

            return this;
        }

        /// <summary>
        /// Check the properties values before build the configuration and throws an exception is invalid values are found
        /// </summary>
        /// <exception cref="InvalidOperationException">The value of one or more properties is not correct</exception>
        protected override void PreBuildCheck()
        {
            base.PreBuildCheck();

            if (string.IsNullOrEmpty(UofConfiguration.Rabbit.Host))
            {
                throw new InvalidOperationException("MessagingHost is missing");
            }
            if (UofConfiguration.Rabbit.Host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || UofConfiguration.Rabbit.Host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"messagingHost must not contain protocol specification. Value={UofConfiguration.Rabbit.Host}");
            }
            if (string.IsNullOrEmpty(UofConfiguration.Api.Host))
            {
                throw new InvalidOperationException("ApiHost is missing");
            }
            if (UofConfiguration.Api.Host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || UofConfiguration.Api.Host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"apiHost must not contain protocol specification. Value={UofConfiguration.Api.Host}");
            }
        }

        /// <summary>
        /// Builds and returns a <see cref="IUofConfiguration" /> instance
        /// </summary>
        /// <returns>The constructed <see cref="IUofConfiguration" /> instance</returns>
        public override IUofConfiguration Build()
        {
            PreBuildCheck();

            UofConfiguration.Environment = SdkEnvironment.Custom;

            FetchBookmakerDetails();

            FetchProducers();

            return UofConfiguration;
        }
    }
}
