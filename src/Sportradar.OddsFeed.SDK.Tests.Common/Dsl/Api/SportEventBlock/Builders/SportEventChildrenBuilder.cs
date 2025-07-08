// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <sport_event id="sr:stage:1190059" type="child" stage_type="practice" scheduled="2025-03-21T03:30:00+00:00"/>
public class SportEventChildrenBuilder
{
    private readonly sportEventChildrenSport_event _sportEvent = new sportEventChildrenSport_event();

    public SportEventChildrenBuilder WithId(Urn id)
    {
        _sportEvent.id = id.ToString();
        return this;
    }

    public SportEventChildrenBuilder WithName(string name)
    {
        _sportEvent.name = name;
        return this;
    }

    public SportEventChildrenBuilder WithType(string type)
    {
        _sportEvent.type = type;
        return this;
    }

    public SportEventChildrenBuilder WithScheduledTime(DateTime? scheduledTime)
    {
        _sportEvent.scheduled = scheduledTime ?? DateTime.MinValue;
        _sportEvent.scheduledSpecified = scheduledTime != null;
        return this;
    }

    public SportEventChildrenBuilder WithScheduledEndTime(DateTime? scheduledEndTime)
    {
        _sportEvent.scheduled_end = scheduledEndTime ?? DateTime.MinValue;
        _sportEvent.scheduled_endSpecified = scheduledEndTime != null;
        return this;
    }

    public SportEventChildrenBuilder WithStartTimeTbd(bool? tbd = true)
    {
        _sportEvent.start_time_tbdSpecified = false;
        if (!tbd.HasValue)
        {
            return this;
        }
        _sportEvent.start_time_tbd = tbd.Value;
        _sportEvent.start_time_tbdSpecified = true;

        return this;
    }

    public SportEventChildrenBuilder WithStageType(string stageType)
    {
        _sportEvent.stage_type = stageType;
        return this;
    }

    public SportEventChildrenBuilder WithReplacedBy(Urn replacedBy)
    {
        _sportEvent.replaced_by = replacedBy.ToString();
        return this;
    }

    public sportEventChildrenSport_event Build()
    {
        return _sportEvent;
    }
}
