/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Class MarketDescriptionsMapperFactory
    /// </summary>
    public class MarketDescriptionsMapperFactory : ISingleTypeMapperFactory<market_descriptions, EntityList<MarketDescriptionDTO>>
    {
        /// <summary>
        /// Creates and returns an instance of Mapper for mapping <see cref="market_descriptions"/>
        /// </summary>
        /// <param name="data">A input instance which the created <see cref="MarketDescriptionMapper"/> will map</param>
        /// <returns>New <see cref="MarketDescriptionMapper" /> instance</returns>
        public ISingleTypeMapper<EntityList<MarketDescriptionDTO>> CreateMapper(market_descriptions data)
        {
            return new MarketDescriptionsMapper(data);
        }
    }
}
