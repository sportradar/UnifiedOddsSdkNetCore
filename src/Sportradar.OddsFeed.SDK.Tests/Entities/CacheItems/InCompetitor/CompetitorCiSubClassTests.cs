// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorCiSubClassTests : CompetitorSetup
{
    [Fact]
    public void DivisionWhenConstructor()
    {
        var divisionCi = new DivisionCacheItem(new DivisionDto(1, DefaultDivisionName));

        Assert.NotNull(divisionCi);
        Assert.Equal(1, divisionCi.Id);
        Assert.Equal(DefaultDivisionName, divisionCi.Name);
    }

    [Fact]
    public void DivisionWhenConstructorWithNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new DivisionCacheItem((DivisionDto)null));
    }

    [Fact]
    public void DivisionWhenConstructorWithNullExportableThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new DivisionCacheItem((ExportableDivision)null));
    }

    [Fact]
    public void DivisionWhenExport()
    {
        var divisionCi = new DivisionCacheItem(new DivisionDto(1, DefaultDivisionName));

        var exportableDivision = divisionCi.Export();

        Assert.NotNull(exportableDivision);
        Assert.Equal(1, exportableDivision.Id);
        Assert.Equal(DefaultDivisionName, exportableDivision.Name);
    }

    [Fact]
    public void DivisionWhenImport()
    {
        var divisionCi = new DivisionCacheItem(new DivisionDto(1, DefaultDivisionName));
        var exportableDivision = divisionCi.Export();

        var importedCi = new DivisionCacheItem(exportableDivision);

        Assert.NotNull(importedCi);
        Assert.Equal(divisionCi.Id, importedCi.Id);
        Assert.Equal(divisionCi.Name, exportableDivision.Name);
    }

    [Fact]
    public void RaceDriverProfileWhenConstructor()
    {
        var dto = new RaceDriverProfileDto(GetApiRaceDriverProfile(1));
        var ci = new RaceDriverProfileCacheItem(dto);

        Assert.NotNull(ci);
        Assert.NotNull(ci.Car);
        Assert.Equal(dto.Car.Chassis, ci.Car.Chassis);
        Assert.Equal(dto.Car.Name, ci.Car.Name);
        Assert.Equal(dto.Car.EngineName, ci.Car.EngineName);
        Assert.Equal(dto.RaceDriverId, ci.RaceDriverId);
        Assert.Equal(dto.RaceTeamId, ci.RaceTeamId);
    }

    [Fact]
    public void RaceDriverProfileWhenConstructorWithNullDtoThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new RaceDriverProfileCacheItem((RaceDriverProfileDto)null));
    }

    [Fact]
    public void RaceDriverProfileWhenConstructorWithNullExportableThenThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new RaceDriverProfileCacheItem((ExportableRaceDriverProfile)null));
    }

    [Fact]
    public async Task RaceDriverProfileWhenExport()
    {
        var dto = new RaceDriverProfileDto(GetApiRaceDriverProfile(1));
        var ci = new RaceDriverProfileCacheItem(dto);

        var exported = await ci.ExportAsync();

        Assert.NotNull(exported);
        Assert.NotNull(exported.Car);
        Assert.Equal(ci.Car.Chassis, exported.Car.Chassis);
        Assert.Equal(ci.Car.Name, exported.Car.Name);
        Assert.Equal(ci.Car.EngineName, exported.Car.EngineName);
        Assert.Equal(ci.RaceDriverId.ToString(), exported.RaceDriverId);
        Assert.Equal(ci.RaceTeamId.ToString(), exported.RaceTeamId);
    }

    [Fact]
    public async Task RaceDriverProfileWhenExportWithCarNull()
    {
        var apiRaceDriver = GetApiRaceDriverProfile(1);
        apiRaceDriver.car = null;
        var dto = new RaceDriverProfileDto(apiRaceDriver);
        var ci = new RaceDriverProfileCacheItem(dto);

        var exported = await ci.ExportAsync();

        Assert.NotNull(exported);
        Assert.Null(exported.Car);
    }

    [Fact]
    public async Task RaceDriverProfileWhenImport()
    {
        var dto = new RaceDriverProfileDto(GetApiRaceDriverProfile(1));
        var ci = new RaceDriverProfileCacheItem(dto);
        var exported = await ci.ExportAsync();

        var imported = new RaceDriverProfileCacheItem(exported);

        Assert.NotNull(imported);
        Assert.NotNull(imported.Car);
        Assert.Equal(exported.Car.Chassis, imported.Car.Chassis);
        Assert.Equal(exported.Car.Name, imported.Car.Name);
        Assert.Equal(exported.Car.EngineName, imported.Car.EngineName);
        Assert.Equal(exported.RaceDriverId, imported.RaceDriverId.ToString());
        Assert.Equal(exported.RaceTeamId, imported.RaceTeamId.ToString());
    }

    [Fact]
    public void CarWhenConstructor()
    {
        var apiCar = GetApiCar();

        var carCi = new CarCacheItem(new CarDto(apiCar));

        Assert.NotNull(carCi);
        Assert.Equal(apiCar.name, carCi.Name);
        Assert.Equal(apiCar.engine_name, carCi.EngineName);
        Assert.Equal(apiCar.chassis, carCi.Chassis);
    }

    [Fact]
    public void CarWhenConstructorWithNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new CarCacheItem((CarDto)null));

        Assert.Contains("(Parameter 'item')", ex.Message);
    }

    [Fact]
    public void CarWhenConstructorWithNullExportable()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new CarCacheItem((ExportableCar)null));

        Assert.Contains("exportable", ex.Message);
    }

    [Fact]
    public async Task CarWhenExport()
    {
        var apiCar = GetApiCar();
        var carCi = new CarCacheItem(new CarDto(apiCar));

        var exported = await carCi.ExportAsync();

        Assert.NotNull(exported);
        Assert.Equal(apiCar.name, exported.Name);
        Assert.Equal(apiCar.engine_name, exported.EngineName);
        Assert.Equal(apiCar.chassis, exported.Chassis);
    }

    [Fact]
    public async Task CarWhenImport()
    {
        var apiCar = GetApiCar();
        var carCi = new CarCacheItem(new CarDto(apiCar));
        var exported = await carCi.ExportAsync();

        var imported = new CarCacheItem(exported);

        Assert.NotNull(imported);
        Assert.Equal(exported.Name, imported.Name);
        Assert.Equal(exported.EngineName, imported.EngineName);
        Assert.Equal(exported.Chassis, imported.Chassis);
    }

    [Fact]
    public void ManagerWhenConstructor()
    {
        var apiManager = GetApiManager();

        var managerCi = new ManagerCacheItem(new ManagerDto(apiManager), TestData.Culture);

        Assert.NotNull(managerCi);
        Assert.Single(managerCi.Name);
        Assert.Equal(apiManager.name, managerCi.Name[TestData.Culture]);
        Assert.Equal(apiManager.country_code, managerCi.CountryCode);
        Assert.Single(managerCi.Nationality);
        Assert.Equal(apiManager.nationality, managerCi.Nationality[TestData.Culture]);
    }

    [Fact]
    public void ManagerWhenConstructorWithNull()
    {
        Assert.Throws<NullReferenceException>(() => new ManagerCacheItem(null, TestData.Culture));
    }

    [Fact]
    public void ManagerWhenConstructorWithNullExportable()
    {
        Assert.Throws<NullReferenceException>(() => new ManagerCacheItem(null));
    }

    [Fact]
    public void ManagerWhenConstructorWithoutName()
    {
        var apiManager = GetApiManager();
        apiManager.name = null;

        var managerCi = new ManagerCacheItem(new ManagerDto(apiManager), TestData.Culture);

        Assert.NotNull(managerCi);
        Assert.Single(managerCi.Name);
        Assert.Null(managerCi.Name.Values.First());
        Assert.Equal(apiManager.country_code, managerCi.CountryCode);
        Assert.Equal(apiManager.nationality, managerCi.Nationality[TestData.Culture]);
    }

    [Fact]
    public void ManagerWhenMerge()
    {
        var apiManager1 = GetApiManager();
        var apiManager2 = GetApiManager();
        apiManager2.name = "Second name";
        apiManager2.nationality = "Second nationality";

        var managerCi = new ManagerCacheItem(new ManagerDto(apiManager1), TestData.Culture);
        managerCi.Merge(new ManagerDto(apiManager2), TestData.CultureNl);

        Assert.NotNull(managerCi);
        Assert.Equal(2, managerCi.Name.Count);
        Assert.Equal(apiManager1.name, managerCi.Name[TestData.Culture]);
        Assert.Equal(apiManager2.name, managerCi.Name[TestData.CultureNl]);
        Assert.Equal(apiManager1.country_code, managerCi.CountryCode);
        Assert.Equal(2, managerCi.Nationality.Count);
        Assert.Equal(apiManager1.nationality, managerCi.Nationality[TestData.Culture]);
        Assert.Equal(apiManager2.nationality, managerCi.Nationality[TestData.CultureNl]);
    }

    [Fact]
    public async Task ManagerWhenExport()
    {
        var apiManager = GetApiManager();
        var managerCi = new ManagerCacheItem(new ManagerDto(apiManager), TestData.Culture);

        var exported = await managerCi.ExportAsync();

        Assert.NotNull(exported);
        Assert.Single(exported.Names);
        Assert.Equal(apiManager.name, exported.Names[TestData.Culture]);
        Assert.Equal(apiManager.country_code, exported.CountryCode);
        Assert.Single(managerCi.Nationality);
        Assert.Equal(apiManager.nationality, exported.Nationality[TestData.Culture]);
    }

    [Fact]
    public async Task ManagerWhenImport()
    {
        var apiManager = GetApiManager();
        var managerCi = new ManagerCacheItem(new ManagerDto(apiManager), TestData.Culture);
        var exported = await managerCi.ExportAsync();

        var imported = new ManagerCacheItem(exported);

        Assert.NotNull(imported);
        Assert.Single(imported.Name);
        Assert.Equal(managerCi.Name[TestData.Culture], imported.Name[TestData.Culture]);
        Assert.Equal(apiManager.country_code, imported.CountryCode);
        Assert.Single(imported.Nationality);
        Assert.Equal(managerCi.Nationality[TestData.Culture], imported.Nationality[TestData.Culture]);
    }

    [Fact]
    public async Task ManagerWhenImport2Cultures()
    {
        var apiManager2 = GetApiManager();
        apiManager2.name = "Second name";
        apiManager2.nationality = "Second nationality";
        var managerCi = new ManagerCacheItem(new ManagerDto(GetApiManager()), TestData.Culture);
        managerCi.Merge(new ManagerDto(apiManager2), TestData.CultureNl);
        var exported = await managerCi.ExportAsync();

        var imported = new ManagerCacheItem(exported);

        Assert.Equal(2, imported.Name.Count);
        Assert.Equal(managerCi.Name[TestData.Culture], imported.Name[TestData.Culture]);
        Assert.Equal(managerCi.Name[TestData.CultureNl], imported.Name[TestData.CultureNl]);
        Assert.Equal(managerCi.CountryCode, imported.CountryCode);
        Assert.Equal(2, imported.Nationality.Count);
        Assert.Equal(managerCi.Nationality[TestData.Culture], imported.Nationality[TestData.Culture]);
        Assert.Equal(managerCi.Nationality[TestData.CultureNl], imported.Nationality[TestData.CultureNl]);
    }

    [Fact]
    public void JerseyWhenConstructorDirectValues()
    {
        var jerseyDto = new JerseyDto(GetApiJersey());

        var jerseyCi = new JerseyCacheItem(jerseyDto);

        Assert.NotNull(jerseyCi);
        Assert.Equal(jerseyCi.BaseColor, jerseyDto.BaseColor);
        Assert.Equal(jerseyCi.HorizontalStripesColor, jerseyDto.HorizontalStripesColor);
        Assert.Equal(jerseyCi.Number, jerseyDto.Number);
        Assert.Equal(jerseyCi.ShirtType, jerseyDto.ShirtType);
        Assert.Equal(jerseyCi.SleeveColor, jerseyDto.SleeveColor);
        Assert.Equal(jerseyCi.SleeveDetail, jerseyDto.SleeveDetail);
        Assert.Equal(jerseyCi.SplitColor, jerseyDto.SplitColor);
        Assert.Equal(jerseyCi.SquareColor, jerseyDto.SquareColor);
        Assert.Equal(jerseyCi.StripesColor, jerseyDto.StripesColor);
        Assert.Equal(jerseyCi.Type, jerseyDto.Type);
    }

    [Fact]
    public void JerseyWhenConstructorSpecifiedValues()
    {
        var jerseyDto = new JerseyDto(GetApiJersey());

        var jerseyCi = new JerseyCacheItem(jerseyDto);

        Assert.NotNull(jerseyCi);
        Assert.Equal(jerseyCi.HorizontalStripes, jerseyDto.HorizontalStripes);
        Assert.Equal(jerseyCi.Split, jerseyDto.Split);
        Assert.Equal(jerseyCi.Squares, jerseyDto.Squares);
        Assert.Equal(jerseyCi.Stripes, jerseyDto.Stripes);
    }

    [Fact]
    public void JerseyWhenConstructorWithNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new JerseyCacheItem((JerseyDto)null));

        Assert.Contains("(Parameter 'item')", ex.Message);
    }

    [Fact]
    public void JerseyWhenConstructorWithNullExportable()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new JerseyCacheItem((ExportableJersey)null));

        Assert.Contains("(Parameter 'exportable')", ex.Message);
    }

    [Fact]
    public void JerseyWhenNotSpecifiedHorizontalStripes()
    {
        var apiJersey = GetApiJersey();
        apiJersey.horizontal_stripesSpecified = false;

        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        Assert.Null(jerseyCi.HorizontalStripes);
    }

    [Fact]
    public void JerseyWhenNotSpecifiedSplit()
    {
        var apiJersey = GetApiJersey();
        apiJersey.splitSpecified = false;

        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        Assert.Null(jerseyCi.Split);
    }

    [Fact]
    public void JerseyWhenNotSpecifiedSquares()
    {
        var apiJersey = GetApiJersey();
        apiJersey.squaresSpecified = false;

        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        Assert.Null(jerseyCi.Squares);
    }
    [Fact]
    public void JerseyWhenNotSpecifiedStripes()
    {
        var apiJersey = GetApiJersey();
        apiJersey.stripesSpecified = false;

        var jerseyCi = new JerseyCacheItem(new JerseyDto(apiJersey));

        Assert.Null(jerseyCi.Stripes);
    }

    [Fact]
    public async Task JerseyWhenExport()
    {
        var jerseyCi = new JerseyCacheItem(new JerseyDto(GetApiJersey()));

        var exported = await jerseyCi.ExportAsync();

        Assert.Equal(jerseyCi.BaseColor, exported.BaseColor);
        Assert.Equal(jerseyCi.HorizontalStripesColor, exported.HorizontalStripesColor);
        Assert.Equal(jerseyCi.Number, exported.Number);
        Assert.Equal(jerseyCi.ShirtType, exported.ShirtType);
        Assert.Equal(jerseyCi.SleeveColor, exported.SleeveColor);
        Assert.Equal(jerseyCi.SleeveDetail, exported.SleeveDetail);
        Assert.Equal(jerseyCi.SplitColor, exported.SplitColor);
        Assert.Equal(jerseyCi.SquareColor, exported.SquareColor);
        Assert.Equal(jerseyCi.StripesColor, exported.StripesColor);
        Assert.Equal(jerseyCi.Type, exported.Type);
        Assert.Equal(jerseyCi.HorizontalStripes, exported.HorizontalStripes);
        Assert.Equal(jerseyCi.Split, exported.Split);
        Assert.Equal(jerseyCi.Squares, exported.Squares);
        Assert.Equal(jerseyCi.Stripes, exported.Stripes);
    }

    [Fact]
    public async Task JerseyWhenImport()
    {
        var jerseyCi = new JerseyCacheItem(new JerseyDto(GetApiJersey()));
        var exported = await jerseyCi.ExportAsync();

        var imported = new JerseyCacheItem(exported);

        Assert.Equal(jerseyCi.BaseColor, imported.BaseColor);
        Assert.Equal(jerseyCi.HorizontalStripesColor, imported.HorizontalStripesColor);
        Assert.Equal(jerseyCi.Number, imported.Number);
        Assert.Equal(jerseyCi.ShirtType, imported.ShirtType);
        Assert.Equal(jerseyCi.SleeveColor, imported.SleeveColor);
        Assert.Equal(jerseyCi.SleeveDetail, imported.SleeveDetail);
        Assert.Equal(jerseyCi.SplitColor, imported.SplitColor);
        Assert.Equal(jerseyCi.SquareColor, imported.SquareColor);
        Assert.Equal(jerseyCi.StripesColor, imported.StripesColor);
        Assert.Equal(jerseyCi.Type, imported.Type);
        Assert.Equal(jerseyCi.HorizontalStripes, imported.HorizontalStripes);
        Assert.Equal(jerseyCi.Split, imported.Split);
        Assert.Equal(jerseyCi.Squares, imported.Squares);
        Assert.Equal(jerseyCi.Stripes, imported.Stripes);
    }

    [Fact]
    public void ReferenceIdWhenConstructor()
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

        Assert.NotNull(referenceCi);
        Assert.Equal(5, referenceCi.ReferenceIds.Count);
        Assert.Equal(12345, referenceCi.BetradarId);
        Assert.Equal(2222, referenceCi.BetfairId);
        Assert.Equal(1111, referenceCi.RotationNumber);
        Assert.Equal(9876, referenceCi.AamsId);
    }

    [Fact]
    public void ReferenceIdWhenConstructorWithNullDictionary()
    {
        var referenceCi = new ReferenceIdCacheItem(null);

        Assert.NotNull(referenceCi);
        Assert.Null(referenceCi.ReferenceIds);
        Assert.Equal(0, referenceCi.BetradarId);
        Assert.Equal(0, referenceCi.BetfairId);
        Assert.Equal(0, referenceCi.RotationNumber);
        Assert.Null(referenceCi.AamsId);
    }

    [Fact]
    public void ReferenceIdWhenConstructorWithEmptyDictionary()
    {
        var referenceCi = new ReferenceIdCacheItem(new Dictionary<string, string>());

        Assert.NotNull(referenceCi);
        Assert.Empty(referenceCi.ReferenceIds);
        Assert.Equal(0, referenceCi.BetradarId);
        Assert.Equal(0, referenceCi.BetfairId);
        Assert.Equal(0, referenceCi.RotationNumber);
        Assert.Null(referenceCi.AamsId);
    }

    [Fact]
    public void ReferenceIdWhenConstructorWithAdditionalUnknownValue()
    {
        var references = new Dictionary<string, string>
        {
            { "betradar", "12345" },
            { "UnknownKey", "12345" },
        };

        var referenceCi = new ReferenceIdCacheItem(references);

        Assert.NotNull(referenceCi);
        Assert.Equal(2, referenceCi.ReferenceIds.Count);
        Assert.Equal(12345, referenceCi.BetradarId);
        Assert.Equal(0, referenceCi.BetfairId);
        Assert.Equal(0, referenceCi.RotationNumber);
        Assert.Null(referenceCi.AamsId);
    }

    [Fact]
    public void ReferenceIdWhenMerge()
    {
        var references1 = new Dictionary<string, string>
        {
            { "betradar", "11111" },
            { "BetradarCtrl", "11111" },
            { "betfair", "11" },
            { "rotation_number", "111" },
            { "aams", "1" }
        };
        var references2 = new Dictionary<string, string>
        {
            { "betradar", "22222" },
            { "BetradarCtrl", "22222" },
            { "betfair", "22" },
            { "rotation_number", "222" },
            { "aams", "2" }
        };

        var referenceCi = new ReferenceIdCacheItem(references1);
        referenceCi.Merge(references2, true);

        Assert.NotNull(referenceCi);
        Assert.Equal(5, referenceCi.ReferenceIds.Count);
        Assert.Equal(22222, referenceCi.BetradarId);
        Assert.Equal(22, referenceCi.BetfairId);
        Assert.Equal(222, referenceCi.RotationNumber);
        Assert.Equal(2, referenceCi.AamsId);
    }

    [Fact]
    public void ReferenceIdWhenMergeWithNull()
    {
        var references1 = new Dictionary<string, string>
        {
            { "betradar", "11111" },
            { "BetradarCtrl", "11111" },
            { "betfair", "11" },
            { "rotation_number", "111" },
            { "aams", "1" }
        };

        var referenceCi = new ReferenceIdCacheItem(references1);
        referenceCi.Merge(null, true);

        Assert.NotNull(referenceCi);
        Assert.Equal(5, referenceCi.ReferenceIds.Count);
        Assert.Equal(11111, referenceCi.BetradarId);
        Assert.Equal(11, referenceCi.BetfairId);
        Assert.Equal(111, referenceCi.RotationNumber);
        Assert.Equal(1, referenceCi.AamsId);
    }
}
