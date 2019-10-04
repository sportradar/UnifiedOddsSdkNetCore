/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities
{
    internal class MarketAttribute : IMarketAttribute
    {
        /// <summary>
        /// Gets the attribute name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the attribute description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketAttribute"/> class.
        /// </summary>
        /// <param name="cacheItem">A <see cref="MarketAttributeCacheItem"/> containing attribute data.</param>
        public MarketAttribute(MarketAttributeCacheItem cacheItem)
        {
            Contract.Requires(cacheItem != null);

            Name = cacheItem.Name;
            Description = cacheItem.Description;
        }
    }
}