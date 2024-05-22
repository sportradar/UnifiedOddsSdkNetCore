// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class OutcomeMappingCacheItem
    {
        public string OutcomeId { get; private set; }

        public string ProducerOutcomeId { get; private set; }

        public IDictionary<CultureInfo, string> ProducerOutcomeNames { get; }

        public string MarketId { get; private set; }

        public OutcomeMappingCacheItem(OutcomeMappingDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            ProducerOutcomeNames = new Dictionary<CultureInfo, string>();

            Merge(dto, culture);
        }

        public void Merge(OutcomeMappingDto dto, CultureInfo culture)
        {
            OutcomeId = dto.OutcomeId;
            ProducerOutcomeId = dto.ProducerOutcomeId;
            ProducerOutcomeNames[culture] = dto.ProducerOutcomeName;
            MarketId = dto.MarketId;
        }
    }
}
