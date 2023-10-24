/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{PlayerProfileDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class PlayerProfileMapperFactory : ISingleTypeMapperFactory<playerProfileEndpoint, PlayerProfileDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="playerProfileEndpoint"/> instances to <see cref="PlayerProfileDto"/> instances
        /// </summary>
        /// <param name="data">A <see cref="playerProfileEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="playerProfileEndpoint"/> instances to <see cref="PlayerProfileDto"/> instances</returns>
        public ISingleTypeMapper<PlayerProfileDto> CreateMapper(playerProfileEndpoint data)
        {
            return new PlayerProfileMapper(data);
        }
    }
}
