/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a market mapping
    /// </summary>
    public class MarketMappingDTO
    {
        [Obsolete("Changed with ProducerIds property")]
        internal int ProducerId { get; }

        internal IEnumerable<int> ProducerIds { get; }

        internal URN SportId { get; }

        internal int MarketTypeId { get; }

        internal int? MarketSubTypeId { get; }

        internal string SovTemplate { get; }

        internal string ValidFor { get; }

        internal IEnumerable<OutcomeMappingDTO> OutcomeMappings { get; }

        internal string OrgMarketId { get; }

        internal MarketMappingDTO(mappingsMapping mapping)
        {
            Contract.Requires(mapping != null);
            Contract.Requires(mapping.product_id > 0);
            Contract.Requires(!string.IsNullOrEmpty(mapping.sport_id));
            Contract.Requires(!string.IsNullOrEmpty(mapping.market_id));

            ProducerId = mapping.product_id;
            ProducerIds = string.IsNullOrEmpty(mapping.product_ids)
                ? new[] { mapping.product_id }
                : mapping.product_ids.Split(new[] {SdkInfo.MarketMappingProductsDelimiter}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
            SportId = mapping.sport_id == "all" ? null : URN.Parse(mapping.sport_id);
            OrgMarketId = null;
            var marketId = mapping.market_id.Split(':'); // legacy
            int typeId;
            int.TryParse(marketId[0], out typeId);
            MarketTypeId = typeId;
            if (marketId.Length == 2)
            {
                int subTypeId;
                MarketSubTypeId = int.TryParse(marketId[1], out subTypeId) ? subTypeId : (int?) null;
            }
            SovTemplate = mapping.sov_template;
            ValidFor = mapping.valid_for;

            if (mapping.mapping_outcome != null)
            {
                OutcomeMappings = mapping.mapping_outcome.Select(o => new OutcomeMappingDTO(o, mapping.market_id));
            }
        }

        internal MarketMappingDTO(variant_mappingsMapping mapping)
        {
            Contract.Requires(mapping != null);
            Contract.Requires(mapping.product_id > 0);
            Contract.Requires(!string.IsNullOrEmpty(mapping.sport_id));
            Contract.Requires(!string.IsNullOrEmpty(mapping.market_id));

            ProducerId = mapping.product_id;
            ProducerIds = string.IsNullOrEmpty(mapping.product_ids)
                ? new[] { mapping.product_id }
                : mapping.product_ids.Split(new[] { SdkInfo.MarketMappingProductsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
            SportId = mapping.sport_id == "all" ? null : URN.Parse(mapping.sport_id);
            OrgMarketId = mapping.market_id;
            var marketId = string.IsNullOrEmpty(mapping.product_market_id)
                               ? mapping.market_id.Split(':')
                               : mapping.product_market_id.Split(':');
            int typeId;
            int.TryParse(marketId[0], out typeId);
            MarketTypeId = typeId;
            if (marketId.Length == 2)
            {
                int subTypeId;
                MarketSubTypeId = int.TryParse(marketId[1], out subTypeId) ? subTypeId : (int?) null;
            }
            SovTemplate = mapping.sov_template;
            ValidFor = mapping.valid_for;

            if (mapping.mapping_outcome != null)
            {
                OutcomeMappings = mapping.mapping_outcome.Select(o => new OutcomeMappingDTO(o, mapping.market_id));
            }
        }
    }
}
