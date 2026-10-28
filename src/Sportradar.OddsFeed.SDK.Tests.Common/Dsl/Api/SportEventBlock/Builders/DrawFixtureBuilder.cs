// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <draw_fixture id="wns:draw:4936143671" draw_date="2026-10-23T01:59:00.000Z" status="finished" display_id="17">
//   <lottery .../>
// </draw_fixture>
public class DrawFixtureBuilder
{
    private readonly draw_fixture _drawFixture = new draw_fixture();

    public DrawFixtureBuilder WithId(Urn id)
    {
        _drawFixture.id = id.ToString();
        return this;
    }

    public DrawFixtureBuilder WithDrawDate(DateTime drawDate)
    {
        _drawFixture.draw_date = drawDate;
        _drawFixture.draw_dateSpecified = true;
        return this;
    }

    public DrawFixtureBuilder WithStatus(drawStatus status)
    {
        _drawFixture.status = status;
        _drawFixture.statusSpecified = true;
        return this;
    }

    public DrawFixtureBuilder WithDisplayId(int displayId)
    {
        _drawFixture.display_id = displayId;
        _drawFixture.display_idSpecified = true;
        return this;
    }

    public DrawFixtureBuilder WithLottery(Func<LotteryBuilder, LotteryBuilder> builder)
    {
        _drawFixture.lottery = builder(new LotteryBuilder()).Build();
        return this;
    }

    public draw_fixture Build()
    {
        return _drawFixture;
    }
}
