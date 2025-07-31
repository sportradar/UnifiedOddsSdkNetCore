// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    internal interface IMarketFactory
    {
        IMarketWithOdds GetMarketWithOdds(ISportEvent sportEvent, oddsChangeMarket marketOddsChange, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures);

        IMarketWithProbabilities GetMarketWithProbabilities(ISportEvent sportEvent, oddsChangeMarket marketOddsChange, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures);

        IMarketCancel GetMarketCancel(ISportEvent sportEvent, market market, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures);

        IMarketWithSettlement GetMarketWithResults(ISportEvent sportEvent, betSettlementMarket marketSettlement, int producerId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures);
    }
}
