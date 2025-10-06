// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}"/> instances used to map <see cref="tournamentsEndpoint"/> instances to
    /// <see cref="EntityList{SportDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class TournamentsMapperFactory : ISingleTypeMapperFactory<tournamentsEndpoint, EntityList<SportDto>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="tournamentsEndpoint"/> instances to
        /// <see cref="EntityList{SportDto}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="tournamentsEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>The constructed <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<SportDto>> CreateMapper(tournamentsEndpoint data)
        {
            return TournamentsMapper.Create(data);
        }
    }
}
