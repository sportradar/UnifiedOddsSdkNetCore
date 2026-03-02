// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    internal class MarketRollbackSettlement : MarketCancel, IMarketRollbackSettlement
    {
        internal MarketRollbackSettlement(int id,
                                          IReadOnlyDictionary<string, string> specifiers,
                                          IReadOnlyDictionary<string, string> additionalInfo,
                                          INameProvider nameProvider,
                                          IMarketMappingProvider mappingProvider,
                                          IMarketDefinition marketDefinition,
                                          int? voidReason,
                                          INamedValueCache voidReasonsCache,
                                          IReadOnlyCollection<CultureInfo> cultures,
                                          IReadOnlyCollection<IOutcomeRollbackSettlement> outcomes)
            : base(id, specifiers, additionalInfo, nameProvider, mappingProvider, marketDefinition, voidReason, voidReasonsCache, cultures)
        {
            Outcomes = outcomes?.ToList().AsReadOnly();
        }

        public IEnumerable<IOutcomeRollbackSettlement> Outcomes { get; }
    }
}
