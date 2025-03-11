// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public class MappingBuilder
{
    private string _marketId;
    private int _productId;
    private string _productIds;
    private string _sovTemplate;
    private string _sportId;
    private string _validFor;
    private string _productMarketId;
    private ICollection<mappingsMappingMapping_outcome> _invariantOutcomes;
    private ICollection<variant_mappingsMappingMapping_outcome> _variantOutcomes;

    public static MappingBuilder Create()
    {
        return new MappingBuilder();
    }

    public MappingBuilder WithMarketId(string marketId)
    {
        _marketId = marketId;
        return this;
    }

    public MappingBuilder WithProductId(int productId)
    {
        _productId = productId;
        return this;
    }

    public MappingBuilder WithProductIds(string productIds)
    {
        _productIds = productIds;
        return this;
    }

    public MappingBuilder WithSovTemplate(string sovTemplate)
    {
        _sovTemplate = sovTemplate;
        return this;
    }

    public MappingBuilder WithSportId(string sportId)
    {
        _sportId = sportId;
        return this;
    }

    public MappingBuilder WithValidFor(string validFor)
    {
        _validFor = validFor;
        return this;
    }

    public MappingBuilder WithProductMarketId(string productMarketId)
    {
        _productMarketId = productMarketId;
        return this;
    }

    public MappingBuilder AddInvariantMappingOutcome(string outcomeId, string productOutcomeId, string productOutcomeName)
    {
        _invariantOutcomes = _invariantOutcomes.IsNullOrEmpty()
            ? new List<mappingsMappingMapping_outcome> { MappingOutcomeBuilder.CreateInvariant(outcomeId, productOutcomeId, productOutcomeName) }
            : _invariantOutcomes.Append(MappingOutcomeBuilder.CreateInvariant(outcomeId, productOutcomeId, productOutcomeName)).ToList();
        return this;
    }

    public MappingBuilder AddVariantMappingOutcome(string outcomeId, string productOutcomeId, string productOutcomeName)
    {
        _variantOutcomes = _variantOutcomes.IsNullOrEmpty()
            ? new List<variant_mappingsMappingMapping_outcome> { MappingOutcomeBuilder.CreateVariant(outcomeId, productOutcomeId, productOutcomeName) }
            : _variantOutcomes.Append(MappingOutcomeBuilder.CreateVariant(outcomeId, productOutcomeId, productOutcomeName)).ToList();
        return this;
    }

    public mappingsMapping BuildInvariant()
    {
        var mapping = new mappingsMapping
        {
            market_id = _marketId,
            product_id = _productId,
            product_ids = _productIds,
            sov_template = _sovTemplate,
            sport_id = _sportId,
            valid_for = _validFor
        };
        if (!_invariantOutcomes.IsNullOrEmpty())
        {
            mapping.mapping_outcome = _invariantOutcomes.ToArray();
        }
        return mapping;
    }

    public variant_mappingsMapping BuildVariant()
    {
        var mapping = new variant_mappingsMapping
        {
            market_id = _marketId,
            product_id = _productId,
            product_ids = _productIds,
            sov_template = _sovTemplate,
            sport_id = _sportId,
            valid_for = _validFor,
            product_market_id = _productMarketId
        };
        if (!_invariantOutcomes.IsNullOrEmpty())
        {
            mapping.mapping_outcome = _variantOutcomes.ToArray();
        }
        return mapping;
    }

    public class MappingOutcomeBuilder
    {
        private string _outcomeId;
        private string _productOutcomeId;
        private string _productOutcomeName;

        public static MappingOutcomeBuilder Create()
        {
            return new MappingOutcomeBuilder();
        }

        public static mappingsMappingMapping_outcome CreateInvariant(string outcomeId, string productOutcomeId, string productOutcomeName)
        {
            return Create().WitOutcomeId(outcomeId).WithProductOutcomeId(productOutcomeId).WithProductOutcomeName(productOutcomeName).BuildInvariant();
        }

        public static variant_mappingsMappingMapping_outcome CreateVariant(string outcomeId, string productOutcomeId, string productOutcomeName)
        {
            return Create().WitOutcomeId(outcomeId).WithProductOutcomeId(productOutcomeId).WithProductOutcomeName(productOutcomeName).BuildVariant();
        }

        public MappingOutcomeBuilder WitOutcomeId(string outcomeId)
        {
            _outcomeId = outcomeId;
            return this;
        }

        public MappingOutcomeBuilder WithProductOutcomeId(string productOutcomeId)
        {
            _productOutcomeId = productOutcomeId;
            return this;
        }

        public MappingOutcomeBuilder WithProductOutcomeName(string productOutcomeName)
        {
            _productOutcomeName = productOutcomeName;
            return this;
        }

        public mappingsMappingMapping_outcome BuildInvariant()
        {
            return new mappingsMappingMapping_outcome
            {
                outcome_id = _outcomeId,
                product_outcome_id = _productOutcomeId,
                product_outcome_name = _productOutcomeName
            };
        }

        public variant_mappingsMappingMapping_outcome BuildVariant()
        {
            return new variant_mappingsMappingMapping_outcome
            {
                outcome_id = _outcomeId,
                product_outcome_id = _productOutcomeId,
                product_outcome_name = _productOutcomeName
            };
        }
    }
}
