/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes capable of interacting with Replay Server
    /// </summary>
    public interface IReplayManagerV1 : IReplayManager
    {
        /// <summary>
        /// Start replay the event from replay queue. Events are played in the order they were played in reality
        /// </summary>
        /// <param name="speed">The speed factor of the replay</param>
        /// <param name="maxDelay">The maximum delay between messages in milliseconds</param>
        /// <param name="producerId">The id of the producer from which we want to get messages, or null for messages from all producers</param>
        /// <param name="rewriteTimestamps">Should the timestamp in messages be rewritten with current time</param>
        /// <param name="runParallel">Should the events in queue replay independently</param>
        /// <remarks>Start replay the event from replay queue. Events are played in the order they were played in reality, e.g. if there are some events that were played simultaneously in reality, they will be played in parallel as well here on replay server. If not specified, default values speed = 10 and max_delay = 10000 are used. This means that messages will be sent 10x faster than in reality, and that if there was some delay between messages that was longer than 10 seconds it will be reduced to exactly 10 seconds/10 000 ms (this is helpful especially in pre-match odds where delay can be even a few hours or more). If player is already in play, nothing will happen</remarks>
        /// <returns>Returns an <see cref="IReplayResponse"/></returns>
        IReplayResponse StartReplay(int speed, int maxDelay, int? producerId, bool? rewriteTimestamps, bool? runParallel);
    }
}
