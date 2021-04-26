/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    internal class PeriodStatus : IPeriodStatus
    {
        /// <summary>
        /// Gets the number of the specific lap.
        /// </summary>
        /// <value>The number of the specific lap.</value>
        public int? Number { get; }

        /// <summary>
        /// Gets the type
        /// </summary>
        /// <value>The type</value>
        /// <remarks>Possible values: lap</remarks>
        public string Type { get; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        /// <remarks>Possible values: not_started, started, completed.</remarks>
        public string Status { get; }

        /// <summary>
        /// Gets the period results
        /// </summary>
        /// <value>The results</value>
        public IEnumerable<IPeriodCompetitorResult> PeriodResults { get; }

        public PeriodStatus(PeriodStatusDTO periodStatus)
        {
            Guard.Argument(periodStatus, nameof(periodStatus)).NotNull();

            Number = periodStatus.Number;
            Type = periodStatus.Type;
            Status = periodStatus.Status;
            if (!periodStatus.PeriodResults.IsNullOrEmpty())
            {
                PeriodResults = periodStatus.PeriodResults.Select(s => new PeriodCompetitorResult(s));
            }
            else
            {
                PeriodResults = new List<IPeriodCompetitorResult>();
            }
        }
    }
}
