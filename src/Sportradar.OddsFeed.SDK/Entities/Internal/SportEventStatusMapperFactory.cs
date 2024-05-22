// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{SportEventStatusDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{sportEventStatus, SportEventStatusDto}" />
    internal class SportEventStatusMapperFactory : SportEventStatusMapperBase, ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto>
    {
        /// <summary>
        /// Creates and returns a mapper instance for sport event status
        /// </summary>
        /// <param name="data">A <see cref="sportEventStatus"/> instance which the created mapper will map</param>
        /// <returns>New mapper instance</returns>
        public ISingleTypeMapper<SportEventStatusDto> CreateMapper(sportEventStatus data)
        {
            return SportEventStatusMapper.Create(data);
        }
    }
}
