/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for fixture change
    /// </summary>
    public class FixtureChangeDTO
    {
        /// <summary>
        /// Gets the <see cref="URN"/> specifying the sport event
        /// </summary>
        public URN SportEventId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the last update time
        /// </summary>
        public DateTime UpdateTime { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        internal FixtureChangeDTO(fixtureChange fixtureChange, DateTime? generatedAt)
        {
            if (fixtureChange == null)
                throw new ArgumentNullException(nameof(fixtureChange));

            SportEventId = URN.Parse(fixtureChange.sport_event_id);
            UpdateTime = fixtureChange.update_time;
            GeneratedAt = generatedAt;
        }
    }
}
