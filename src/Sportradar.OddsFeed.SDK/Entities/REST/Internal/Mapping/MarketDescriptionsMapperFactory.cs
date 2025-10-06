// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Class MarketDescriptionsMapperFactory
    /// </summary>
    internal class MarketDescriptionsMapperFactory : ISingleTypeMapperFactory<market_descriptions, EntityList<MarketDescriptionDto>>
    {
        /// <summary>
        /// Creates and returns an instance of Mapper for mapping <see cref="market_descriptions"/>
        /// </summary>
        /// <param name="data">A input instance which the created <see cref="MarketDescriptionMapper"/> will map</param>
        /// <returns>New <see cref="MarketDescriptionMapper" /> instance</returns>
        public ISingleTypeMapper<EntityList<MarketDescriptionDto>> CreateMapper(market_descriptions data)
        {
            return new MarketDescriptionsMapper(data);
        }
    }
}
