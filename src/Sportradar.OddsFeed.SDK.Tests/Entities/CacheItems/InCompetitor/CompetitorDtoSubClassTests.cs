// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorDtoSubClassTests : CompetitorSetup
{
    [Fact]
    public void DivisionWhenNormalConstructor()
    {
        var divisionDto = new DivisionDto(1, DefaultDivisionName);

        Assert.NotNull(divisionDto);
        Assert.Equal(1, divisionDto.Id);
        Assert.Equal(DefaultDivisionName, divisionDto.Name);
    }

    [Fact]
    public void DivisionWhenConstructorWithNullId()
    {
        var divisionDto = new DivisionDto(null, DefaultDivisionName);

        Assert.NotNull(divisionDto);
        Assert.Null(divisionDto.Id);
        Assert.Equal(DefaultDivisionName, divisionDto.Name);
    }

    [Fact]
    public void DivisionWhenConstructorWithNullName()
    {
        var divisionDto = new DivisionDto(1, null);

        Assert.NotNull(divisionDto);
        Assert.Equal(1, divisionDto.Id);
        Assert.Null(divisionDto.Name);
    }

    [Fact]
    public void DivisionWhenConstructorWithNullIdAndName()
    {
        var divisionDto = new DivisionDto(null, null);

        Assert.NotNull(divisionDto);
        Assert.Null(divisionDto.Id);
        Assert.Null(divisionDto.Name);
    }

    [Fact]
    public void PlayerCompetitorWhenConstructor()
    {
        const int playerId = 123;
        var apiPlayerCompetitor = GetApiPlayerCompetitor(playerId);

        var playerCompetitorDto = new PlayerCompetitorDto(apiPlayerCompetitor);

        ValidatePlayerCompetitor(apiPlayerCompetitor, playerCompetitorDto);
    }

    [Fact]
    public void PlayerCompetitorWhenConstructorWithNull()
    {
        Assert.Throws<NullReferenceException>(() => new PlayerCompetitorDto(null));
    }

    [Fact]
    public void PlayerCompetitorWhenConstructorWithoutIdThenThrows()
    {
        var apiPlayerCompetitor = GetApiPlayerCompetitor();
        apiPlayerCompetitor.id = null;

        var ex = Assert.Throws<ArgumentNullException>(() => new PlayerCompetitorDto(apiPlayerCompetitor));

        Assert.Contains("(Parameter 'id')", ex.Message);
    }

    [Fact]
    public void PlayerCompetitorWhenConstructorWithoutName()
    {
        var apiPlayerCompetitor = GetApiPlayerCompetitor();
        apiPlayerCompetitor.name = null;

        var playerCompetitorDto = new PlayerCompetitorDto(apiPlayerCompetitor);

        ValidatePlayerCompetitor(apiPlayerCompetitor, playerCompetitorDto);
    }

    [Fact]
    public void PlayerCompetitorWhenConstructorWithoutAbbreviation()
    {
        var apiPlayerCompetitor = GetApiPlayerCompetitor();
        apiPlayerCompetitor.abbreviation = null;

        var playerCompetitorDto = new PlayerCompetitorDto(apiPlayerCompetitor);

        ValidatePlayerCompetitor(apiPlayerCompetitor, playerCompetitorDto);
    }

    [Fact]
    public void PlayerCompetitorWhenConstructorWithoutNationality()
    {
        var apiPlayerCompetitor = GetApiPlayerCompetitor();
        apiPlayerCompetitor.nationality = null;

        var playerCompetitorDto = new PlayerCompetitorDto(apiPlayerCompetitor);

        ValidatePlayerCompetitor(apiPlayerCompetitor, playerCompetitorDto);
    }

    [Fact]
    public void RaceDriverProfileWhenNormalConstructor()
    {
        var apiRaceDriverProfile = GetApiRaceDriverProfile();

        var raceDriverProfileDto = new RaceDriverProfileDto(apiRaceDriverProfile);

        Assert.NotNull(raceDriverProfileDto);
        Assert.NotNull(raceDriverProfileDto.Car);
        Assert.NotNull(raceDriverProfileDto.RaceDriverId);
        Assert.NotNull(raceDriverProfileDto.RaceTeamId);
    }

    [Fact]
    public void RaceDriverProfileWhenConstructorWithNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new RaceDriverProfileDto(null));

        Assert.Contains("(Parameter 'item')", ex.Message);
    }

    [Fact]
    public void RaceDriverProfileWhenConstructorWithNullCar()
    {
        var apiRaceDriverProfile = GetApiRaceDriverProfile();
        apiRaceDriverProfile.car = null;

        var raceDriverProfileDto = new RaceDriverProfileDto(apiRaceDriverProfile);

        Assert.NotNull(raceDriverProfileDto);
        Assert.Null(raceDriverProfileDto.Car);
        Assert.NotNull(raceDriverProfileDto.RaceDriverId);
        Assert.NotNull(raceDriverProfileDto.RaceTeamId);
    }

    [Fact]
    public void RaceDriverProfileWhenConstructorWithNullRaceDriverId()
    {
        var apiRaceDriverProfile = GetApiRaceDriverProfile();
        apiRaceDriverProfile.race_driver = null;

        var raceDriverProfileDto = new RaceDriverProfileDto(apiRaceDriverProfile);

        Assert.NotNull(raceDriverProfileDto);
        Assert.NotNull(raceDriverProfileDto.Car);
        Assert.Null(raceDriverProfileDto.RaceDriverId);
        Assert.NotNull(raceDriverProfileDto.RaceTeamId);
    }

    [Fact]
    public void RaceDriverProfileWhenConstructorWithNullRaceTeamId()
    {
        var apiRaceDriverProfile = GetApiRaceDriverProfile();
        apiRaceDriverProfile.race_team = null;

        var raceDriverProfileDto = new RaceDriverProfileDto(apiRaceDriverProfile);

        Assert.NotNull(raceDriverProfileDto);
        Assert.NotNull(raceDriverProfileDto.Car);
        Assert.NotNull(raceDriverProfileDto.RaceDriverId);
        Assert.Null(raceDriverProfileDto.RaceTeamId);
    }

    [Fact]
    public void CarWhenNormalConstructor()
    {
        var apiCar = new car { chassis = "some-car-chassis", engine_name = "some-engine-name", name = "some-car-name" };

        var carDto = new CarDto(apiCar);

        Assert.NotNull(carDto);
        Assert.Equal(apiCar.name, carDto.Name);
        Assert.Equal(apiCar.engine_name, carDto.EngineName);
        Assert.Equal(apiCar.chassis, carDto.Chassis);
    }

    [Fact]
    public void CarWhenConstructorWithNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new CarDto(null));

        Assert.Contains("(Parameter 'item')", ex.Message);
    }

    [Fact]
    public void ManagerWhenConstructor()
    {
        var apiManager = GetApiManager();

        var managerDto = new ManagerDto(apiManager);

        Assert.NotNull(managerDto);
        Assert.Equal(apiManager.name, managerDto.Name);
        Assert.Equal(apiManager.country_code, managerDto.CountryCode);
        Assert.Equal(apiManager.nationality, managerDto.Nationality);
    }

    [Fact]
    public void ManagerWhenConstructorWithNull()
    {
        Assert.Throws<NullReferenceException>(() => new ManagerDto(null));
    }

    [Fact]
    public void ManagerWhenConstructorWithNullId()
    {
        var apiManager = GetApiManager();
        apiManager.id = null;

        var ex = Assert.Throws<ArgumentNullException>(() => new ManagerDto(apiManager));

        Assert.Contains("(Parameter 'id')", ex.Message);
    }

    [Fact]
    public void ManagerWhenConstructorWithoutName()
    {
        var apiManager = GetApiManager();
        apiManager.name = null;

        var managerDto = new ManagerDto(apiManager);

        Assert.NotNull(managerDto);
        Assert.Null(managerDto.Name);
        Assert.Equal(apiManager.name, managerDto.Name);
        Assert.Equal(apiManager.country_code, managerDto.CountryCode);
        Assert.Equal(apiManager.nationality, managerDto.Nationality);
    }

    [Fact]
    public void JerseyWhenConstructorDirectValues()
    {
        var apiJersey = GetApiJersey();

        var jerseyDto = new JerseyDto(apiJersey);

        Assert.NotNull(jerseyDto);
        Assert.Equal(apiJersey.@base, jerseyDto.BaseColor);
        Assert.Equal(apiJersey.horizontal_stripes_color, jerseyDto.HorizontalStripesColor);
        Assert.Equal(apiJersey.number, jerseyDto.Number);
        Assert.Equal(apiJersey.shirt_type, jerseyDto.ShirtType);
        Assert.Equal(apiJersey.sleeve, jerseyDto.SleeveColor);
        Assert.Equal(apiJersey.sleeve_detail, jerseyDto.SleeveDetail);
        Assert.Equal(apiJersey.split_color, jerseyDto.SplitColor);
        Assert.Equal(apiJersey.squares_color, jerseyDto.SquareColor);
        Assert.Equal(apiJersey.stripes_color, jerseyDto.StripesColor);
        Assert.Equal(apiJersey.type, jerseyDto.Type);
    }

    [Fact]
    public void JerseyWhenConstructorSpecifiedValues()
    {
        var apiJersey = GetApiJersey();

        var jerseyDto = new JerseyDto(apiJersey);

        Assert.NotNull(jerseyDto);
        Assert.Equal(apiJersey.horizontal_stripes, jerseyDto.HorizontalStripes);
        Assert.Equal(apiJersey.split, jerseyDto.Split);
        Assert.Equal(apiJersey.squares, jerseyDto.Squares);
        Assert.Equal(apiJersey.stripes, jerseyDto.Stripes);
    }

    [Fact]
    public void JerseyWhenConstructorWithNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new JerseyDto(null));

        Assert.Contains("(Parameter 'item')", ex.Message);
    }

    [Fact]
    public void JerseyWhenNotSpecifiedHorizontalStripes()
    {
        var apiJersey = GetApiJersey();
        apiJersey.horizontal_stripesSpecified = false;

        var jerseyDto = new JerseyDto(apiJersey);

        Assert.Null(jerseyDto.HorizontalStripes);
    }

    [Fact]
    public void JerseyWhenNotSpecifiedSplit()
    {
        var apiJersey = GetApiJersey();
        apiJersey.splitSpecified = false;

        var jerseyDto = new JerseyDto(apiJersey);

        Assert.Null(jerseyDto.Split);
    }

    [Fact]
    public void JerseyWhenNotSpecifiedSquares()
    {
        var apiJersey = GetApiJersey();
        apiJersey.squaresSpecified = false;

        var jerseyDto = new JerseyDto(apiJersey);

        Assert.Null(jerseyDto.Squares);
    }
    [Fact]
    public void JerseyWhenNotSpecifiedStripes()
    {
        var apiJersey = GetApiJersey();
        apiJersey.stripesSpecified = false;

        var jerseyDto = new JerseyDto(apiJersey);

        Assert.Null(jerseyDto.Stripes);
    }
}
