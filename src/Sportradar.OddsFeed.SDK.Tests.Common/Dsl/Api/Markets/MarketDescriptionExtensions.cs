// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public static class MarketDescriptionExtensions
{
    public static desc_market AddSuffix(this desc_market market, CultureInfo language)
    {
        market.AddSuffix($" [{language.TwoLetterISOLanguageName.ToUpperInvariant()}]");
        return market;
    }

    public static desc_market AddSuffix(this desc_market market, string suffix)
    {
        market.name += suffix;
        if (market.outcomes != null)
        {
            foreach (var outcome in market.outcomes)
            {
                outcome.name += suffix;
            }
        }
        if (market.mappings == null)
        {
            return market;
        }
        foreach (var mapping in market.mappings)
        {
            if (mapping.mapping_outcome == null)
            {
                continue;
            }
            foreach (var mappingOutcome in mapping.mapping_outcome)
            {
                mappingOutcome.product_outcome_name += suffix;
            }
        }
        return market;
    }

    public static ICollection<desc_market> AddSuffix(this ICollection<desc_market> markets, string suffix)
    {
        if (markets.IsNullOrEmpty())
        {
            return markets;
        }
        foreach (var market in markets)
        {
            market.AddSuffix(suffix);
        }
        return markets;
    }

    public static desc_variant AddSuffix(this desc_variant market, string suffix)
    {
        if (market.outcomes != null)
        {
            foreach (var outcome in market.outcomes)
            {
                outcome.name += suffix;
            }
        }
        if (market.mappings == null)
        {
            return market;
        }
        foreach (var mapping in market.mappings)
        {
            if (mapping.mapping_outcome == null)
            {
                continue;
            }
            foreach (var mappingOutcome in mapping.mapping_outcome)
            {
                mappingOutcome.product_outcome_name += suffix;
            }
        }
        return market;
    }

    public static ICollection<desc_variant> AddSuffix(this ICollection<desc_variant> markets, string suffix)
    {
        if (markets.IsNullOrEmpty())
        {
            return markets;
        }
        foreach (var market in markets)
        {
            market.AddSuffix(suffix);
        }
        return markets;
    }
}
