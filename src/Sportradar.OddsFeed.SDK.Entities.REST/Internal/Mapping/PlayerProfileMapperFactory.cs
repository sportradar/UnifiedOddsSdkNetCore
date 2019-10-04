/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{PlayerProfileDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class PlayerProfileMapperFactory : ISingleTypeMapperFactory<playerProfileEndpoint, PlayerProfileDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="playerProfileEndpoint"/> instances to <see cref="PlayerProfileDTO"/> instances
        /// </summary>
        /// <param name="data">A <see cref="playerProfileEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="playerProfileEndpoint"/> instances to <see cref="PlayerProfileDTO"/> instances</returns>
        public ISingleTypeMapper<PlayerProfileDTO> CreateMapper(playerProfileEndpoint data)
        {
            return new PlayerProfileMapper(data);
        }
    }
}
