// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class WeatherInfoBuilder
{
    private readonly weatherInfo _weather = new weatherInfo();

    public weatherInfo Build()
    {
        return _weather;
    }
}
