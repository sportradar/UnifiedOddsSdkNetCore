// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Class PeriodCompetitorResultDto.
    /// </summary>
    internal class PeriodCompetitorResultDto
    {
        /// <summary>
        /// Gets the competitor id
        /// </summary>
        /// <value>The competitor id</value>
        public Urn Id { get; }

        /// <summary>
        /// Gets the competitor results
        /// </summary>
        /// <value>The results</value>
        public ICollection<CompetitorResultDto> CompetitorResults { get; }

        public PeriodCompetitorResultDto(periodStatusCompetitor periodStatusCompetitor)
        {
            Guard.Argument(periodStatusCompetitor, nameof(periodStatusCompetitor)).NotNull();

            Id = Urn.Parse(periodStatusCompetitor.id);
            CompetitorResults = new List<CompetitorResultDto>();
            if (!periodStatusCompetitor.result.IsNullOrEmpty())
            {
                CompetitorResults = periodStatusCompetitor.result.Select(s => new CompetitorResultDto(s)).ToList();
            }
        }
    }
}
