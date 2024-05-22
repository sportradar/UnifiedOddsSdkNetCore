// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Config
{
    /// <summary>
    /// Defines a contract implemented by classes representing rabbit connection configuration / settings
    /// </summary>
    public interface IUofRabbitConfiguration
    {
        /// <summary>
        /// Gets a value specifying the host name of the AQMP broker
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the port used for connecting to the AQMP broker
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the user name for connecting to the AQMP broker
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password for connecting to the AQMP broker
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets a value specifying the virtual host of the AQMP broker
        /// </summary>
        string VirtualHost { get; }

        /// <summary>
        /// Gets a value specifying whether the connection to AMQP broker should use SSL encryption
        /// </summary>
        bool UseSsl { get; }

        /// <summary>
        /// Gets a rabbit timeout setting for connection attempts (in seconds)
        /// </summary>
        /// <value>A rabbit timeout setting for connection attempts (in seconds)</value>
        /// <remarks>Between 10 and 120 (default 30s)</remarks>
        TimeSpan ConnectionTimeout { get; }

        /// <summary>
        /// Gets a heartbeat timeout to use when negotiating with the server (in seconds)
        /// </summary>
        /// <value>A heartbeat timeout to use when negotiating with the server (in seconds)</value>
        /// <remarks>Between 10 and 180 (default 60s)</remarks>
        TimeSpan Heartbeat { get; }
    }
}
