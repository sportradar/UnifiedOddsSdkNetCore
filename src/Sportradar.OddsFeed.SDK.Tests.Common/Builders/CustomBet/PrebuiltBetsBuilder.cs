// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

public sealed class PrebuiltBetsBuilder
{
    private int _requestedRecommendations;
    private string _generatedAt;
    private Urn _eventId;
    private int _providedRecommendations;
    private List<RecommendationsType> _recommendations;

    private PrebuiltBetsBuilder()
    {
    }

    public static PrebuiltBetsBuilder Create()
    {
        return new PrebuiltBetsBuilder();
    }

    public PrebuiltBetsBuilder WithRequestedRecommendations(int requestedRecommendations)
    {
        _requestedRecommendations = requestedRecommendations;
        return this;
    }

    public PrebuiltBetsBuilder WithGeneratedAt(string generatedAt)
    {
        _generatedAt = generatedAt;
        return this;
    }

    public PrebuiltBetsBuilder WithEventId(Urn eventId)
    {
        _eventId = eventId;
        return this;
    }

    public PrebuiltBetsBuilder WithProvidedRecommendations(int providedRecommendations)
    {
        _providedRecommendations = providedRecommendations;
        return this;
    }

    public PrebuiltBetsBuilder AddRecommendation(Func<PrebuiltBetsRecommendationBuilder> recommendationBuilder)
    {
        _recommendations ??= [];
        _recommendations.Add(recommendationBuilder().Build());
        return this;
    }

    public PreBuiltBetsType Build()
    {
#pragma warning disable format
        var result = new PreBuiltBetsType
        {
            requested_recommendations = _requestedRecommendations,
            generated_at = _generatedAt,
            @event =
                [
                    new EventRecommendationsType
                        {
                            id = _eventId.ToString(),
                            provided_recommendation = _providedRecommendations,
                            recommendations = _recommendations.ToArray()
                        }
                ]
        };
#pragma warning restore format
        return result;
    }
}
