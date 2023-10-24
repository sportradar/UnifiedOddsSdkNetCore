using System.Net;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems;

public class RestMapperHelperTests
{
    [Theory]
    [InlineData(bonusDrumType.additional, BonusDrumType.Additional)]
    [InlineData(bonusDrumType.same, BonusDrumType.Same)]
    public void BonusDrumStringTypeIsConvertedToEnum(bonusDrumType inputType, BonusDrumType expectedType)
    {
        var convertedType = RestMapperHelper.MapBonusDrumType(inputType, true);

        Assert.True(convertedType.HasValue);
        Assert.Equal(expectedType, convertedType);
    }

    [Fact]
    public void BonusDrumNotPresentReturnsDefault()
    {
        var convertedType = RestMapperHelper.MapBonusDrumType(bonusDrumType.additional, false);

        Assert.False(convertedType.HasValue);
    }

    [Theory]
    [InlineData(drawType.drum, DrawType.Drum)]
    [InlineData(drawType.rng, DrawType.Rng)]
    public void DrawTypeStringTypeIsConvertedToEnum(drawType inputType, DrawType expectedType)
    {
        var convertedType = RestMapperHelper.MapDrawType(inputType, true);

        Assert.Equal(expectedType, convertedType);
    }

    [Fact]
    public void DrawTypeNotPresentReturnsDefault()
    {
        var convertedType = RestMapperHelper.MapDrawType(drawType.drum, false);

        Assert.Equal(DrawType.Unknown, convertedType);
    }

    [Theory]
    [InlineData(drawStatus.open, DrawStatus.Open)]
    [InlineData(drawStatus.canceled, DrawStatus.Cancelled)]
    [InlineData(drawStatus.closed, DrawStatus.Closed)]
    [InlineData(drawStatus.finished, DrawStatus.Finished)]
    public void DrawStatusStringTypeIsConvertedToEnum(drawStatus inputType, DrawStatus expectedType)
    {
        var convertedType = RestMapperHelper.MapDrawStatus(inputType, true);

        Assert.Equal(expectedType, convertedType);
    }

    [Fact]
    public void DrawStatusNotPresentReturnsDefault()
    {
        var convertedType = RestMapperHelper.MapDrawStatus(drawStatus.open, false);

        Assert.Equal(DrawStatus.Unknown, convertedType);
    }

    [Theory]
    [InlineData(timeType.interval, TimeType.Interval)]
    [InlineData(timeType.@fixed, TimeType.Fixed)]
    public void TimeTypeStringTypeIsConvertedToEnum(timeType inputCode, TimeType expectedType)
    {
        var convertedType = RestMapperHelper.MapTimeType(inputCode, true);

        Assert.Equal(expectedType, convertedType);
    }

    [Fact]
    public void TimeTypeNotPresentReturnsDefault()
    {
        var convertedType = RestMapperHelper.MapTimeType(timeType.interval, false);

        Assert.Equal(TimeType.Unknown, convertedType);
    }

    [Theory]
    [InlineData(response_code.ACCEPTED, HttpStatusCode.Accepted)]
    [InlineData(response_code.BAD_REQUEST, HttpStatusCode.BadRequest)]
    [InlineData(response_code.CONFLICT, HttpStatusCode.Conflict)]
    [InlineData(response_code.CREATED, HttpStatusCode.Created)]
    [InlineData(response_code.FORBIDDEN, HttpStatusCode.Forbidden)]
    [InlineData(response_code.MOVED_PERMANENTLY, HttpStatusCode.MovedPermanently)]
    [InlineData(response_code.NOT_FOUND, HttpStatusCode.NotFound)]
    [InlineData(response_code.NOT_IMPLEMENTED, HttpStatusCode.NotImplemented)]
    [InlineData(response_code.OK, HttpStatusCode.OK)]
    [InlineData(response_code.SERVICE_UNAVAILABLE, HttpStatusCode.ServiceUnavailable)]
    public void ResponseCodeStringTypeIsConvertedToEnum(response_code inputCode, HttpStatusCode expectedType)
    {
        var convertedType = RestMapperHelper.MapResponseCode(inputCode, true);

        Assert.True(convertedType.HasValue);
        Assert.Equal(expectedType, convertedType);
    }

    [Fact]
    public void ResponseCodeNotPresentReturnsNull()
    {
        var convertedType = RestMapperHelper.MapResponseCode(response_code.ACCEPTED, false);

        Assert.False(convertedType.HasValue);
    }

    [Theory]
    [InlineData("not_booked", BookingStatus.Bookable)]
    [InlineData("bookable", BookingStatus.Bookable)]
    [InlineData("booked", BookingStatus.Booked)]
    [InlineData("buyable", BookingStatus.Buyable)]
    [InlineData("not_available", BookingStatus.Unavailable)]
    public void BookingStatusStringTypeIsConvertedToEnum(string typeAsString, BookingStatus expectedType)
    {
        var result = RestMapperHelper.TryGetBookingStatus(typeAsString, out var convertedType);

        Assert.True(result);
        Assert.Equal(expectedType, convertedType);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void BookingStatusInvalidTypeReturnsNull(string typeAsString)
    {
        var result = RestMapperHelper.TryGetBookingStatus(typeAsString, out var convertedType);

        Assert.False(result);
        Assert.False(convertedType.HasValue);
    }

    [Theory]
    [InlineData("parent", StageType.Parent)]
    [InlineData("child", StageType.Child)]
    [InlineData("event", StageType.Event)]
    [InlineData("season", StageType.Season)]
    [InlineData("round", StageType.Round)]
    [InlineData("competition_group", StageType.CompetitionGroup)]
    [InlineData("discipline", StageType.Discipline)]
    [InlineData("dicipline", StageType.Discipline)]
    [InlineData("race", StageType.Race)]
    [InlineData("stage", StageType.Stage)]
    [InlineData("practice", StageType.Practice)]
    [InlineData("qualifying", StageType.Qualifying)]
    [InlineData("qualifying_part", StageType.QualifyingPart)]
    [InlineData("lap", StageType.Lap)]
    [InlineData("prologue", StageType.Prologue)]
    [InlineData("run", StageType.Run)]
    [InlineData("sprint_race", StageType.SprintRace)]
    public void StageStringTypeIsConvertedToEnum(string stageTypeString, StageType expectedStageType)
    {
        var result = RestMapperHelper.TryGetStageType(stageTypeString, out var convertedStageType);

        Assert.True(result);
        Assert.Equal(expectedStageType, convertedStageType);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void StageInvalidTypeReturnsNull(string typeAsString)
    {
        var result = RestMapperHelper.TryGetStageType(typeAsString, out var convertedType);

        Assert.False(result);
        Assert.False(convertedType.HasValue);
    }

    [Fact]
    public void SpringRaceIsConvertedToEnum()
    {
        var result = RestMapperHelper.TryGetStageType("sprint_race", out var convertedStageType);

        Assert.True(result);
        Assert.Equal(StageType.SprintRace, convertedStageType.Value);
    }

    [Theory]
    [InlineData("parent", SportEventType.Parent)]
    [InlineData("child", SportEventType.Child)]
    public void SportEventStringTypeIsConvertedToEnum(string typeAsString, SportEventType expectedType)
    {
        var result = RestMapperHelper.TryGetSportEventType(typeAsString, out var convertedType);

        Assert.True(result);
        Assert.Equal(expectedType, convertedType);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void SportEventInvalidTypeReturnsNull(string typeAsString)
    {
        var result = RestMapperHelper.TryGetSportEventType(typeAsString, out var convertedSportEventType);

        Assert.False(result);
        Assert.False(convertedSportEventType.HasValue);
    }

    [Theory]
    [InlineData("tv", CoveredFrom.Tv)]
    [InlineData("venue", CoveredFrom.Venue)]
    public void CoveredFromStringTypeIsConvertedToEnum(string typeAsString, CoveredFrom expectedType)
    {
        var result = RestMapperHelper.TryGetCoveredFrom(typeAsString, out var convertedType);

        Assert.True(result);
        Assert.Equal(expectedType, convertedType);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void CoveredFromInvalidTypeReturnsNull(string typeAsString)
    {
        var result = RestMapperHelper.TryGetCoveredFrom(typeAsString, out var convertedSportEventType);

        Assert.False(result);
        Assert.False(convertedSportEventType.HasValue);
    }
}
