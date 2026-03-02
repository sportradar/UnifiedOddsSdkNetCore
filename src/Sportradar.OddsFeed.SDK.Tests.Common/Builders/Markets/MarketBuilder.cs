// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed.Messages;

public sealed class MarketBuilder
{
    private readonly market _market = new market();
    private readonly List<marketOutcome> _outcomes = [];

    public static MarketBuilder Create()
    {
        return new MarketBuilder();
    }

    public MarketBuilder WithId(int id)
    {
        _market.id = id;
        return this;
    }

    public MarketBuilder WithSpecifiers(string specifiers)
    {
        _market.specifiers = specifiers;
        _market.Specifiers = ParseSpecifiers(specifiers);
        return this;
    }

    public MarketBuilder WithExtendedSpecifiers(string extendedSpecifiers)
    {
        _market.extended_specifiers = string.IsNullOrEmpty(extendedSpecifiers) ? null : extendedSpecifiers;
        return this;
    }

    public MarketBuilder WithVoidReason(int voidReason)
    {
        _market.void_reason = voidReason;
        _market.void_reasonSpecified = true;
        return this;
    }

    public MarketBuilder WithoutVoidReason()
    {
        _market.void_reason = default;
        _market.void_reasonSpecified = false;
        return this;
    }

    public MarketBuilder WithValidationFailed(bool validationFailed = true)
    {
        _market.ValidationFailed = validationFailed;
        return this;
    }

    public MarketBuilder WithParsedSpecifiers(IReadOnlyDictionary<string, string> specifiers)
    {
        _market.Specifiers = specifiers ?? new Dictionary<string, string>(0);
        return this;
    }

    public MarketBuilder AddOutcome(string outcomeId)
    {
        _outcomes.Add(new marketOutcome
        {
            id = outcomeId
        });
        return this;
    }

    public market Build()
    {
        if (_outcomes.Count != 0)
        {
            _market.outcome = _outcomes.ToArray();
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
        var parts = spec.Split('|', StringSplitOptions.RemoveEmptyEntries);

        foreach (var p in parts)
        {
            var eq = p.IndexOf('=', StringComparison.Ordinal);
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
