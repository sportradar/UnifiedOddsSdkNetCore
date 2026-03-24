// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Markets;

public sealed class BetSettlementMarketBuilder
{
    private readonly betSettlementMarket _market = new();
    private readonly List<betSettlementMarketOutcome> _outcomes = [];

    public static BetSettlementMarketBuilder Create()
    {
        return new BetSettlementMarketBuilder();
    }

    public BetSettlementMarketBuilder WithId(int id)
    {
        _market.id = id;
        return this;
    }

    public BetSettlementMarketBuilder WithSpecifiers(string specifiers)
    {
        _market.specifiers = specifiers;
        _market.Specifiers = ParseSpecifiers(specifiers);
        return this;
    }

    public BetSettlementMarketBuilder WithExtendedSpecifiers(string extendedSpecifiers)
    {
        _market.extended_specifiers = string.IsNullOrEmpty(extendedSpecifiers) ? null : extendedSpecifiers;
        return this;
    }

    public BetSettlementMarketBuilder WithVoidReason(int voidReason)
    {
        _market.void_reason = voidReason;
        _market.void_reasonSpecified = true;
        return this;
    }

    public BetSettlementMarketBuilder AddOutcome(int id, int result)
    {
        _outcomes.Add(new betSettlementMarketOutcome
        {
            id = id.ToString(),
            result = result
        });
        return this;
    }

    public BetSettlementMarketBuilder AddOutcome(string id, int result)
    {
        _outcomes.Add(new betSettlementMarketOutcome
        {
            id = id,
            result = result
        });
        return this;
    }

    public betSettlementMarket Build()
    {
        _market.Items = [];
        if (_outcomes.Count != 0)
        {
            _market.Items = _outcomes.ToArray();
        }

        return _market;
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
