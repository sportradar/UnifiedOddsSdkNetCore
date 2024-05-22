// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InSportEvent;

public class StageDtoTests
{
    [Fact]
    public void ConstructStageDtoFromApiStageSportEvent()
    {
        var sportEvent = MessageFactoryRest.GetSportEventEndpoint(123);
        sportEvent.id = "sr:stage:123";
        sportEvent.stage_type = "race";

        var stageDto = new StageDto(sportEvent);

        Assert.NotNull(stageDto);
        Assert.Equal(sportEvent.id, stageDto.Id.ToString());
        Assert.Equal(StageType.Race, stageDto.StageType);
    }

    [Fact]
    public void ConstructStageDtoFromApiStageSummary()
    {
        var stageSummaryEndpoint = MessageFactoryRest.GetStageSummaryEndpoint(123);
        stageSummaryEndpoint.sport_event.id = "sr:stage:123";
        stageSummaryEndpoint.sport_event.stage_type = "race";

        var stageDto = new StageDto(stageSummaryEndpoint);

        Assert.NotNull(stageDto);
        Assert.Equal(stageSummaryEndpoint.sport_event.id, stageDto.Id.ToString());
        Assert.Equal(StageType.Race, stageDto.StageType);
    }

    [Fact]
    public void ConstructStageDtoFromApiParentStage()
    {
        var stageSummaryEndpoint = MessageFactoryRest.GetParentStageSummaryEndpoint(123);
        stageSummaryEndpoint.id = "sr:stage:123";
        stageSummaryEndpoint.stage_type = "parent";

        var stageDto = new StageDto(stageSummaryEndpoint);

        Assert.NotNull(stageDto);
        Assert.Equal(stageSummaryEndpoint.id, stageDto.Id.ToString());
        Assert.Equal(StageType.Parent, stageDto.StageType);
    }

    [Fact]
    public void ConstructStageDtoFromApiSportEventChildrenSportEvent()
    {
        var scheduledStart = new DateTime(2024, 2, 17, 0, 0, 0, 0, DateTimeKind.Utc);
        var scheduledEnd = new DateTime(2024, 2, 18, 0, 0, 0, 0, DateTimeKind.Utc);

        var childStage = new sportEventChildrenSport_event
        {
            id = "sr:stage:123",
            scheduledSpecified = true,
            scheduled = scheduledStart,
            scheduled_endSpecified = true,
            scheduled_end = scheduledEnd
        };
        childStage.id = "sr:stage:123";
        childStage.stage_type = "child";

        var stageDto = new StageDto(childStage);

        Assert.NotNull(stageDto);
        Assert.Equal(childStage.id, stageDto.Id.ToString());
        Assert.Equal(StageType.Child, stageDto.StageType);
        Assert.Equal(scheduledStart, stageDto.Scheduled);
        Assert.Equal(scheduledEnd, stageDto.ScheduledEnd);
    }
}
