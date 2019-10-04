/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using RabbitMQ.Client;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Represents a factory used to construct <see cref="IModel"/> instances representing channels to the broker
    /// </summary>
    /// <seealso cref="IChannelFactory" />
    /// <seealso cref="System.IDisposable" />
    public class ChannelFactory : IChannelFactory, IDisposable
    {
        /// <summary>
        /// The <see cref="IConnectionFactory"/> used to construct connections to the broker
        /// </summary>
        private readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// The object used to ensure thread safety
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// The <see cref="IConnection"/> representing connection to the broker
        /// </summary>
        private IConnection _connection;

        /// <summary>
        /// Value indicating whether the current instance has been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFactory"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        public ChannelFactory(IConnectionFactory connectionFactory)
        {
            Contract.Requires(connectionFactory != null);

            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (_disposed || !disposing)
            {
                return;
            }

            _disposed = true;
            _connection.Dispose();
        }

        /// <summary>
        /// Constructs and returns a <see cref="IModel" /> representing a channel used to communicate with the broker
        /// </summary>
        /// <returns>a <see cref="IModel" /> representing a channel used to communicate with the broker</returns>
        public IModel CreateChannel()
        {
            lock (_lock)
            {
                if (_connection == null)
                {
                    _connection = _connectionFactory.CreateConnection();
                }

                return _connection.CreateModel();
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
    }
}