/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for result change
    /// </summary>
    internal class ResultChangeDto
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

        internal ResultChangeDto(resultChange resultChange, DateTime? generatedAt)
        {
            if (resultChange == null)
            {
                throw new ArgumentNullException(nameof(resultChange));
            }

            SportEventId = Urn.Parse(resultChange.sport_event_id);
            UpdateTime = resultChange.update_time;
            GeneratedAt = generatedAt;
        }
    }
}
