/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{MatchDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class MatchSummaryMapperFactory : ISingleTypeMapperFactory<matchSummaryEndpoint, MatchDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{MatchDTO}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="matchSummaryEndpoint" /> instance which the created <see cref="ISingleTypeMapper{MatchDTO}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{MatchDTO}" /> instance</returns>
        public ISingleTypeMapper<MatchDTO> CreateMapper(matchSummaryEndpoint data)
        {
            return new MatchSummaryMapper(data);
        }
    }
}
