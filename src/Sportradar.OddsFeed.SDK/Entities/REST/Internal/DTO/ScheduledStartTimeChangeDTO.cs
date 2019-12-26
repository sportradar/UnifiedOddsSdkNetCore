/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for scheduled start time change (used in fixtures)
    /// </summary>
    public class ScheduledStartTimeChangeDTO
    {
        /// <summary>
        /// Gets the old time
        /// </summary>
        /// <value>The old time</value>
        public DateTime OldTime { get; }

        /// <summary>
        /// Gets the new time
        /// </summary>
        /// <value>The new time</value>
        public DateTime NewTime { get; }

        /// <summary>
        /// Gets the changed at
        /// </summary>
        /// <value>The changed at</value>
        public DateTime ChangedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledStartTimeChangeDTO"/> class
        /// </summary>
        /// <param name="timeChange">The time change</param>
        public ScheduledStartTimeChangeDTO(scheduledStartTimeChange timeChange)
        {
            Guard.Argument(timeChange, nameof(timeChange)).NotNull();

            OldTime = timeChange.old_time;
            NewTime = timeChange.new_time;
            ChangedAt = timeChange.changed_at;
        }
    }
}
