// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.CustomBet.CustomBetEndpoint;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

public class CalculationResponseBuilder
{
    private double _odds;
    private double _probability;
    private bool? _harmonization;
    private Urn _eventId;

    public static CalculationResponseBuilder Create()
    {
        return new CalculationResponseBuilder();
    }

    public CalculationResponseBuilder WithOdds(double odds)
    {
        _odds = odds;
        return this;
    }

    public CalculationResponseBuilder WithProbability(double probability)
    {
        _probability = probability;
        return this;
    }

    public CalculationResponseBuilder WithHarmonization(bool harmonization)
    {
        _harmonization = harmonization;
        return this;
    }

    public CalculationResponseBuilder WithEventId(Urn eventId)
    {
        _eventId = eventId;
        return this;
    }

    public CalculationResponseType Build()
    {
        var result = new CalculationResponseType
        {
            calculation = new CalculationResultType
            {
                odds = _odds,
                probability = _probability,
                harmonization = _harmonization.HasValue && _harmonization.Value,
                harmonizationSpecified = _harmonization.HasValue
            },
            generated_at = DateTime.Now.ToString("o"),
            available_selections = GetEventTypes(_eventId)
        };
        return result;
    }

    private static EventType[] GetEventTypes(Urn eventId)
    {
        if (eventId is null)
        {
            return [GetAvailableSelectionsFor(TestData.EventMatchId).@event];
        }

        return [GetAvailableSelectionsFor(eventId).@event];
    }
}
