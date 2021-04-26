/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Class PeriodCompetitorResultDTO.
    /// </summary>
    internal class PeriodCompetitorResultDTO
    {
        /// <summary>
        /// Gets the competitor id
        /// </summary>
        /// <value>The competitor id</value>
        public URN Id { get; }

        /// <summary>
        /// Gets the competitor results
        /// </summary>
        /// <value>The results</value>
        public ICollection<CompetitorResultDTO> CompetitorResults { get; }

        public PeriodCompetitorResultDTO(periodStatusCompetitor periodStatusCompetitor)
        {
            Guard.Argument(periodStatusCompetitor, nameof(periodStatusCompetitor)).NotNull();

            Id = URN.Parse(periodStatusCompetitor.id);
            CompetitorResults = new List<CompetitorResultDTO>();
            if (!periodStatusCompetitor.result.IsNullOrEmpty())
            {
                CompetitorResults = periodStatusCompetitor.result.Select(s => new CompetitorResultDTO(s)).ToList();
            }
        }
    }
}
