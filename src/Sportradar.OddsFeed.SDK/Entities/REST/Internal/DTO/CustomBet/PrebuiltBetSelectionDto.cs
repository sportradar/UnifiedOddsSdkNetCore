// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet
{
    internal class PrebuiltBetSelectionDto
    {
        public PrebuiltBetSelectionDto(PreBuiltBetsSelectionType selection)
        {
            MarketId = selection.market_id;
            OutcomeId = selection.outcome_id;
            Specifiers = selection.specifiers;
        }

        public int MarketId { get; }
        public string OutcomeId { get; }
        public string Specifiers { get; }
    }
}
