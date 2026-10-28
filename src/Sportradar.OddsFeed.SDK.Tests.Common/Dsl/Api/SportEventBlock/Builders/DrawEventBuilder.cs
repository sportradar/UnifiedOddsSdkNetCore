// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <draw_event id="wns:draw:4936143671" scheduled="2026-10-23T01:59:00.000Z" status="finished" display_id="17"/>
public class DrawEventBuilder
{
    private readonly draw_event _drawEvent = new draw_event();

    public DrawEventBuilder WithId(Urn id)
    {
        _drawEvent.id = id.ToString();
        return this;
    }

    public DrawEventBuilder WithScheduled(DateTime scheduled)
    {
        _drawEvent.scheduled = scheduled;
        _drawEvent.scheduledSpecified = true;
        return this;
    }

    public DrawEventBuilder WithStatus(drawStatus status)
    {
        _drawEvent.status = status;
        _drawEvent.statusSpecified = true;
        return this;
    }

    public DrawEventBuilder WithDisplayId(int displayId)
    {
        _drawEvent.display_id = displayId;
        _drawEvent.display_idSpecified = true;
        return this;
    }

    public draw_event Build()
    {
        return _drawEvent;
    }
}
