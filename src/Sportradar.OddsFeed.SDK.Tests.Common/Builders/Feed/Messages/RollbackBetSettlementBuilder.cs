// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed.Messages;

public class RollbackBetSettlementBuilder
{
    private readonly rollback_bet_settlement _rollbackBetSettlement = new rollback_bet_settlement();
    private readonly List<market> _markets = [];

    public static RollbackBetSettlementBuilder Create()
    {
        return new RollbackBetSettlementBuilder();
    }

    public RollbackBetSettlementBuilder WithEventId(Urn eventId)
    {
        _rollbackBetSettlement.event_id = eventId.ToString();
        return this;
    }

    public RollbackBetSettlementBuilder WithProduct(int product)
    {
        _rollbackBetSettlement.product = product;
        return this;
    }

    public RollbackBetSettlementBuilder WithRequestId(long requestId)
    {
        _rollbackBetSettlement.request_id = requestId;
        _rollbackBetSettlement.request_idSpecified = true;
        return this;
    }

    public RollbackBetSettlementBuilder WithoutRequestId()
    {
        _rollbackBetSettlement.request_id = 0;
        _rollbackBetSettlement.request_idSpecified = false;
        return this;
    }

    public RollbackBetSettlementBuilder AddMarket(market market)
    {
        _markets.Add(market);
        return this;
    }

    public RollbackBetSettlementBuilder AddMarket(MarketBuilder marketBuilder)
    {
        _markets.Add(marketBuilder.Build());
        return this;
    }

    public rollback_bet_settlement Build()
    {
        if (_markets.Count != 0)
        {
            _rollbackBetSettlement.market = _markets.ToArray();
        }
        _rollbackBetSettlement.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return _rollbackBetSettlement;
    }
}
