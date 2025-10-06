// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;

public sealed class BetSettlementBuilder
{
    private readonly bet_settlement _betSettlement;
    private readonly List<betSettlementMarket> _outcomes = [];
    private betSettlementMarket _currentOutcome;

    private BetSettlementBuilder(string eventId)
    {
        _betSettlement = new bet_settlement()
        {
            event_id = eventId
        };
    }

    public static BetSettlementBuilder CreateForSportEvent(string eventId)
    {
        return new BetSettlementBuilder(eventId);
    }

    public BetSettlementBuilder AddMarket(int id, string specifiers, string extendedSpecifiers = null)
    {
        _currentOutcome = new betSettlementMarket
        {
            id = id,
            specifiers = specifiers,
            extended_specifiers = string.IsNullOrEmpty(extendedSpecifiers) ? null : extendedSpecifiers,
            Specifiers = ParseSpecifiers(specifiers)
        };

        _outcomes.Add(_currentOutcome);
        return this;
    }

    public BetSettlementBuilder WithSportId(Urn sportId)
    {
        _betSettlement.SportId = sportId;
        return this;
    }

    public BetSettlementBuilder WithRequestId(long requestId)
    {
        _betSettlement.request_id = requestId;
        _betSettlement.request_idSpecified = true;
        return this;
    }

    public BetSettlementBuilder WithTimestamp(long timestamp)
    {
        _betSettlement.timestamp = timestamp;
        return this;
    }

    public BetSettlementBuilder WithProducerId(int producerId)
    {
        _betSettlement.product = producerId;
        return this;
    }

    public BetSettlementBuilder AddOutcome(int id, int result)
    {
        if (_currentOutcome == null)
        {
            throw new InvalidOperationException("AddOutcome must follow AddMarket.");
        }

        var list = _currentOutcome.Items != null
                       ? new List<betSettlementMarketOutcome>(_currentOutcome.Items)
                       : new List<betSettlementMarketOutcome>();

        list.Add(new betSettlementMarketOutcome { id = id.ToString(), result = result });
        _currentOutcome.Items = list.ToArray();
        return this;
    }

    public BetSettlementBuilder AddMarketWithOutcomes(int id, string specifiers, string extendedSpecifiers, params Tuple<int, int>[] outcomes)
    {
        AddMarket(id, specifiers, extendedSpecifiers);
        if (outcomes == null)
        {
            return this;
        }

        foreach (var outcome in outcomes)
        {
            AddOutcome(outcome.Item1, outcome.Item2);
        }

        return this;
    }

    public BetSettlementBuilder WithCertainty(int certainty)
    {
        _betSettlement.certainty = certainty;
        return this;
    }

    public BetSettlementBuilder WithTransportTimes(long sentAt, long receivedAt)
    {
        _betSettlement.SentAt = sentAt;
        _betSettlement.ReceivedAt = receivedAt;
        return this;
    }

    public bet_settlement Build()
    {
        _betSettlement.outcomes = _outcomes.ToArray();
        return _betSettlement;
    }

    private static IReadOnlyDictionary<string, string> ParseSpecifiers(string spec)
    {
        if (string.IsNullOrEmpty(spec))
        {
            return new Dictionary<string, string>(0);
        }

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        var parts = spec.Split('|');
        foreach (var p in parts)
        {
            if (string.IsNullOrEmpty(p))
            {
                continue;
            }

            var eq = p.IndexOf('=');
            if (eq <= 0 || eq == p.Length - 1)
            {
                continue;
            }

            var key = p.Substring(0, eq);
            dict[key] = p.Substring(eq + 1);
        }

        return dict;
    }
}
