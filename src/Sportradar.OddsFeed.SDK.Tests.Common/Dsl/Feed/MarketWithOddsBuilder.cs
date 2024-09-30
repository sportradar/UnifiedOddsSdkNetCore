// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;

public class MarketWithOddsBuilder
{
    private readonly oddsChangeMarket _market = new oddsChangeMarket();
    private readonly List<oddsChangeMarketOutcome> _outcomes = new List<oddsChangeMarketOutcome>();

    public static MarketWithOddsBuilder Create()
    {
        return new MarketWithOddsBuilder();
    }

    public MarketWithOddsBuilder WithMarketId(int marketId)
    {
        _market.id = marketId;
        return this;
    }

    public MarketWithOddsBuilder WithOutcome(string outcomeId, double odds)
    {
        var outcome = new oddsChangeMarketOutcome
        {
            id = outcomeId, odds = odds, oddsSpecified = true, active = 1,
            activeSpecified = true
        };
        _outcomes.Add(outcome);
        return this;
    }

    public MarketWithOddsBuilder WithSpecifiers(string specifiers)
    {
        _market.specifiers = specifiers;
        return this;
    }

    public oddsChangeMarket Build()
    {
        if (_outcomes.Any())
        {
            _market.outcome = _outcomes.ToArray();
        }
        return _market;
    }
}
