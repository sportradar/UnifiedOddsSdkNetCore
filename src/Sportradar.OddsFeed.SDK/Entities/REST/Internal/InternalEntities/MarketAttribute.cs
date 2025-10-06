// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities
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
            Guard.Argument(cacheItem, nameof(cacheItem)).NotNull();

            Name = cacheItem.Name;
            Description = cacheItem.Description;
        }
    }
}
