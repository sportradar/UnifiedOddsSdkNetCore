/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by messages indicating that all messages from the requested snapshot were send
    /// </summary>
    public interface ISnapshotCompleted : IMessage
    {
        /// <summary>
        /// Get the id of the request which triggered the current <see cref="ISnapshotCompleted"/> message
        /// </summary>
        long RequestId { get; }
    }
}
