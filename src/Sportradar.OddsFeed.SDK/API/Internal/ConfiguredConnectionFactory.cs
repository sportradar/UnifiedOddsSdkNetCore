/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using System;
using System.Net.Security;
using System.Security.Authentication;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A <see cref="IConnectionFactory"/> implementations which properly configures it self before first <see cref="IConnection"/> is created
    /// </summary>
    internal class ConfiguredConnectionFactory : ConnectionFactory, IDisposable
    {
        /// <summary>
        /// Signaled when an exception occurs in a callback invoked by the connection.
        /// </summary>
        /// <remarks>
        /// This event is signaled when a ConnectionShutdown handler throws an exception. If, in future, more events appear on <see cref="IConnection" />, then this event will be signaled whenever one of those event handlers throws an exception, as well.
        /// </remarks>
        public event EventHandler<CallbackExceptionEventArgs> CallbackException;

        public event EventHandler<EventArgs> RecoverySucceeded;

        public event EventHandler<ConnectionRecoveryErrorEventArgs> ConnectionRecoveryError;

        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;

        /// <summary>Raised when the connection is destroyed.</summary>
        /// <remarks>
        /// If the connection is already destroyed at the time an
        /// event handler is added to this event, the event handler
        /// will be fired immediately.
        /// </remarks>
        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;

        public event EventHandler<EventArgs> ConnectionUnblocked;

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
        /// Gets the connection created date
        /// </summary>
        /// <value>The connection created date</value>
        public DateTime ConnectionCreated { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredConnectionFactory"/> class
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfigurationInternal"/> instance containing configuration information</param>
        public ConfiguredConnectionFactory(IOddsFeedConfigurationInternal config)
        {
            Guard.Argument(config, nameof(config)).NotNull();

            _config = config;
            ConnectionCreated = DateTime.MinValue;
        }

        /// <summary>
        /// Configures the current <see cref="ConfiguredConnectionFactory"/> based on config options read from <code>_config</code> field
        /// </summary>
        protected void Configure()
        {
            HostName = _config.Host;
            Port = _config.Port;
            UserName = _config.Username;
            Password = _config.Password;
            VirtualHost = _config.VirtualHost;
            AutomaticRecoveryEnabled = true;
            RequestedHeartbeat = (ushort)OperationManager.RabbitHeartbeat;
            RequestedConnectionTimeout = OperationManager.RabbitConnectionTimeout * 1000;

            Ssl.Enabled = _config.UseSsl;
            Ssl.Version = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
            if (_config.UseSsl)
            {
                Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
            }

            ClientProperties.Add("SrUfSdkType", ".netstd");
            ClientProperties.Add("SrUfSdkVersion", SdkInfo.GetVersion());
            ClientProperties.Add("SrUfSdkInit", $"{DateTime.Now:yyyyMMddHHmm}");
            ClientProperties.Add("SrUfSdkConnName", "RabbitMQ / NETStd");
            ClientProperties.Add("SrUfSdkBId", $"{_config.BookmakerDetails?.BookmakerId}");
        }

        public bool IsConnected()
        {
            lock (_syncLock)
            {
                return _connectionSingleton != null && _connectionSingleton.IsOpen;
            }
        }

        /// <summary>
        /// Create a connection to the specified endpoint or return existing one
        /// </summary>
        /// <exception cref="RabbitMQ.Client.Exceptions.BrokerUnreachableException">When the configured host name was not reachable</exception>
        public override IConnection CreateConnection()
        {
            lock (_syncLock)
            {
                if (_connectionSingleton == null)
                {
                    if (!ClientProperties.ContainsKey("SrUfSdkType"))
                    {
                        Configure(); // configure only the first time, even if disconnect happens
                    }
                    _connectionSingleton = base.CreateConnection();
                    _connectionSingleton.ConnectionBlocked += OnConnectionBlocked;
                    _connectionSingleton.ConnectionUnblocked += OnConnectionUnblocked;
                    _connectionSingleton.RecoverySucceeded += OnRecoverySucceeded;
                    _connectionSingleton.ConnectionRecoveryError += OnConnectionRecoveryError;
                    _connectionSingleton.CallbackException += OnCallbackException;
                    _connectionSingleton.ConnectionShutdown += OnConnectionShutdown;
                    ConnectionCreated = DateTime.Now;
                }
                return _connectionSingleton;
            }
        }

        public void CloseConnection()
        {
            lock (_syncLock)
            {
                if (_connectionSingleton != null)
                {
                    try
                    {
                        _connectionSingleton.ConnectionBlocked -= OnConnectionBlocked;
                        _connectionSingleton.ConnectionUnblocked -= OnConnectionUnblocked;
                        _connectionSingleton.RecoverySucceeded -= OnRecoverySucceeded;
                        _connectionSingleton.ConnectionRecoveryError -= OnConnectionRecoveryError;
                        _connectionSingleton.CallbackException -= OnCallbackException;
                        _connectionSingleton.ConnectionShutdown -= OnConnectionShutdown;

                        _connectionSingleton.Close();
                    }
                    catch (Exception e)
                    {
                        SdkLoggerFactory.LoggerFactory.CreateLogger("ConnectionFactory").LogWarning(e, "Error closing connection");
                    }
                    finally
                    {
                        _connectionSingleton.Dispose();
                        _connectionSingleton = null;
                        ConnectionCreated = DateTime.MinValue;
                    }
                }
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (_syncLock)
            {
                if (_connectionSingleton?.IsOpen == true)
                {
                    _connectionSingleton?.Close();
                }
                _connectionSingleton?.Dispose();
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            ConnectionBlocked?.Invoke(sender, e);
        }

        private void OnConnectionUnblocked(object sender, EventArgs e)
        {
            ConnectionUnblocked?.Invoke(sender, e);
        }

        private void OnRecoverySucceeded(object sender, EventArgs e)
        {
            RecoverySucceeded?.Invoke(sender, e);
        }

        private void OnConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs e)
        {
            ConnectionRecoveryError?.Invoke(sender, e);
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            CallbackException?.Invoke(sender, e);
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            ConnectionShutdown?.Invoke(sender, e);
        }
    }
}
