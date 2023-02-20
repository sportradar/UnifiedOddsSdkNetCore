/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to parse the RabbitMq routing key in order to
    /// determine the sportId of the sport associated with the message
    /// </summary>
    internal interface IRoutingKeyParser
    {
        /// <summary>
        /// Gets a <see cref="URN"/> representing the sportId by parsing the provided <c>routingKey</c>
        /// </summary>
        /// <param name="routingKey">The routing key specified by the feed</param>
        /// <param name="messageTypeName">The type name of the received message</param>
        /// <returns>The sportId obtained by parsing the provided <c>routingKey</c></returns>
        URN GetSportId(string routingKey, string messageTypeName);

        /// <summary>
        /// Tries to get a <see cref="URN"/> representing of the sportId by parsing the provided <c>routingKey</c>
        /// </summary>
        /// <param name="routingKey">The routing key specified by the feed</param>
        /// <param name="messageTypeName">The type name of the received message</param>
        /// <param name="sportId">If the method returned true the sportId; Otherwise undefined</param>
        /// <returns>True if sportId could be retrieved from <c>routingKey</c>; Otherwise false</returns>
        bool TryGetSportId(string routingKey, string messageTypeName, out URN sportId);
    }
}
