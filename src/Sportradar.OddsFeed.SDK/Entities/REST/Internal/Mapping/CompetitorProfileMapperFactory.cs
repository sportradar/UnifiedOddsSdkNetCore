// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{CompetitorProfileDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class CompetitorProfileMapperFactory : ISingleTypeMapperFactory<competitorProfileEndpoint, CompetitorProfileDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="competitorProfileEndpoint"/> instances to <see cref="CompetitorProfileDto"/> instances
        /// </summary>
        /// <param name="data">A <see cref="competitorProfileEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="competitorProfileEndpoint"/> instances to <see cref="CompetitorProfileDto"/> instances</returns>
        public ISingleTypeMapper<CompetitorProfileDto> CreateMapper(competitorProfileEndpoint data)
        {
            return new CompetitorProfileMapper(data);
        }
    }
}
