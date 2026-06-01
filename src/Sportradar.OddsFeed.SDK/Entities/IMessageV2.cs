// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Extends <see cref="IMessage"/> with AMQP message header information.
    /// This V2 interface exists to avoid breaking changes for customers who implement <see cref="IMessage"/> directly.
    /// Access the new properties via an explicit cast:
    /// <code>
    /// if (oddsChange is IMessageV2 v2)
    /// {
    ///     var headers = v2.MessageHeaders;
    /// }
    /// </code>
    /// </summary>
    public interface IMessageV2 : IMessage
    {
        /// <summary>
        /// Gets all AMQP headers delivered with the message as a read-only string dictionary.
        /// The returned dictionary is never null.
        /// </summary>
        IReadOnlyDictionary<string, string> MessageHeaders { get; }
    }
}
