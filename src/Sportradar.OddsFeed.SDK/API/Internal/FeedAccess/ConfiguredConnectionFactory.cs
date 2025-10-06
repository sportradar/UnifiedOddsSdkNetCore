// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Security;
using Dawn;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    /// <summary>
    /// A <see cref="IConnectionFactory"/> implementations which properly configures it self before first <see cref="IConnection"/> is created
    /// </summary>
    internal class ConfiguredConnectionFactory : IDisposable
    {
        internal IConnectionFactory ConnectionFactory;
        public DateTime ConnectionCreated { get; private set; }

        public event EventHandler<CallbackExceptionEventArgs> CallbackException;

        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;

        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;

        public event EventHandler<EventArgs> ConnectionUnblocked;

        private readonly IUofConfiguration _config;

        private readonly ILogger<ConfiguredConnectionFactory> _logger;

        /// <summary>
        /// A singleton instance of <see cref="IConnection"/> class created by current factory.
        /// </summary>
        private IConnection _connectionSingleton;

        /// <summary>
        /// A <see cref="object"/> used to ensure thread safety when creating the connection singleton
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Value indicating whether the current instance has been disposed
        /// </summary>
        private bool _disposed;

        public ConfiguredConnectionFactory(IUofConfiguration config, ILogger<ConfiguredConnectionFactory> logger)
        {
            Guard.Argument(config, nameof(config)).NotNull();
            Guard.Argument(logger, nameof(logger)).NotNull();

            _config = config;
            _logger = logger;
            ConnectionCreated = DateTime.MinValue;

            CreateAndConfigureConnectionFactory();
        }

        /// <summary>
        /// Configures the current <see cref="IConnectionFactory"/> based on config options read from <c>_config</c> field
        /// </summary>
        private void CreateAndConfigureConnectionFactory()
        {
            var sslOption = new SslOption
            {
                Enabled = _config.Rabbit.UseSsl
            };

            if (_config.Rabbit.UseSsl)
            {
                sslOption.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
            }

            var clientProperties = new Dictionary<string, object>
                                       {
                                           { "SrUfSdkType", SdkInfo.SdkType },
                                           { "SrUfSdkVersion", SdkInfo.GetVersion() },
                                           { "SrUfSdkInit", SdkInfo.UtcNowString() },
                                           { "SrUfSdkConnName", $"RabbitMQ / {SdkInfo.SdkType}" },
                                           { "SrUfSdkBId", _config.BookmakerDetails?.BookmakerId.ToString(CultureInfo.InvariantCulture) }
                                       };

            ConnectionFactory = new ConnectionFactory
            {
                ClientProvidedName = $"UofSdk / {SdkInfo.SdkType}",
                HostName = _config.Rabbit.Host,
                Port = _config.Rabbit.Port,
                UserName = _config.Rabbit.Username,
                Password = _config.Rabbit.Password ?? string.Empty,
                VirtualHost = _config.Rabbit.VirtualHost,
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = _config.Rabbit.Heartbeat,
                RequestedConnectionTimeout = _config.Rabbit.ConnectionTimeout,
                Ssl = sslOption,
                ClientProperties = clientProperties
            };
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
        public IConnection CreateConnection()
        {
            lock (_syncLock)
            {
                if (_connectionSingleton != null)
                {
                    return _connectionSingleton;
                }

                ConnectionFactory.ClientProperties["SrUfSdkInit"] = SdkInfo.UtcNowString();
                _connectionSingleton = ConnectionFactory.CreateConnection();
                _connectionSingleton.ConnectionBlocked += OnConnectionBlocked;
                _connectionSingleton.ConnectionUnblocked += OnConnectionUnblocked;
                _connectionSingleton.CallbackException += OnCallbackException;
                _connectionSingleton.ConnectionShutdown += OnConnectionShutdown;
                ConnectionCreated = DateTime.Now;
                _logger.LogInformation("New connection created");
                return _connectionSingleton;
            }
        }

        public void CloseConnection()
        {
            lock (_syncLock)
            {
                if (_connectionSingleton == null)
                {
                    return;
                }

                try
                {
                    ReleaseConnectionEventsAndClose();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error closing connection");
                }
                finally
                {
                    _connectionSingleton.Dispose();
                    _connectionSingleton = null;
                    ConnectionCreated = DateTime.MinValue;
                }
            }
        }

        private void ReleaseConnectionEventsAndClose()
        {
            if (_connectionSingleton != null)
            {
                _connectionSingleton.ConnectionBlocked -= OnConnectionBlocked;
                _connectionSingleton.ConnectionUnblocked -= OnConnectionUnblocked;
                _connectionSingleton.CallbackException -= OnCallbackException;
                _connectionSingleton.ConnectionShutdown -= OnConnectionShutdown;
                _connectionSingleton.Close(TimeSpan.FromSeconds(10));
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
            if (_disposed || !disposing)
            {
                return;
            }

            _disposed = true;

            CloseConnection();
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            ConnectionBlocked?.Invoke(sender, e);
        }

        private void OnConnectionUnblocked(object sender, EventArgs e)
        {
            ConnectionUnblocked?.Invoke(sender, e);
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
