/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for result change
    /// </summary>
    internal class ResultChangeDTO
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

        internal ResultChangeDTO(resultChange resultChange, DateTime? generatedAt)
        {
            if (resultChange == null)
            {
                throw new ArgumentNullException(nameof(resultChange));
            }

            SportEventId = URN.Parse(resultChange.sport_event_id);
            UpdateTime = resultChange.update_time;
            GeneratedAt = generatedAt;
        }
    }
}
