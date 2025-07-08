// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <parent id="sr:stage:1189123" name="Formula 1 2025" type="parent" stage_type="season"/>
public class ParentStageBuilder
{
    private readonly parentStage _parent = new parentStage();

    public ParentStageBuilder WithId(Urn id)
    {
        _parent.id = id.ToString();
        return this;
    }

    public ParentStageBuilder WithName(string name)
    {
        _parent.name = name;
        return this;
    }

    public ParentStageBuilder WithType(string type)
    {
        _parent.type = type;
        return this;
    }

    public ParentStageBuilder WithScheduledTime(DateTime? scheduledTime)
    {
        _parent.scheduled = scheduledTime ?? DateTime.MinValue;
        _parent.scheduledSpecified = scheduledTime != null;
        return this;
    }

    public ParentStageBuilder WithScheduledEndTime(DateTime? scheduledEndTime)
    {
        _parent.scheduled_end = scheduledEndTime ?? DateTime.MinValue;
        _parent.scheduled_endSpecified = scheduledEndTime != null;
        return this;
    }

    public ParentStageBuilder WithStartTimeTbd(bool? tbd = true)
    {
        _parent.start_time_tbdSpecified = false;
        if (!tbd.HasValue)
        {
            return this;
        }
        _parent.start_time_tbd = tbd.Value;
        _parent.start_time_tbdSpecified = true;

        return this;
    }

    public ParentStageBuilder WithStageType(string stageType)
    {
        _parent.stage_type = stageType;
        return this;
    }

    public ParentStageBuilder WithReplacedBy(Urn replacedBy)
    {
        _parent.replaced_by = replacedBy.ToString();
        return this;
    }

    public parentStage Build()
    {
        return _parent;
    }
}
