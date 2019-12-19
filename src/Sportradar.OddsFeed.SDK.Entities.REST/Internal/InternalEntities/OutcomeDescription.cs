/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
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
            string name;
            return _names.TryGetValue(culture, out name)
                ? name
                : null;
        }

        public string GetDescription(CultureInfo culture)
        {
            string description;
            return _descriptions.TryGetValue(culture, out description)
                ? description
                : null;
        }

        internal OutcomeDescription(MarketOutcomeCacheItem cacheItem, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(cacheItem).NotNull();
            Guard.Argument(cultures).NotNull().NotEmpty();

            var cultureList = cultures as List<CultureInfo> ?? cultures.ToList();

            Id = cacheItem.Id;
            _names = new ReadOnlyDictionary<CultureInfo, string>(cultureList.ToDictionary(culture => culture, cacheItem.GetName));
            _descriptions = new ReadOnlyDictionary<CultureInfo, string>(cultureList.Where(c => !string.IsNullOrEmpty(cacheItem.GetDescription(c))).ToDictionary(c => c, cacheItem.GetDescription));
        }
    }
}
