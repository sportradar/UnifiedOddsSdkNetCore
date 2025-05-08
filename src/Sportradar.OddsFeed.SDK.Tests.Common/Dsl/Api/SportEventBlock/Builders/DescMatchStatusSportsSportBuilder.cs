// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class DescMatchStatusSportsSportBuilder
{
    private readonly desc_match_statusSportsSport _sport = new desc_match_statusSportsSport();

    public DescMatchStatusSportsSportBuilder WithId(string id)
    {
        _sport.id = id;
        return this;
    }

    public desc_match_statusSportsSport Build()
    {
        return _sport;
    }
}
