/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object containing data for stage period summary
    /// </summary>
    internal class PeriodSummaryDto
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> specifying the id of the sport event associated with the current instance
        /// </summary>
        public Urn Id { get; }

        /// <summary>
        /// Gets the period statuses
        /// </summary>
        /// <value>The period statuses</value>
        public IEnumerable<PeriodStatusDto> PeriodStatuses { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodSummaryDto"/> class
        /// </summary>
        /// <param name="stagePeriod">A <see cref="stagePeriodEndpoint"/> containing data</param>
        internal PeriodSummaryDto(stagePeriodEndpoint stagePeriod)
        {
            Guard.Argument(stagePeriod, nameof(stagePeriod)).NotNull();
            Guard.Argument(stagePeriod.sport_event, nameof(stagePeriod.sport_event)).NotNull();

            Id = Urn.Parse(stagePeriod.sport_event.id);
            PeriodStatuses = new List<PeriodStatusDto>();
            if (!stagePeriod.period_statuses.IsNullOrEmpty())
            {
                PeriodStatuses = stagePeriod.period_statuses.Select(s => new PeriodStatusDto(s));
            }

            GeneratedAt = stagePeriod.generated_atSpecified
                ? stagePeriod.generated_at.ToLocalTime()
                : (DateTime?)null;
        }
    }
}
