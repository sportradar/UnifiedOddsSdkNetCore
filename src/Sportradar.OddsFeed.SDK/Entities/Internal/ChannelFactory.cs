/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading;
using Dawn;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.API.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Represents a factory used to construct <see cref="IModel"/> instances representing channels to the broker
    /// </summary>
    /// <seealso cref="IChannelFactory" />
    /// <seealso cref="IDisposable" />
    internal class ChannelFactory : IChannelFactory, IDisposable
    {
        /// <summary>
        /// The <see cref="IConnectionFactory"/> used to construct connections to the broker
        /// </summary>
        public ConfiguredConnectionFactory ConnectionFactory { get; }

        /// <summary>
        /// The object used to ensure thread safety
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Value indicating whether the current instance has been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFactory"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        public ChannelFactory(ConfiguredConnectionFactory connectionFactory)
        {
            Guard.Argument(connectionFactory, nameof(connectionFactory)).NotNull();

            ConnectionFactory = connectionFactory;
        }

        /// <summary>
        /// Constructs and returns a <see cref="IModel" /> representing a channel used to communicate with the broker
        /// </summary>
        /// <returns>a <see cref="IModel" /> representing a channel used to communicate with the broker</returns>
        public IModel CreateChannel()
        {
            lock (_lock)
            {
                return ConnectionFactory.CreateConnection().CreateModel();
            }
        }

        public void ResetConnection()
        {
            lock (_lock)
            {
                ConnectionFactory.CloseConnection();
                Thread.Sleep(5000);
                ConnectionFactory.CreateConnection();
            }
        }

        /// <summary>
        /// Checks if the connection is opened
        /// </summary>
        public bool IsConnectionOpen()
        {
            lock (_lock)
            {
                return ConnectionFactory.IsConnected();
            }
        }

        /// <summary>
        /// DateTime when connection was created
        /// </summary>
        public DateTime ConnectionCreated
        {
            get
            {
                if (ConnectionFactory != null)
                {
                    lock (_lock)
                    {
                        return ConnectionFactory.ConnectionCreated;
                    }
                }
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
            lock (_lock)
            {
                ConnectionFactory.Dispose();
            }
        }
    }
}
