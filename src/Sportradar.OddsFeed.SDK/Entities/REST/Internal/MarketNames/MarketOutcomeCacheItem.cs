// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class MarketOutcomeCacheItem
    {
        internal string Id { get; }

        internal readonly IDictionary<CultureInfo, string> Names;

        private readonly IDictionary<CultureInfo, string> _descriptions;

        internal MarketOutcomeCacheItem(OutcomeDescriptionDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Id = dto.Id;
            Names = new Dictionary<CultureInfo, string> { { culture, dto.Name } };
            _descriptions = string.IsNullOrEmpty(dto.Description)
                                ? new Dictionary<CultureInfo, string>()
                                : new Dictionary<CultureInfo, string> { { culture, dto.Description } };
        }

        internal string GetName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return Names.TryGetValue(culture, out var name) ? name : null;
        }

        internal string GetDescription(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return _descriptions.TryGetValue(culture, out var description) ? description : null;
        }

        internal void Merge(OutcomeDescriptionDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Names[culture] = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description))
            {
                _descriptions[culture] = dto.Description;
            }
        }
    }
}
