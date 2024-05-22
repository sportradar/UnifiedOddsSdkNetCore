// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for match timeline
    /// </summary>
    internal class MatchTimelineMapperFactory : ISingleTypeMapperFactory<matchTimelineEndpoint, MatchTimelineDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for match timeline
        /// </summary>
        /// <param name="data">A <see cref="matchTimelineEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<MatchTimelineDto> CreateMapper(matchTimelineEndpoint data)
        {
            return new MatchTimelineMapper(data);
        }
    }
}
