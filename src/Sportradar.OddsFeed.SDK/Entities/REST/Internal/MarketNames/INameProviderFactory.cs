/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Defines a contract implemented by factories used to construct <see cref="INameProvider"/> instances
    /// </summary>
    public interface INameProviderFactory
    {
        /// <summary>
        /// Builds and returns a new instance of the <see cref="INameProvider"/>
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> instance representing associated sport @event</param>
        /// <param name="marketId">A market identifier, identifying the market associated with the returned instance</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> representing specifiers of the associated market</param>
        /// <returns>INameProvider.</returns>
        INameProvider BuildNameProvider(ISportEvent sportEvent, int marketId, IReadOnlyDictionary<string, string> specifiers);
    }
}
