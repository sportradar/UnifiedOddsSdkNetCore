// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

public sealed class AvailableSelectionsBuilder
{
    private string _generatedAt;
    private Urn _eventId;
    private readonly List<MarketType> _markets = [];

    private AvailableSelectionsBuilder()
    {
    }

    public static AvailableSelectionsBuilder Create()
    {
        return new AvailableSelectionsBuilder();
    }

    public AvailableSelectionsBuilder WithGeneratedAt(string generatedAt)
    {
        _generatedAt = generatedAt;
        return this;
    }

    public AvailableSelectionsBuilder WithEventId(Urn eventId)
    {
        _eventId = eventId;
        return this;
    }

    public AvailableSelectionsBuilder AddMarket(int marketId, string specifiers = null, params string[] outcomeIds)
    {
        var outcomes = new List<OutcomeType>();
        if (outcomeIds != null)
        {
            foreach (var outcomeId in outcomeIds)
            {
                outcomes.Add(new OutcomeType { id = outcomeId });
            }
        }

        _markets.Add(new MarketType
        {
            id = marketId,
            specifiers = specifiers,
            outcome = outcomes.ToArray()
        });

        return this;
    }

    public AvailableSelectionsType Build()
    {
        return new AvailableSelectionsType
        {
            generated_at = string.IsNullOrWhiteSpace(_generatedAt) ? DateTime.UtcNow.ToString(SdkInfo.Iso860124HFullFormat) : _generatedAt,
            @event = new EventType
            {
                id = _eventId?.ToString(),
                markets = _markets.ToArray()
            }
        };
    }
}
