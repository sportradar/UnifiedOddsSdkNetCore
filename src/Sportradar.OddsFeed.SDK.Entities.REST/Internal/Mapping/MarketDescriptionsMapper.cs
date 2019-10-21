/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    internal class MarketDescriptionsMapper : ISingleTypeMapper<EntityList<MarketDescriptionDTO>>
    {
        /// <summary>
        /// A <see cref="market_descriptions"/> instance containing data used to construct <see cref="EntityList{MarketDescriptionDTO}"/> instance
        /// </summary>
        private readonly market_descriptions _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketDescriptionsMapper"/> class
        /// </summary>
        /// <param name="data">A <see cref="market_descriptions"/> instance containing data used to construct <see cref="EntityList{MarketDescriptionDTO}"/> instance</param>
        internal MarketDescriptionsMapper(market_descriptions data)
        {
            Guard.Argument(data).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{MarketDescriptionDTO}"/> instance
        /// </summary>
        /// <returns>The created<see cref="EntityList{MarketDescriptionDTO}"/> instance</returns>
        EntityList<MarketDescriptionDTO> ISingleTypeMapper<EntityList<MarketDescriptionDTO>>.Map()
        {
            var descriptions = _data.market.Select(m => new MarketDescriptionDTO(m)).ToList();
            return new EntityList<MarketDescriptionDTO>(descriptions);
        }
    }
}
