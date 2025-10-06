// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Extensions
{
    internal static class MarketMappingExtensions
    {
        internal static bool MarketMappingsMatch(this MarketMappingCacheItem ci, MarketMappingDto dto)
        {
            var isMatch = ci.MarketTypeId.Equals(dto.MarketTypeId) && ci.MarketSubTypeId == dto.MarketSubTypeId;

            if (isMatch && ci.SportId != null)
            {
                isMatch = ci.SportId.Equals(dto.SportId);
            }

            if (isMatch && ci.ProducerIds != null)
            {
                isMatch = ci.ProducerIds.All(a => dto.ProducerIds.Contains(a));
            }

            if (isMatch)
            {
                isMatch = string.Equals(ci.ValidFor, dto.ValidFor, StringComparison.InvariantCultureIgnoreCase);
            }

            return isMatch;
        }

        internal static string GenerateMarketMappingId(this MarketMappingDto marketMappingDto)
        {
            return marketMappingDto.MarketSubTypeId == null
                       ? marketMappingDto.MarketTypeId.ToString()
                       : $"{marketMappingDto.MarketTypeId}:{marketMappingDto.MarketSubTypeId}";
        }
    }
}
