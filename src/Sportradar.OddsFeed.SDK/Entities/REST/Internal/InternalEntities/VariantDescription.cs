// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.InternalEntities
{
    internal class VariantDescription : IVariantDescription
    {
        public string Id { get; }

        public IEnumerable<IOutcomeDescription> Outcomes { get; }

        public IEnumerable<IMarketMappingData> Mappings { get; internal set; }

        internal VariantDescriptionCacheItem VariantDescriptionCacheItem { get; }

        internal VariantDescription(VariantDescriptionCacheItem cacheItem, IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(cacheItem, nameof(cacheItem)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            Id = cacheItem.Id;
            Outcomes = cacheItem.Outcomes == null
                           ? null
                           : new ReadOnlyCollection<IOutcomeDescription>(cacheItem.Outcomes.Select(o => (IOutcomeDescription)new OutcomeDescription(o, cultures)).ToList());
            Mappings = cacheItem.Mappings == null
                           ? null
                           : new ReadOnlyCollection<IMarketMappingData>(cacheItem.Mappings.Select(m => (IMarketMappingData)new MarketMapping(m)).ToList());

            VariantDescriptionCacheItem = cacheItem;
        }

        public void SetMappings(ReadOnlyCollection<IMarketMappingData> mappings)
        {
            Mappings = mappings;
        }
    }
}
