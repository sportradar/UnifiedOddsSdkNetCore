// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed.Messages;

public static class OddsChanges
{
    public static odds_change GetWithSingleMarketAndSpecifier()
    {
        return OddsChangeBuilder.Create()
                                .WithProduct(1)
                                .WithMatchId(1)
                                .AddMarket(market => market.WithMarketId(1)
                                                           .WithSpecifiers("turn=1")
                                                           .WithOutcome("1", 1.5)
                                                           .WithOutcome("2", 2.5))
                                .Build();
    }
}
