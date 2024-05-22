// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}"/> instances used to map <see cref="producers"/> instances to
    /// <see cref="EntityList{T}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class ProducersMapperFactory : ISingleTypeMapperFactory<producers, EntityList<ProducerDto>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="producers"/> instances to
        /// <see cref="EntityList{ProducerDto}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="producers" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>The constructed <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<ProducerDto>> CreateMapper(producers data)
        {
            return ProducersMapper.Create(data);
        }
    }
}
