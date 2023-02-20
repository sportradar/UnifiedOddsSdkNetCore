/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object containing data for stage period summary
    /// </summary>
    internal class PeriodSummaryDTO
    {
        /// <summary>
        /// Gets a <see cref="URN"/> specifying the id of the sport event associated with the current instance
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets the period statuses
        /// </summary>
        /// <value>The period statuses</value>
        public IEnumerable<PeriodStatusDTO> PeriodStatuses { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodSummaryDTO"/> class
        /// </summary>
        /// <param name="stagePeriod">A <see cref="stagePeriodEndpoint"/> containing data</param>
        internal PeriodSummaryDTO(stagePeriodEndpoint stagePeriod)
        {
            Guard.Argument(stagePeriod, nameof(stagePeriod)).NotNull();
            Guard.Argument(stagePeriod.sport_event, nameof(stagePeriod.sport_event)).NotNull();

            Id = URN.Parse(stagePeriod.sport_event.id);
            PeriodStatuses = new List<PeriodStatusDTO>();
            if (!stagePeriod.period_statuses.IsNullOrEmpty())
            {
                PeriodStatuses = stagePeriod.period_statuses.Select(s => new PeriodStatusDTO(s));
            }

            GeneratedAt = stagePeriod.generated_atSpecified
                ? stagePeriod.generated_at.ToLocalTime()
                : (DateTime?)null;
        }
    }
}
