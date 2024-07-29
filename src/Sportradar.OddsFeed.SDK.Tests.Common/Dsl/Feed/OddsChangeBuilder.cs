// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;

public class OddsChangeBuilder
{
    private readonly odds_change _oddsChange = new odds_change();
    private readonly List<oddsChangeMarket> _markets = new List<oddsChangeMarket>();

    public static OddsChangeBuilder Create()
    {
        var builder = new OddsChangeBuilder().WithTimestamp(DateTime.Now.ToEpochTime());
        return builder;
    }

    public OddsChangeBuilder WithMatchId(long eventId)
    {
        _oddsChange.event_id = $"sr:match:{eventId}";
        return this;
    }

    public OddsChangeBuilder ForEventId(Urn eventId)
    {
        _oddsChange.event_id = eventId.ToString();
        return this;
    }

    public OddsChangeBuilder WithProduct(int productId)
    {
        _oddsChange.product = productId;
        return this;
    }

    public OddsChangeBuilder WithTimestamp(long timestamp)
    {
        _oddsChange.timestamp = timestamp;
        return this;
    }

    public odds_change Build()
    {
        if (_markets.Any())
        {
            _oddsChange.odds = new odds_changeOdds
            {
                market = _markets.ToArray()
            };
        }
        return _oddsChange;
    }

    public OddsChangeBuilder AddMarket(Action<MarketWithOddsBuilder> marketBuilderAction)
    {
        var marketBuilder = MarketWithOddsBuilder.Create();
        marketBuilderAction.Invoke(marketBuilder);
        _markets.Add(marketBuilder.Build());
        return this;
    }

    public OddsChangeBuilder AddMarket(oddsChangeMarket market)
    {
        _markets.Add(market);
        return this;
    }

    public OddsChangeBuilder WithSportEventStatus(sportEventStatus ses)
    {
        _oddsChange.sport_event_status = ses;
        return this;
    }

    // public OddsChangeBuilder AddMarkets(int marketSize, int outcomeSize)
    // {
    //     var marketBuilder = MarketWithOddsBuilder.Create();
    //     marketBuilderAction.Invoke(marketBuilder);
    //     _markets.Add(marketBuilder.Build());
    //     return this;
    // }
}
