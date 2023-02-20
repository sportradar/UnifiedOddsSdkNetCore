/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.InternalEntities
{
    internal class OutcomeDescription : IOutcomeDescription
    {
        private readonly IDictionary<CultureInfo, string> _names;

        private readonly IDictionary<CultureInfo, string> _descriptions;

        public string Id { get; }

        public string GetName(CultureInfo culture)
        {
            return _names.TryGetValue(culture, out var name)
                ? name
                : null;
        }

        public string GetDescription(CultureInfo culture)
        {
            return _descriptions.TryGetValue(culture, out var description)
                ? description
                : null;
        }

        internal OutcomeDescription(MarketOutcomeCacheItem cacheItem, IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(cacheItem, nameof(cacheItem)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            Id = cacheItem.Id;
            _names = new ReadOnlyDictionary<CultureInfo, string>(cultures.ToDictionary(culture => culture, cacheItem.GetName));
            _descriptions = new ReadOnlyDictionary<CultureInfo, string>(cultures.Where(c => !string.IsNullOrEmpty(cacheItem.GetDescription(c))).ToDictionary(c => c, cacheItem.GetDescription));
        }
    }
}
