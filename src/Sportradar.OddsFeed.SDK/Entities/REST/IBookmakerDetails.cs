/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines  a contract implemented by classes representing bookmaker information
    /// </summary>
    public interface IBookmakerDetails : IEntityPrinter
    {
        /// <summary>
        /// Gets an optional message associated with the current instance
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets a value specifying the bookmaker's token will expire
        /// </summary>
        DateTime ExpireAt { get; }

        /// <summary>
        /// Gets the Sportradar's provided bookmaker id of the associated bookmaker
        /// </summary>
        int BookmakerId { get; }

        /// <summary>
        /// Gets the response code of the server's response or a null reference
        /// </summary>
        HttpStatusCode? ResponseCode { get; }

        /// <summary>
        /// Gets the virtual host which should be used when connecting to the AMQP broker
        /// </summary>
        string VirtualHost { get; }

        /// <summary>
        /// Gets the server time difference
        /// </summary>
        /// <value>The server time difference</value>
        TimeSpan ServerTimeDifference { get; }
    }
}
