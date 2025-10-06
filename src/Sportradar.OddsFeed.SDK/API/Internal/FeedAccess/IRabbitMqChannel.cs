// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    /// <summary>
    /// Represents a contract implemented by classes used to connect to rabbit mq broker
    /// </summary>
    internal interface IRabbitMqChannel
    {
        /// <summary>
        /// Gets a value indicating whether the current channel is opened
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Opens the current channel and binds the created queue to provided routing keys
        /// </summary>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session using this instance</param>
        /// <param name="routingKeys">A <see cref="IEnumerable{String}"/> specifying the routing keys of the constructed queue.</param>
        void Open(MessageInterest interest, IEnumerable<string> routingKeys);

        /// <summary>
        /// Closes the current channel
        /// </summary>
        void Close();

        /// <summary>
        /// Occurs when the current channel received the data
        /// </summary>
        event EventHandler<BasicDeliverEventArgs> Received;
    }
}
