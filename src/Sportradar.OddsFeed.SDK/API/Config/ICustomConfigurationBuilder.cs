// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes used to set general and custom configuration properties
    /// </summary>
    public interface ICustomConfigurationBuilder : IRecoveryConfigurationBuilder<ICustomConfigurationBuilder>
    {
        /// <summary>
        /// Set the host name of the Sports API server
        /// </summary>
        /// <param name="apiHost">The host name of the Sports API server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetApiHost(string apiHost);

        /// <summary>
        /// Sets the host name of the AMQP server
        /// </summary>
        /// <param name="host">The host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingHost(string host);

        /// <summary>
        /// Sets the virtual host name of the AMQP server
        /// </summary>
        /// <param name="virtualHost">The virtual host name of the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetVirtualHost(string virtualHost);

        /// <summary>
        /// Sets the port used to connect to the AMQP server
        /// </summary>
        /// <param name="port">The port used to connect to the AMQP server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingPort(int port);

        /// <summary>
        /// Sets the username used to authenticate with the messaging server
        /// </summary>
        /// <param name="username">The username used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingUsername(string username);

        /// <summary>
        /// Sets the password used to authenticate with the messaging server
        /// </summary>
        /// <param name="password">The password used to authenticate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder SetMessagingPassword(string password);

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with Sports API
        /// </summary>
        /// <param name="useApiSsl">The value specifying whether SSL should be used to communicate with Sports API</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder UseApiSsl(bool useApiSsl);

        /// <summary>
        /// Sets the value specifying whether SSL should be used to communicate with the messaging server
        /// </summary>
        /// <param name="useMessagingSsl">The value specifying whether SSL should be used to communicate with the messaging server</param>
        /// <returns>The <see cref="ICustomConfigurationBuilder"/> instance used to set custom config values</returns>
        ICustomConfigurationBuilder UseMessagingSsl(bool useMessagingSsl);
    }
}
