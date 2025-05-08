// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class DescMatchStatusBuilder
{
    private long _id;
    private string _description;
    private int _periodNumber;
    private bool _periodNumberSpecified;
    private desc_match_statusSports _sports;

    public DescMatchStatusBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public DescMatchStatusBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public DescMatchStatusBuilder WithPeriodNumber(int periodNumber)
    {
        _periodNumber = periodNumber;
        _periodNumberSpecified = true;
        return this;
    }

    public DescMatchStatusBuilder WithoutPeriodNumber()
    {
        _periodNumberSpecified = false;
        return this;
    }

    public DescMatchStatusBuilder WithSports(Func<DescMatchStatusSportsBuilder, desc_match_statusSports> builderFunc)
    {
        _sports = builderFunc(new DescMatchStatusSportsBuilder());
        return this;
    }

    public desc_match_status Build()
    {
        return new desc_match_status
        {
            id = _id,
            description = _description,
            period_number = _periodNumber,
            period_numberSpecified = _periodNumberSpecified,
            sports = _sports
        };
    }
}
