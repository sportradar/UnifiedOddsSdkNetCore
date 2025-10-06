// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Api.Replay
{
    /// <summary>
    /// Defines a contract implemented by classes representing replay events
    /// </summary>
    public interface IReplayEvent
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> specifying the event id
        /// </summary>
        Urn Id { get; }

        /// <summary>
        /// Gets a <see cref="int"/> specifying the position
        /// </summary>
        int? Position { get; }

        /// <summary>
        /// Gets a <see cref="int"/> specifying the start time
        /// </summary>
        int? StartTime { get; }
    }
}
