/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract for status of replay player
    /// </summary>
    public interface IReplayStatus
    {
        /// <summary>
        /// Gets the player status. Possible values are: player is playing, player is stopped, player was never playing.
        /// </summary>
        ReplayPlayerStatus PlayerStatus { get; }

        /// <summary>
        /// Gets the <see cref="URN"/> of the event in last message
        /// </summary>
        /// <value>The last message from event.</value>
        URN LastMessageFromEvent { get; }
    }
}