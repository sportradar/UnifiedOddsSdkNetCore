/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal class MarketOutcomeCacheItem
    {
        internal string Id { get; }

        private readonly IDictionary<CultureInfo, string> _names;

        private readonly IDictionary<CultureInfo, string> _descriptions;

        internal MarketOutcomeCacheItem(OutcomeDescriptionDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);

            Id = dto.Id;
            _names = new Dictionary<CultureInfo, string> { {culture, dto.Name} };
            _descriptions = string.IsNullOrEmpty(dto.Description)
                ? new Dictionary<CultureInfo, string>()
                : new Dictionary<CultureInfo, string> {{culture, dto.Description}};
        }

        internal string GetName(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            string name;
            if (_names.TryGetValue(culture, out name))
            {
                return name;
            }
            return null;
        }

        internal string GetDescription(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            string description;
            if (_descriptions.TryGetValue(culture, out description))
            {
                return description;
            }
            return null;
        }

        internal void Merge(OutcomeDescriptionDTO dto, CultureInfo culture)
        {
            Contract.Requires(dto != null);
            Contract.Requires(culture != null);

            _names[culture] = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description))
            {
                _descriptions[culture] = dto.Description;
            }
        }
    }
}
