// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class MarketSpecifierCacheItem
    {
        internal string Name { get; }

        internal string Type { get; }

        internal string Description { get; }

        internal MarketSpecifierCacheItem(SpecifierDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Type = dto.Type;
            Name = dto.Name;
            Description = dto.Description;
        }
    }
}
