using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems;

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
        var childStage = new sportEventChildrenSport_event()
        {
            id = "sr:stage:123",
            scheduledSpecified = true,
            scheduled = new DateTime(DateTime.Now.Year, 2, 17),
            scheduled_endSpecified = true,
            scheduled_end = new DateTime(DateTime.Now.Year, 2, 18)
        };
        childStage.id = "sr:stage:123";
        childStage.stage_type = "child";

        var stageDto = new StageDto(childStage);

        Assert.NotNull(stageDto);
        Assert.Equal(childStage.id, stageDto.Id.ToString());
        Assert.Equal(StageType.Child, stageDto.StageType);
    }
}
