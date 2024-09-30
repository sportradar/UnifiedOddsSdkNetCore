// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.OutCompetitor;

public class CompetitorSubClassTests : CompetitorSetup
{
    [Fact]
    public void Division_Constructor()
    {
        var divisionCi = new DivisionCacheItem(new DivisionDto(1, DefaultDivisionName));

        var division = new Division(divisionCi);

        Assert.NotNull(division);
        Assert.Equal(1, division.Id);
        Assert.Equal(DefaultDivisionName, division.Name);
    }

    [Fact]
    public void Division_ConstructorWithNullDto_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Division(null));
    }

    [Fact]
    public void RaceDriverProfile_Constructor()
    {
        var dto = new RaceDriverProfileDto(GetApiRaceDriverProfile(1));
        var ci = new RaceDriverProfileCacheItem(dto);

        var item = new RaceDriverProfile(ci);

        Assert.NotNull(item);
        Assert.NotNull(item.Car);
        Assert.Equal(dto.Car.Chassis, item.Car.Chassis);
        Assert.Equal(dto.Car.Name, item.Car.Name);
        Assert.Equal(dto.Car.EngineName, item.Car.EngineName);
        Assert.Equal(dto.RaceDriverId, item.RaceDriverId);
        Assert.Equal(dto.RaceTeamId, item.RaceTeamId);
    }

    [Fact]
    public void RaceDriverProfile_ConstructorWithNullDto_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RaceDriverProfile(null));
    }

    [Fact]
    public void Car_Constructor()
    {
        var apiCar = GetApiCar();
        var carCi = new CarCacheItem(new CarDto(apiCar));

        var car = new Car(carCi);

        Assert.NotNull(car);
        Assert.Equal(apiCar.name, car.Name);
        Assert.Equal(apiCar.engine_name, car.EngineName);
        Assert.Equal(apiCar.chassis, car.Chassis);
    }

    [Fact]
    public void Car_ConstructorWithNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Car(null));

        Assert.Contains("(Parameter 'car')", ex.Message);
    }

    [Fact]
    public void Manager_Constructor()
    {
        var apiManager = GetApiManager();
        var managerCi = new ManagerCacheItem(new ManagerDto(apiManager), TestData.Culture);

        var manager = new Manager(managerCi);

        Assert.NotNull(manager);
        Assert.Single(manager.Names);
        Assert.Equal(apiManager.name, manager.GetName(TestData.Culture));
        Assert.Equal(apiManager.country_code, manager.CountryCode);
        Assert.Equal(apiManager.nationality, manager.GetNationality(TestData.Culture));
    }

    [Fact]
    public void Manager_ConstructorWithNull()
    {
        Assert.Throws<NullReferenceException>(() => new Manager(null));
    }

    [Fact]
    public void Jersey_ConstructorDirectValues()
    {
        var jerseyDto = new JerseyDto(GetApiJersey());
        var jerseyCi = new JerseyCacheItem(jerseyDto);

        var jersey = new Jersey(jerseyCi);

        Assert.NotNull(jersey);
        Assert.Equal(jerseyCi.BaseColor, jersey.BaseColor);
        Assert.Equal(jerseyCi.HorizontalStripesColor, jersey.HorizontalStripesColor);
        Assert.Equal(jerseyCi.Number, jersey.Number);
        Assert.Equal(jerseyCi.ShirtType, jersey.ShirtType);
        Assert.Equal(jerseyCi.SleeveColor, jersey.SleeveColor);
        Assert.Equal(jerseyCi.SleeveDetail, jersey.SleeveDetail);
        Assert.Equal(jerseyCi.SplitColor, jersey.SplitColor);
        Assert.Equal(jerseyCi.SquareColor, jersey.SquareColor);
        Assert.Equal(jerseyCi.StripesColor, jersey.StripesColor);
        Assert.Equal(jerseyCi.Type, jersey.Type);
    }

    [Fact]
    public void Jersey_ConstructorSpecifiedValues()
    {
        var jerseyDto = new JerseyDto(GetApiJersey());
        var jerseyCi = new JerseyCacheItem(jerseyDto);

        var jersey = new Jersey(jerseyCi);

        Assert.NotNull(jersey);
        Assert.Equal(jerseyCi.HorizontalStripes, jersey.HorizontalStripes);
        Assert.Equal(jerseyCi.Split, jersey.Split);
        Assert.Equal(jerseyCi.Squares, jersey.Squares);
        Assert.Equal(jerseyCi.Stripes, jersey.Stripes);
    }

    [Fact]
    public void Jersey_ConstructorWithNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Jersey(null));

        Assert.Contains("(Parameter 'item')", ex.Message);
    }

    [Fact]
    public void Jersey_NotSpecifiedHorizontalStripes()
    {
        var apiJersey = GetApiJersey();
        apiJersey.horizontal_stripesSpecified = false;
        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        var jersey = new Jersey(jerseyCi);

        Assert.Null(jersey.HorizontalStripes);
    }

    [Fact]
    public void Jersey_NotSpecifiedSplit()
    {
        var apiJersey = GetApiJersey();
        apiJersey.splitSpecified = false;
        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        var jersey = new Jersey(jerseyCi);

        Assert.Null(jersey.Split);
    }

    [Fact]
    public void Jersey_NotSpecifiedSquares()
    {
        var apiJersey = GetApiJersey();
        apiJersey.squaresSpecified = false;
        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        var jersey = new Jersey(jerseyCi);

        Assert.Null(jersey.Squares);
    }

    [Fact]
    public void Jersey_NotSpecifiedStripes()
    {
        var apiJersey = GetApiJersey();
        apiJersey.stripesSpecified = false;
        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        var jersey = new Jersey(jerseyCi);

        Assert.Null(jersey.Stripes);
    }

    [Fact]
    public void ReferenceId_Constructor()
    {
        var references = new Dictionary<string, string>
        {
            { "betradar", "12345" },
            { "BetradarCtrl", "12345" },
            { "betfair", "2222" },
            { "rotation_number", "1111" },
            { "aams", "9876" }
        };
        var referenceCi = new ReferenceIdCacheItem(references);

        var reference = new Reference(referenceCi);

        Assert.NotNull(reference);
        Assert.Equal(5, reference.References.Count);
        Assert.Equal(12345, reference.BetradarId);
        Assert.Equal(2222, reference.BetfairId);
        Assert.Equal(1111, reference.RotationNumber);
        Assert.Equal(9876, reference.AamsId);
    }

    [Fact]
    public void ReferenceId_ConstructorWithNullDictionary()
    {
        var referenceCi = new ReferenceIdCacheItem(null);

        var reference = new Reference(referenceCi);

        Assert.NotNull(reference);
        Assert.Null(reference.References);
        Assert.Equal(0, reference.BetradarId);
        Assert.Equal(0, reference.BetfairId);
        Assert.Equal(0, reference.RotationNumber);
        Assert.Null(reference.AamsId);
    }

    [Fact]
    public void ReferenceId_ConstructorWithEmptyDictionary()
    {
        var referenceCi = new ReferenceIdCacheItem(new Dictionary<string, string>());

        var reference = new Reference(referenceCi);

        Assert.NotNull(reference);
        Assert.Empty(reference.References);
        Assert.Equal(0, reference.BetradarId);
        Assert.Equal(0, reference.BetfairId);
        Assert.Equal(0, reference.RotationNumber);
        Assert.Null(reference.AamsId);
    }

    [Fact]
    public void ReferenceId_ConstructorWithAdditionalUnknownValue()
    {
        var references = new Dictionary<string, string>
        {
            { "betradar", "12345" },
            { "UnknownKey", "12345" },
        };
        var referenceCi = new ReferenceIdCacheItem(references);

        var reference = new Reference(referenceCi);

        Assert.NotNull(reference);
        Assert.Equal(2, reference.References.Count);
        Assert.Equal(12345, reference.BetradarId);
        Assert.Equal(0, reference.BetfairId);
        Assert.Equal(0, reference.RotationNumber);
        Assert.Null(reference.AamsId);
    }
}
