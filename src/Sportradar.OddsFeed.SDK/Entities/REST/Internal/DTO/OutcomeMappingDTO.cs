/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a market mapping for outcome
    /// </summary>
    internal class OutcomeMappingDto
    {
        internal string OutcomeId { get; }

        internal string ProducerOutcomeId { get; }

        internal string ProducerOutcomeName { get; }

        internal string MarketId { get; }

        internal OutcomeMappingDto(mappingsMappingMapping_outcome outcome, string marketId)
        {
            Guard.Argument(outcome, nameof(outcome)).NotNull();
            Guard.Argument(marketId, nameof(marketId)).NotNull().NotEmpty();

            OutcomeId = outcome.outcome_id;
            ProducerOutcomeId = outcome.product_outcome_id;
            ProducerOutcomeName = outcome.product_outcome_name;
            MarketId = marketId;
        }

        internal OutcomeMappingDto(variant_mappingsMappingMapping_outcome outcome, string marketId)
        {
            Guard.Argument(outcome, nameof(outcome)).NotNull();
            Guard.Argument(marketId, nameof(marketId)).NotNull().NotEmpty();

            OutcomeId = outcome.outcome_id;
            ProducerOutcomeId = outcome.product_outcome_id;
            ProducerOutcomeName = outcome.product_outcome_name;
            MarketId = marketId;
        }
    }
}
