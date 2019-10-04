/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    internal class MarketDescriptionMapper : ISingleTypeMapper<MarketDescriptionDTO>
    {
        /// <summary>
        /// A <see cref="market_descriptions"/> instance containing data used to construct <see cref="MarketDescriptionDTO"/> instance
        /// </summary>
        private readonly market_descriptions _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketDescriptionsMapper"/> class
        /// </summary>
        /// <param name="data">A <see cref="market_descriptions"/> instance containing data used to construct <see cref="MarketDescriptionDTO"/> instance</param>
        internal MarketDescriptionMapper(market_descriptions data)
        {
            Contract.Requires(data != null);

            _data = data;
        }


        /// <summary>
        /// Maps it's data to <see cref="MarketDescriptionDTO"/> instance
        /// </summary>
        /// <returns>The created<see cref="MarketDescriptionDTO"/> instance</returns>
        MarketDescriptionDTO ISingleTypeMapper<MarketDescriptionDTO>.Map()
        {
            var descriptions = _data.market.Select(m => new MarketDescriptionDTO(m)).ToList();
            return descriptions.FirstOrDefault();
        }
    }
}
