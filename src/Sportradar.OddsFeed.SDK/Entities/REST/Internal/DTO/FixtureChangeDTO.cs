// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for fixture change
    /// </summary>
    internal class FixtureChangeDto
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> specifying the sport event
        /// </summary>
        public Urn SportEventId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the last update time
        /// </summary>
        public DateTime UpdateTime { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        internal FixtureChangeDto(fixtureChange fixtureChange, DateTime? generatedAt)
        {
            if (fixtureChange == null)
            {
                throw new ArgumentNullException(nameof(fixtureChange));
            }

            SportEventId = Urn.Parse(fixtureChange.sport_event_id);
            UpdateTime = fixtureChange.update_time;
            GeneratedAt = generatedAt;
        }
    }
}
