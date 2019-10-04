/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes representing alive message dispatched by feed
    /// </summary>
    internal interface IAlive : IMessageV1
    {
        /// <summary>
        /// Gets a value whether the SDK is currently subscribed to receive messages from the producer specified by <see cref="IMessage.Producer"/> property
        /// </summary>
        bool Subscribed { get; }
    }
}
