/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes to keep a track of timming of received messages
    /// </summary>
    public interface ITimestampTracker
    {
        /// <summary>
        /// Gets a value indicating whether the feed messages are processed in a timely manner
        /// </summary>
        bool IsBehind { get; }

        /// <summary>
        /// Gets a value indicating whether the alive messages on a system session are received in a timely manner
        /// </summary>
        bool IsAliveViolated { get; }

        /// <summary>
        /// Gets the timestamp of the oldest (the one that was generated first) alive message received on the user session.
        /// </summary>
        long OldestUserAliveTimestamp { get; }

        /// <summary>
        /// Gets the epoch timestamp specifying when the last system alive message received was generated
        /// </summary>
        long SystemAliveTimestamp { get; }

        /// <summary>
        /// Records the provided <see cref="FeedMessage"/> timing info
        /// </summary>
        /// <param name="interest">The <see cref="MessageInterest"/> associated with the session receiving the alive message</param>
        /// <param name="message">The received <see cref="FeedMessage"/></param>
        void ProcessUserMessage(MessageInterest interest, FeedMessage message);

        /// <summary>
        /// Records the provided <see cref="alive"/> message timing info received on the system session
        /// </summary>
        /// <param name="alive">The <see cref="alive"/> message received on a system session.</param>
        void ProcessSystemAlive(alive alive);
    }
}