// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    internal class MarketDescriptionsMapper : ISingleTypeMapper<EntityList<MarketDescriptionDto>>
    {
        /// <summary>
        /// A <see cref="market_descriptions"/> instance containing data used to construct <see cref="EntityList{MarketDescriptionDto}"/> instance
        /// </summary>
        private readonly market_descriptions _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketDescriptionsMapper"/> class
        /// </summary>
        /// <param name="data">A <see cref="market_descriptions"/> instance containing data used to construct <see cref="EntityList{MarketDescriptionDto}"/> instance</param>
        internal MarketDescriptionsMapper(market_descriptions data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{MarketDescriptionDto}"/> instance
        /// </summary>
        /// <returns>The created<see cref="EntityList{MarketDescriptionDto}"/> instance</returns>
        EntityList<MarketDescriptionDto> ISingleTypeMapper<EntityList<MarketDescriptionDto>>.Map()
        {
            var descriptions = _data.market.Select(m => new MarketDescriptionDto(m)).ToList();
            return new EntityList<MarketDescriptionDto>(descriptions);
        }
    }
}
