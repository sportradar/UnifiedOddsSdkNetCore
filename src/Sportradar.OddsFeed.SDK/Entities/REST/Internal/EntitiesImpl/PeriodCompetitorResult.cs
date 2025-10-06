// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class PeriodCompetitorResult : IPeriodCompetitorResult
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
        public IEnumerable<ICompetitorResult> CompetitorResults { get; }

        public PeriodCompetitorResult(PeriodCompetitorResultDto periodCompetitorResult)
        {
            Guard.Argument(periodCompetitorResult, nameof(periodCompetitorResult)).NotNull();

            Id = periodCompetitorResult.Id;
            CompetitorResults = new List<CompetitorResult>();
            if (!periodCompetitorResult.CompetitorResults.IsNullOrEmpty())
            {
                CompetitorResults = periodCompetitorResult.CompetitorResults.Select(s => new CompetitorResult(s));
            }
        }
    }
}
