// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Replay;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Replay
{
    /// <summary>
    /// Implementation of <see cref="IReplayStatus"/>
    /// </summary>
    /// <seealso cref="IReplayStatus"/>
    internal class ReplayStatus : IReplayStatus
    {
        /// <summary>
        /// Gets the player status. Possible values are: player is playing, player is stopped, player was never playing.
        /// </summary>
        public ReplayPlayerStatus PlayerStatus { get; }

        /// <summary>
        /// Gets the <see cref="Urn"/> of the event in last message
        /// </summary>
        /// <value>The last message from event.</value>
        public Urn LastMessageFromEvent { get; }

        /// <summary>
        /// Constructs new instance of <see cref="ReplayStatus"/>
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="status"></param>
        public ReplayStatus(Urn eventId, ReplayPlayerStatus status)
        {
            LastMessageFromEvent = eventId;
            PlayerStatus = status;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance</returns>
        public override string ToString()
        {
            return $"[EventId={LastMessageFromEvent}, PlayerStatus={PlayerStatus}]";
        }
    }
}
