// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class DescMatchStatusSportsBuilder
{
    private readonly List<desc_match_statusSportsSport> _sports = new();
    private bool _all;

    public DescMatchStatusSportsBuilder AddSport(Func<DescMatchStatusSportsSportBuilder, DescMatchStatusSportsSportBuilder> builderFunc)
    {
        var builder = builderFunc(new DescMatchStatusSportsSportBuilder());
        _sports.Add(builder.Build());
        return this;
    }

    public DescMatchStatusSportsBuilder WithAll(bool all = true)
    {
        _all = all;
        return this;
    }

    public desc_match_statusSports Build()
    {
        return new desc_match_statusSports
        {
            all = _all,
            sport = _sports.ToArray()
        };
    }
}
