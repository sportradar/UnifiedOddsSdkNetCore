/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by factories used to construct <see cref="IMarketMappingProvider"/> instances
    /// </summary>
    public interface IMarketMappingProviderFactory
    {
        /// <summary>
        /// Builds and returns a new instance of the <see cref="IMarketMappingProvider"/>
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> instance representing associated sport @event</param>
        /// <param name="marketId">A market identifier, identifying the market associated with the returned instance</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> representing specifiers of the associated market</param>
        /// <param name="producer">An <see cref="IProducer"/> of the <see cref="ISportEvent"/></param>
        /// <param name="sportId">A sportId of the <see cref="ISportEvent"/></param>
        /// <returns>Returns an instance of <see cref="IMarketMappingProvider"/></returns>
        IMarketMappingProvider BuildMarketMappingProvider(ISportEvent sportEvent, int marketId, IReadOnlyDictionary<string, string> specifiers, IProducer producer, URN sportId);
    }
}
