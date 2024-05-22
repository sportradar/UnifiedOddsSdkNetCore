// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Class MarketDescriptionMapperFactory
    /// </summary>
    internal class MarketDescriptionMapperFactory : ISingleTypeMapperFactory<market_descriptions, MarketDescriptionDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapperFactory{Tin, Tout}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="market_descriptions" /> instance which the created <see cref="ISingleTypeMapperFactory{Tin, Tout}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapperFactory{Tin, Tout}" /> instance</returns>
        public ISingleTypeMapper<MarketDescriptionDto> CreateMapper(market_descriptions data)
        {
            return new MarketDescriptionMapper(data);
        }
    }
}
