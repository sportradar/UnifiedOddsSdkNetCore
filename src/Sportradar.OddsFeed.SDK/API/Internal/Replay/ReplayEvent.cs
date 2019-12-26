/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal.Replay
{
    /// <summary>
    /// Represents a replay event
    /// </summary>
    internal class ReplayEvent : IReplayEvent
    {
        /// <summary>
        /// Creates new instance of <see cref="ReplayEvent"/>
        /// </summary>
        /// <param name="id">The id of associated sport event</param>
        /// <param name="position">The position of event in the queue</param>
        /// <param name="startTime">The start time specified when the event was added to the queue</param>
        public ReplayEvent(URN id, int? position, int? startTime)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            Id = id;
            Position = position;
            StartTime = startTime;
        }

        /// <summary>
        /// Gets a <see cref="URN"/> specifying the event id
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets a <see cref="int"/> specifying the position
        /// </summary>
        public int? Position { get; }

        /// <summary>
        /// Gets a <see cref="int"/> specifying the start time
        /// </summary>
        public int? StartTime { get; }
    }
}
