// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

public class FilteredCalculationResponseBuilder
{
    private double _odds;
    private double _probability;
    private bool? _harmonization;
    private Urn _eventId;

    public static FilteredCalculationResponseBuilder Create()
    {
        return new FilteredCalculationResponseBuilder();
    }

    public FilteredCalculationResponseBuilder WithOdds(double odds)
    {
        _odds = odds;
        return this;
    }

    public FilteredCalculationResponseBuilder WithProbability(double probability)
    {
        _probability = probability;
        return this;
    }

    public FilteredCalculationResponseBuilder WithHarmonization(bool harmonization)
    {
        _harmonization = harmonization;
        return this;
    }

    public FilteredCalculationResponseBuilder WithEventId(Urn eventId)
    {
        _eventId = eventId;
        return this;
    }

    public FilteredCalculationResponseType Build()
    {
        var filteredEventType = new FilteredEventType { id = _eventId.ToString(), markets = [] };
        var filteredCalculationResultType = new FilteredCalculationResultType
        {
            odds = _odds,
            probability = _probability,
            harmonization = _harmonization.HasValue && _harmonization.Value,
            harmonizationSpecified = _harmonization.HasValue
        };
        var result = new FilteredCalculationResponseType
        {
            calculation = filteredCalculationResultType,
            generated_at = DateTime.Now.ToString("o"),
            available_selections = [filteredEventType]
        };
        return result;
    }
}
