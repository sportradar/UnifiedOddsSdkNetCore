// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    internal class OutcomeRollbackSettlement : Outcome, IOutcomeRollbackSettlement
    {
        public OutcomeRollbackSettlement(string id, INameProvider nameProvider, IMarketMappingProvider mappingProvider, IReadOnlyCollection<CultureInfo> cultures, IOutcomeDefinition outcomeDefinition)
            : base(id, nameProvider, mappingProvider, cultures, outcomeDefinition)
        {
        }
    }
}
