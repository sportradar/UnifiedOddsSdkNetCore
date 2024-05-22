// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{MatchDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class MatchSummaryMapperFactory : ISingleTypeMapperFactory<matchSummaryEndpoint, MatchDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{MatchDto}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="matchSummaryEndpoint" /> instance which the created <see cref="ISingleTypeMapper{MatchDto}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{MatchDto}" /> instance</returns>
        public ISingleTypeMapper<MatchDto> CreateMapper(matchSummaryEndpoint data)
        {
            return new MatchSummaryMapper(data);
        }
    }
}
