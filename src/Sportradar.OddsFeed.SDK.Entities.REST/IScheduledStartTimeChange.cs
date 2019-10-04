/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing schedule start time change
    /// </summary>
    public interface IScheduledStartTimeChange
    {
        /// <summary>
        /// Gets the old time
        /// </summary>
        /// <value>The old time</value>
        DateTime OldTime { get; }

        /// <summary>
        /// Gets the new time
        /// </summary>
        /// <value>The new time</value>
        DateTime NewTime { get; }

        /// <summary>
        /// Gets the changed at
        /// </summary>
        /// <value>The changed at</value>
        DateTime ChangedAt { get; }
    }
}