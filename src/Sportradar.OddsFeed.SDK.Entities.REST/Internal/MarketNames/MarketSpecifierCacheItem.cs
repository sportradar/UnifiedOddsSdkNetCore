/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal class MarketSpecifierCacheItem
    {
        internal string Name { get; }

        internal string Type { get; }

        internal MarketSpecifierCacheItem(SpecifierDTO dto)
        {
            Contract.Requires(dto != null);

            Type = dto.Type;
            Name = dto.Name;
        }
    }

    internal class MarketAttributeCacheItem
    {
        internal string Name { get; }

        internal string Description { get; }

        internal MarketAttributeCacheItem(MarketAttributeDTO dto)
        {
            Contract.Requires(dto != null);

            Name = dto.Name;
            Description = dto.Description;
        }
    }
}
