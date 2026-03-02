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
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    internal sealed class ConfiguredConnectionFactory : IDisposable
    {
        private readonly ILogger<ConfiguredConnectionFactory> _logger;

        internal IConnectionFactory ConnectionFactory;

        public DateTime ConnectionCreated { get; private set; }

        public event EventHandler<CallbackExceptionEventArgs> CallbackException;

        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;

        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;

        public event EventHandler<EventArgs> ConnectionUnblocked;

        private IConnection _connectionSingleton;

        private readonly object _syncLock = new object();

        private bool _disposed;

        public ConfiguredConnectionFactory(IConnectionFactory connectionFactory, IUofConfiguration config, ILogger<ConfiguredConnectionFactory> logger)
        {
            _ = Guard.Argument(connectionFactory, nameof(connectionFactory)).NotNull();
            _ = Guard.Argument(config, nameof(config)).NotNull();
            _ = Guard.Argument(logger, nameof(logger)).NotNull();

            _logger = logger;
            ConnectionCreated = DateTime.MinValue;

            ValidateConnectionFactory(connectionFactory);
            ConnectionFactory = connectionFactory;

            UpdateConnectionFactoryCredentials(ConnectionFactory, config);
        }

        public bool IsConnected()
        {
            lock (_syncLock)
            {
                return _connectionSingleton != null && _connectionSingleton.IsOpen;
            }
        }

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

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
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

        internal static ConnectionFactory CreateConnectionFactoryWithImmutableFields(IUofConfiguration config)
        {
            return new ConnectionFactory
            {
                ClientProvidedName = $"UofSdk / {SdkInfo.SdkType}",
                HostName = config.Rabbit.Host,
                Port = config.Rabbit.Port,
                VirtualHost = config.Rabbit.VirtualHost,
                AutomaticRecoveryEnabled = true,
                RequestedHeartbeat = config.Rabbit.Heartbeat,
                RequestedConnectionTimeout = config.Rabbit.ConnectionTimeout,
                Ssl = GetRabbitSslOptions(config),
                ClientProperties = GetClientPropertiesWithSdkInfo(config)
            };
        }

        internal static Dictionary<string, object> GetClientPropertiesWithSdkInfo(IUofConfiguration config)
        {
            return new Dictionary<string, object>
                       {
                           { "SrUfSdkType", SdkInfo.SdkType },
                           { "SrUfSdkVersion", SdkInfo.GetVersion() },
                           { "SrUfSdkInit", SdkInfo.UtcNowString() },
                           { "SrUfSdkConnName", $"RabbitMQ / {SdkInfo.SdkType}" },
                           { "SrUfSdkBId", config.BookmakerDetails?.BookmakerId.ToString(CultureInfo.InvariantCulture) }
                       };
        }

        private static void ValidateConnectionFactory(IConnectionFactory connectionFactory)
        {
            if (connectionFactory.ClientProvidedName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(connectionFactory), "ClientProvidedName cannot be null or empty");
            }
            if (connectionFactory.ClientProperties.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(connectionFactory), "ClientProperties cannot be null or empty");
            }
        }

        private static void UpdateConnectionFactoryCredentials(IConnectionFactory connectionFactory, IUofConfiguration config)
        {
            connectionFactory.UserName = config.Rabbit.Username;
            connectionFactory.Password = config.Rabbit.Password ?? string.Empty;
        }

        private void ReleaseConnectionEventsAndClose()
        {
            _connectionSingleton.ConnectionBlocked -= OnConnectionBlocked;
            _connectionSingleton.ConnectionUnblocked -= OnConnectionUnblocked;
            _connectionSingleton.CallbackException -= OnCallbackException;
            _connectionSingleton.ConnectionShutdown -= OnConnectionShutdown;
            _connectionSingleton.Close(TimeSpan.FromSeconds(10));
        }

        private static SslOption GetRabbitSslOptions(IUofConfiguration config)
        {
            var sslOption = new SslOption { Enabled = config.Rabbit.UseSsl };

            if (config.Rabbit.UseSsl)
            {
                sslOption.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;
            }

            return sslOption;
        }
    }
}
