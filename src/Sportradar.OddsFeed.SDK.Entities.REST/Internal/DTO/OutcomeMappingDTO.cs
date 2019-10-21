/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a market mapping for outcome
    /// </summary>
    public class OutcomeMappingDTO
    {
        internal string OutcomeId { get; }

        internal string ProducerOutcomeId { get; }

        internal string ProducerOutcomeName { get; }

        internal string MarketId { get; }

        internal OutcomeMappingDTO(mappingsMappingMapping_outcome outcome, string marketId)
        {
            Guard.Argument(outcome).NotNull();
            Guard.Argument(marketId).NotNull().NotEmpty();

            OutcomeId = outcome.outcome_id;
            ProducerOutcomeId = outcome.product_outcome_id;
            ProducerOutcomeName = outcome.product_outcome_name;
            MarketId = marketId;
        }

        internal OutcomeMappingDTO(variant_mappingsMappingMapping_outcome outcome, string marketId)
        {
            Guard.Argument(outcome).NotNull();
            Guard.Argument(marketId).NotNull().NotEmpty();

            OutcomeId = outcome.outcome_id;
            ProducerOutcomeId = outcome.product_outcome_id;
            ProducerOutcomeName = outcome.product_outcome_name;
            MarketId = marketId;
        }
    }
}
