/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}"/> instances used to map <see cref="sportsEndpoint"/> instances to
    /// <see cref="EntityList{SportDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class SportsMapperFactory : ISingleTypeMapperFactory<sportsEndpoint, EntityList<SportDTO>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="sportsEndpoint"/> instances to
        /// <see cref="EntityList{SportDTO}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="sportsEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>The constructed <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportDTO>> CreateMapper(sportsEndpoint data)
        {
            return SportsMapper.Create(data);
        }
    }
}