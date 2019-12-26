/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Class MarketDescriptionMapperFactory
    /// </summary>
    public class MarketDescriptionMapperFactory : ISingleTypeMapperFactory<market_descriptions, MarketDescriptionDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.ISingleTypeMapper`1" /> instance
        /// </summary>
        /// <param name="data">A <see cref="!:TIn" /> instance which the created <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.ISingleTypeMapper`1" /> will map</param>
        /// <returns>New <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.ISingleTypeMapper`1" /> instance</returns>
        public ISingleTypeMapper<MarketDescriptionDTO> CreateMapper(market_descriptions data)
        {
            return new MarketDescriptionMapper(data);
        }
    }
}
