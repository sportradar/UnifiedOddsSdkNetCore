/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for match timeline
    /// </summary>
    public class MatchTimelineMapperFactory : ISingleTypeMapperFactory<matchTimelineEndpoint, MatchTimelineDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for match timeline
        /// </summary>
        /// <param name="data">A <see cref="matchTimelineEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<MatchTimelineDTO> CreateMapper(matchTimelineEndpoint data)
        {
            return new MatchTimelineMapper(data);
        }
    }
}
