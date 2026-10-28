// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <draw_info draw_type="rng" time_type="fixed" game_type="6/42"/>
public class DrawInfoBuilder
{
    private readonly lotteryDraw_info _drawInfo = new lotteryDraw_info();

    public DrawInfoBuilder WithDrawType(drawType drawType)
    {
        _drawInfo.draw_type = drawType;
        _drawInfo.draw_typeSpecified = true;
        return this;
    }

    public DrawInfoBuilder WithTimeType(timeType timeType)
    {
        _drawInfo.time_type = timeType;
        _drawInfo.time_typeSpecified = true;
        return this;
    }

    public DrawInfoBuilder WithGameType(string gameType)
    {
        _drawInfo.game_type = gameType;
        return this;
    }

    public lotteryDraw_info Build()
    {
        return _drawInfo;
    }
}
