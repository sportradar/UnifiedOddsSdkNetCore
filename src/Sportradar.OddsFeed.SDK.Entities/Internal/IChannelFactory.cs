/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using RabbitMQ.Client;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Represents a factory used to construct Rabbit MQ channels / models
    /// </summary>
    public interface IChannelFactory
    {
        /// <summary>
        /// Constructs and returns a <see cref="IModel"/> representing a channel used to communicate with the broker
        /// </summary>
        /// <returns>a <see cref="IModel"/> representing a channel used to communicate with the broker</returns>
        IModel CreateChannel();
    }
}
