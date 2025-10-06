// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="matchSummaryEndpoint" /> instances to <see cref="MatchDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{MatchDto}" />
    internal class MatchSummaryMapper : ISingleTypeMapper<MatchDto>
    {
        /// <summary>
        /// A <see cref="matchSummaryEndpoint"/> containing sport event data
        /// </summary>
        private readonly matchSummaryEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchSummaryMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="matchSummaryEndpoint"/> containing sport event data</param>
        internal MatchSummaryMapper(matchSummaryEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="SportEventSummaryDto" />
        /// </summary>
        /// <returns>The created <see cref="SportEventSummaryDto" /> instance</returns>
        public MatchDto Map()
        {
            return new MatchDto(_data);
        }
    }
}
