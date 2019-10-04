/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Messages.Feed;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Sportradar.OddsFeed.SDK.Messages.EventArguments
{
    /// <summary>
    /// Event arguments for the RawFeedMessageReceived events
    /// </summary>
    public class RawFeedMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The routing key associated with the feed message
        /// </summary>
        public string RoutingKey { get; }

        /// <summary>
        /// The feed message
        /// </summary>
        public FeedMessage FeedMessage { get; }

        /// <summary>
        /// Gets the associated message interest
        /// </summary>
        /// <value>The associated message interest</value>
        public string MessageInterest { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawFeedMessageEventArgs" /> class
        /// </summary>
        /// <param name="routingKey">The routing key associated with the feed message</param>
        /// <param name="feedMessage">The feed message</param>
        /// <param name="messageInterest">The associated message interest</param>
        public RawFeedMessageEventArgs(string routingKey, FeedMessage feedMessage, string messageInterest)
        {
            RoutingKey = routingKey;
            FeedMessage = feedMessage;
            MessageInterest = messageInterest;
        }
    }
}
