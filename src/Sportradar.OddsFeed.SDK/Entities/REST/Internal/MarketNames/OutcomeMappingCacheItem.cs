/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal class OutcomeMappingCacheItem
    {
        public string OutcomeId { get; private set; }

        public string ProducerOutcomeId { get; private set; }

        public IDictionary<CultureInfo, string> ProducerOutcomeNames { get; }

        public string MarketId { get; private set; }

        public OutcomeMappingCacheItem(OutcomeMappingDTO dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            ProducerOutcomeNames = new Dictionary<CultureInfo, string>();

            Merge(dto, culture);
        }

        public void Merge(OutcomeMappingDTO dto, CultureInfo culture)
        {
            OutcomeId = dto.OutcomeId;
            ProducerOutcomeId = dto.ProducerOutcomeId;
            ProducerOutcomeNames[culture] = dto.ProducerOutcomeName;
            MarketId = dto.MarketId;
        }
    }
}
