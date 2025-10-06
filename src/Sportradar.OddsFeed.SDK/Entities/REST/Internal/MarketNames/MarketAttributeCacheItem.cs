// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class MarketAttributeCacheItem
    {
        internal string Name { get; }

        internal string Description { get; }

        internal MarketAttributeCacheItem(MarketAttributeDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Name = dto.Name;
            Description = dto.Description;
        }
    }
}
