// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using FormatException = System.FormatException;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a market mapping
    /// </summary>
    internal class MarketMappingDto
    {
        internal IEnumerable<int> ProducerIds { get; }

        internal Urn SportId { get; }

        internal int MarketTypeId { get; }

        internal int? MarketSubTypeId { get; }

        internal string SovTemplate { get; }

        internal string ValidFor { get; }

        internal IEnumerable<OutcomeMappingDto> OutcomeMappings { get; }

        internal string OrgMarketId { get; }

        internal MarketMappingDto(mappingsMapping mapping)
        {
            Guard.Argument(mapping, nameof(mapping)).NotNull();
            Guard.Argument(mapping.product_id, nameof(mapping.product_id)).Positive();
            Guard.Argument(mapping.sport_id, nameof(mapping.sport_id)).NotNull().NotEmpty();
            Guard.Argument(mapping.market_id, nameof(mapping.market_id)).NotNull().NotEmpty();

            if (!SdkInfo.IsMarketMappingProducerIdsValid(mapping.product_ids))
            {
                throw new FormatException($"Mapping producer_ids '{mapping.product_ids}' is not valid");
            }
            ProducerIds = string.IsNullOrEmpty(mapping.product_ids)
                ? new[] { mapping.product_id }
                : mapping.product_ids.Split(new[] { SdkInfo.MarketMappingProductsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
            SportId = mapping.sport_id == "all" ? null : Urn.Parse(mapping.sport_id);
            OrgMarketId = null;
            if (!SdkInfo.IsMarketMappingMarketIdValid(mapping.market_id))
            {
                throw new FormatException($"Mapping market_id '{mapping.market_id}' is not valid");
            }
            var marketId = mapping.market_id.Split(':'); // legacy
            int.TryParse(marketId[0], out var typeId);
            MarketTypeId = typeId;
            if (marketId.Length == 2)
            {
                MarketSubTypeId = int.TryParse(marketId[1], out var subTypeId) ? subTypeId : (int?)null;
            }
            SovTemplate = mapping.sov_template;
            ValidFor = mapping.valid_for;

            OutcomeMappings = new List<OutcomeMappingDto>();
            if (mapping.mapping_outcome != null)
            {
                OutcomeMappings = mapping.mapping_outcome.Select(o => new OutcomeMappingDto(o, mapping.market_id));
            }
        }

        internal MarketMappingDto(variant_mappingsMapping mapping)
        {
            Guard.Argument(mapping, nameof(mapping)).NotNull();
            Guard.Argument(mapping.product_id, nameof(mapping.product_id)).Positive();
            Guard.Argument(mapping.sport_id, nameof(mapping.sport_id)).NotNull().NotEmpty();
            Guard.Argument(mapping.market_id, nameof(mapping.market_id)).NotNull().NotEmpty();

            if (!SdkInfo.IsMarketMappingProducerIdsValid(mapping.product_ids))
            {
                throw new FormatException($"Mapping producer_ids '{mapping.product_ids}' is not valid");
            }
            if (!string.IsNullOrEmpty(mapping.market_id) && !SdkInfo.IsMarketMappingMarketIdValid(mapping.market_id))
            {
                throw new FormatException($"Mapping market_id '{mapping.market_id}' is not valid");
            }
            if (!string.IsNullOrEmpty(mapping.product_market_id) && !SdkInfo.IsMarketMappingMarketIdValid(mapping.product_market_id))
            {
                throw new FormatException($"Mapping producer_market_id '{mapping.product_market_id}' is not valid");
            }
            ProducerIds = string.IsNullOrEmpty(mapping.product_ids)
                ? new[] { mapping.product_id }
                : mapping.product_ids.Split(new[] { SdkInfo.MarketMappingProductsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
            SportId = mapping.sport_id == "all" ? null : Urn.Parse(mapping.sport_id);
            OrgMarketId = mapping.market_id;
            var marketId = string.IsNullOrEmpty(mapping.product_market_id)
                               ? mapping.market_id.Split(':')
                               : mapping.product_market_id.Split(':');
            int.TryParse(marketId[0], out var typeId);
            MarketTypeId = typeId;
            if (marketId.Length == 2)
            {
                MarketSubTypeId = int.TryParse(marketId[1], out var subTypeId) ? subTypeId : (int?)null;
            }
            SovTemplate = mapping.sov_template;
            ValidFor = mapping.valid_for;

            OutcomeMappings = new List<OutcomeMappingDto>();
            if (mapping.mapping_outcome != null)
            {
                OutcomeMappings = mapping.mapping_outcome.Select(o => new OutcomeMappingDto(o, string.IsNullOrEmpty(mapping.product_market_id) ? mapping.market_id : mapping.product_market_id));
            }
        }
    }
}
