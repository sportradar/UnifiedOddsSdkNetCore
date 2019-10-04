/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Net.Security;
using System.Security.Authentication;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A <see cref="IConnectionFactory"/> implementations which properly configures it self before first <see cref="IConnection"/> is created
    /// </summary>
    internal class  ConfiguredConnectionFactory : ConnectionFactory
    {
        /// <summary>
        /// A <see cref="IOddsFeedConfigurationInternal"/> instance containing configuration information
        /// </summary>
        private readonly IOddsFeedConfigurationInternal _config;

        /// <summary>
        /// A singleton instance of <see cref="IConnection"/> class created by current factory.
        /// </summary>
        private IConnection _connectionSingleton;

        /// <summary>
        /// A <see cref="object"/> used to ensure thread safety when creating the connection singleton
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredConnectionFactory"/> class
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfigurationInternal"/> instance containing configuration information</param>
        public ConfiguredConnectionFactory(IOddsFeedConfigurationInternal config)
        {
            Contract.Requires(config != null);
            _config = config;
        }

        /// <summary>
        /// Configures the current <see cref="ConfiguredConnectionFactory"/> based on config options read from <code>_config</code> field
        /// </summary>
        protected void Configure()
        {
            Contract.Assume(_config != null);
            HostName = _config.Host;
            Port = _config.Port;
            UserName = _config.Username;
            Password = _config.Password;
            VirtualHost = _config.VirtualHost;
            AutomaticRecoveryEnabled = true;

            Contract.Assume(Ssl != null);
            Ssl.Enabled = _config.UseSsl;
            Ssl.Version = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
            if (_config.UseSsl)
            {
                Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
            }

            Contract.Assume(ClientProperties != null);
            ClientProperties.Add("SrSdkType", ".net");
            ClientProperties.Add("SrSdkVersion", SdkInfo.GetVersion());
        }

        /// <summary>
        /// Create a connection to the specified endpoint
        /// </summary>
        /// <exception cref="T:RabbitMQ.Client.Exceptions.BrokerUnreachableException">When the configured host name was not reachable</exception>
        public override IConnection CreateConnection()
        {
            lock (_syncLock)
            {
                if (_connectionSingleton == null)
                {
                    Configure();
                    _connectionSingleton = base.CreateConnection();
                }
                return _connectionSingleton;
            }
        }
    }
}
