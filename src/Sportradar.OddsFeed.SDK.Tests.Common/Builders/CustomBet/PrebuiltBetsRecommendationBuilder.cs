// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

public sealed class PrebuiltBetsRecommendationBuilder
{
    private double _odds;
    private double _probability;
    private List<PreBuiltBetsSelectionType> _selections;

    private PrebuiltBetsRecommendationBuilder()
    {
    }

    public static PrebuiltBetsRecommendationBuilder Create()
    {
        return new PrebuiltBetsRecommendationBuilder();
    }

    public PrebuiltBetsRecommendationBuilder WithOdds(double odds)
    {
        _odds = odds;
        return this;
    }

    public PrebuiltBetsRecommendationBuilder WithProbability(double probability)
    {
        _probability = probability;
        return this;
    }

    public PrebuiltBetsRecommendationBuilder AddSelection(int marketId, string outcomeId, string specifiers = null)
    {
        _selections ??= [];
        _selections.Add(new PreBuiltBetsSelectionType { market_id = marketId, outcome_id = outcomeId, specifiers = specifiers });
        return this;
    }

    public RecommendationsType Build()
    {
        var result = new RecommendationsType
        {
            odds = _odds,
            probability = _probability,
            selection = _selections.ToArray()
        };
        return result;
    }
}
