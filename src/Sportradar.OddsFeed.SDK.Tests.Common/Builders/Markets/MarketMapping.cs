// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

internal class MarketMapping : IMarketMappingData
{
    public IEnumerable<int> ProducerIds
    {
        get;
    }
    public Urn SportId
    {
        get;
    }
    public string MarketId
    {
        get;
    }
    public int MarketTypeId
    {
        get;
    }
    public int? MarketSubTypeId
    {
        get;
    }
    public string SovTemplate
    {
        get;
    }
    public string ValidFor
    {
        get;
    }
    public IEnumerable<IOutcomeMappingData> OutcomeMappings
    {
        get;
    }

    public MarketMapping(string marketId, mappingsMapping mapping, CultureInfo culture)
    {
        ProducerIds = string.IsNullOrEmpty(mapping.product_ids)
            ? new List<int> { mapping.product_id }.AsReadOnly()
            : mapping.product_ids.Split(new[] { SdkInfo.MarketMappingProductsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
        SportId = mapping.sport_id == "all" ? null : Urn.Parse(mapping.sport_id);
        MarketId = marketId;
        if (!SdkInfo.IsMarketMappingMarketIdValid(mapping.market_id))
        {
            throw new FormatException($"Mapping market_id '{mapping.market_id}' is not valid");
        }
        var mappingMarketId = mapping.market_id.Split(':'); // legacy
        int.TryParse(mappingMarketId[0], out var typeId);
        MarketTypeId = typeId;
        if (mappingMarketId.Length == 2)
        {
            MarketSubTypeId = int.TryParse(mappingMarketId[1], out var subTypeId) ? subTypeId : null;
        }
        SovTemplate = mapping.sov_template;
        ValidFor = mapping.valid_for;

        OutcomeMappings = new List<OutcomeMappingData>();
        if (mapping.mapping_outcome != null)
        {
            OutcomeMappings = mapping.mapping_outcome.Select(o => new OutcomeMappingData(MarketId, o, culture));
        }
    }

    public bool CanMap(int producerId, Urn sportId, IReadOnlyDictionary<string, string> specifiers)
    {
        if (!ProducerIds.Contains(producerId) || (SportId != null && !SportId.Equals(sportId)))
        {
            return false;
        }

        return true;
    }

    private class OutcomeMappingData : IOutcomeMappingData
    {
        public string OutcomeId { get; private set; }

        public string ProducerOutcomeId { get; private set; }

        public IDictionary<CultureInfo, string> ProducerOutcomeNames { get; }

        public string MarketId { get; private set; }

        public OutcomeMappingData(string marketId, mappingsMappingMapping_outcome mappingOutcome, CultureInfo culture)
        {
            ProducerOutcomeNames = new Dictionary<CultureInfo, string>();

            Merge(marketId, mappingOutcome, culture);
        }

        public void Merge(string marketId, mappingsMappingMapping_outcome mappingOutcome, CultureInfo culture)
        {
            OutcomeId = mappingOutcome.outcome_id;
            ProducerOutcomeId = mappingOutcome.product_outcome_id;
            ProducerOutcomeNames[culture] = mappingOutcome.product_outcome_name;
            MarketId = marketId;
        }

        public string GetProducerOutcomeName(CultureInfo culture)
        {
            ProducerOutcomeNames.TryGetValue(culture, out var name);

            return name;
        }
    }
}
