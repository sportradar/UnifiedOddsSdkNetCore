/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{SportEventStatusDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{sportEventStatus, SportEventStatusDTO}" />
    internal class SportEventStatusMapperFactory : SportEventStatusMapperBase,  ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDTO>
    {
        /// <summary>
        /// Creates and returns a mapper instance for sport event status
        /// </summary>
        /// <param name="data">A <see cref="sportEventStatus"/> instance which the created mapper will map</param>
        /// <returns>New mapper instance</returns>
        public ISingleTypeMapper<SportEventStatusDTO> CreateMapper(sportEventStatus data)
        {
            return SportEventStatusMapper.Create(data);
        }
    }
}
