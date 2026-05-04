// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
{
    internal class PrebuiltBetSelection : IPrebuiltBetSelection
    {
        public PrebuiltBetSelection(PrebuiltBetSelectionDto selection)
        {
            MarketId = selection.MarketId;
            OutcomeId = selection.OutcomeId;
            Specifiers = selection.Specifiers;
        }

        public int MarketId { get; }
        public string OutcomeId { get; }
        public string Specifiers { get; }
    }
}
