// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{SimpleTeamProfileDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class SimpleTeamProfileMapperFactory : ISingleTypeMapperFactory<simpleTeamProfileEndpoint, SimpleTeamProfileDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="simpleTeamProfileEndpoint"/> instances to <see cref="SimpleTeamProfileDto"/> instances
        /// </summary>
        /// <param name="data">A <see cref="simpleTeamProfileEndpoint" /> instance containing profile data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="simpleTeamProfileEndpoint"/> instances to <see cref="SimpleTeamProfileDto"/> instances</returns>
        public ISingleTypeMapper<SimpleTeamProfileDto> CreateMapper(simpleTeamProfileEndpoint data)
        {
            return new SimpleTeamProfileMapper(data);
        }
    }
}
