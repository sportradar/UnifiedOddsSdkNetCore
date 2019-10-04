/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract followed by all top-level messages produced by the feed
    /// </summary>
    public interface IMessageV1 : IMessage
    {
        /// <summary>
        /// Gets the timestamps when the message was generated, sent, received and dispatched by the sdk
        /// </summary>
        /// <value>The timestamps</value>
        IMessageTimestamp Timestamps { get; }
    }
}
