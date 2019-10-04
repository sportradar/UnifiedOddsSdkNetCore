/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{SimpleTeamProfileDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class SimpleTeamProfileMapperFactory : ISingleTypeMapperFactory<simpleTeamProfileEndpoint, SimpleTeamProfileDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="simpleTeamProfileEndpoint"/> instances to <see cref="SimpleTeamProfileDTO"/> instances
        /// </summary>
        /// <param name="data">A <see cref="simpleTeamProfileEndpoint" /> instance containing profile data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="simpleTeamProfileEndpoint"/> instances to <see cref="SimpleTeamProfileDTO"/> instances</returns>
        public ISingleTypeMapper<SimpleTeamProfileDTO> CreateMapper(simpleTeamProfileEndpoint data)
        {
            return new SimpleTeamProfileMapper(data);
        }
    }
}