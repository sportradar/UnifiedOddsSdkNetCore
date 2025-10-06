// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Api.Replay
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
        /// Gets the <see cref="Urn"/> of the event in last message
        /// </summary>
        /// <value>The last message from event.</value>
        Urn LastMessageFromEvent { get; }
    }
}
