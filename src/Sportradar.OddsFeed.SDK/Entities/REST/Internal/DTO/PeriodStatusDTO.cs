/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Class PeriodStatusDTO.
    /// </summary>
    internal class PeriodStatusDTO
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
        public ICollection<PeriodCompetitorResultDTO> PeriodResults { get; }

        public PeriodStatusDTO(periodStatus periodStatus)
        {
            Guard.Argument(periodStatus, nameof(periodStatus)).NotNull();

            Number = periodStatus.numberSpecified ? periodStatus.number : (int?)null;
            Type = periodStatus.type;
            Status = periodStatus.status;
            if (!periodStatus.competitor.IsNullOrEmpty())
            {
                PeriodResults = periodStatus.competitor.Select(s => new PeriodCompetitorResultDTO(s)).ToList();
            }
            else
            {
                PeriodResults = new List<PeriodCompetitorResultDTO>();
            }
        }
    }
}
