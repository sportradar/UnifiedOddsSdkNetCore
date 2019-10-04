/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{CompetitorProfileDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class CompetitorProfileMapperFactory : ISingleTypeMapperFactory<competitorProfileEndpoint, CompetitorProfileDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="competitorProfileEndpoint"/> instances to <see cref="CompetitorProfileDTO"/> instances
        /// </summary>
        /// <param name="data">A <see cref="competitorProfileEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="competitorProfileEndpoint"/> instances to <see cref="CompetitorProfileDTO"/> instances</returns>
        public ISingleTypeMapper<CompetitorProfileDTO> CreateMapper(competitorProfileEndpoint data)
        {
            return new CompetitorProfileMapper(data);
        }
    }
}