// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class PlayerProfileCiTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly playerExtended _defaultPlayer;
    private readonly playerExtended _playerWithoutJerseyNumber;
    private readonly playerCompetitor _defaultPlayerCompetitor;
    private readonly Urn _competitorId = Urn.Parse("sr:competitor:12345");
    private readonly IDataRouterManager _dataRouterManager;

    public PlayerProfileCiTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _defaultPlayer = GetPlayerExtended();
        _playerWithoutJerseyNumber = GetPlayerExtended();
        _playerWithoutJerseyNumber.jersey_numberSpecified = false;

        _defaultPlayerCompetitor = new playerCompetitor
        {
            id = "sr:player:12345",
            name = "John Smith",
            nationality = "uk",
            abbreviation = "JOH"
        };
        _dataRouterManager = new TestDataRouterManager(new CacheManager(), outputHelper);
    }

    [Fact]
    public void Player()
    {
        var sportEntityDto = new SportEntityDto("sr:player:1", "Sport Entity Name");

        var playerCacheItem = new SportEntityCacheItem(sportEntityDto);

        Assert.NotNull(playerCacheItem);
        Assert.Equal(sportEntityDto.Id, playerCacheItem.Id);
    }

    [Fact]
    public void PlayerProfileCreate()
    {
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem);
        Compare(playerProfileCacheItem, _defaultPlayer, TestData.Culture);
    }

    [Fact]
    public void PlayerProfileCreateWithoutJerseyNumber()
    {
        var playerProfileDto = new PlayerProfileDto(_playerWithoutJerseyNumber, DateTime.Now);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem);
        Compare(playerProfileCacheItem, _playerWithoutJerseyNumber, TestData.Culture);
    }

    [Fact]
    public void PlayerCompetitorCreate()
    {
        var playerCompetitorDto = new PlayerCompetitorDto(_defaultPlayerCompetitor);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, TestData.Culture, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem);
        Assert.Equal(_defaultPlayerCompetitor.id, playerProfileCacheItem.Id.ToString());
        Assert.Equal(_defaultPlayerCompetitor.name, playerProfileCacheItem.GetName(TestData.Culture));
        Assert.Equal(_defaultPlayerCompetitor.nationality, playerProfileCacheItem.GetNationality(TestData.Culture));
        Assert.Equal(_defaultPlayerCompetitor.abbreviation, playerProfileCacheItem.Abbreviation);
    }

    [Fact]
    public void PlayerCompetitorNewCulture()
    {
        var playerCompetitorDto = new PlayerCompetitorDto(_defaultPlayerCompetitor);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, TestData.Culture, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem);
        Assert.Equal(_defaultPlayerCompetitor.id, playerProfileCacheItem.Id.ToString());
        Assert.Equal(_defaultPlayerCompetitor.name, playerProfileCacheItem.GetName(TestData.Culture));
        Assert.Equal(_defaultPlayerCompetitor.nationality, playerProfileCacheItem.GetNationality(TestData.Culture));
        Assert.Equal(_defaultPlayerCompetitor.abbreviation, playerProfileCacheItem.Abbreviation);

        Assert.Null(playerProfileCacheItem.GetName(TestData.CultureNl));
        Assert.Null(playerProfileCacheItem.GetNationality(TestData.CultureNl));
    }

    [Fact]
    public async Task PlayerCompetitorExportImport()
    {
        var playerCompetitorDto = new PlayerCompetitorDto(_defaultPlayerCompetitor);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, TestData.Culture, _dataRouterManager);

        var exportable = await playerProfileCacheItem.ExportAsync();

        Assert.NotNull(exportable);
        var json = JsonConvert.SerializeObject(exportable, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
        var importable = JsonConvert.DeserializeObject<ExportablePlayerProfile>(json);
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(importable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    [Fact]
    public void PlayerCompetitorAddProfile()
    {
        _defaultPlayerCompetitor.nationality = null;
        var playerCompetitorDto = new PlayerCompetitorDto(_defaultPlayerCompetitor);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, TestData.Culture, _dataRouterManager);

        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto);

        playerProfileCacheItem.Merge(playerProfileDto, _competitorId, TestData.CultureNl);

        Compare(playerProfileCacheItem, _defaultPlayer, TestData.CultureNl);
    }

    [Fact]
    public void PlayerProfileMerge()
    {
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, _dataRouterManager);
        Assert.NotNull(playerProfileCacheItem);

        Compare(playerProfileCacheItem, _defaultPlayer, TestData.Culture);

        _defaultPlayer.name = "John SecondName";
        _defaultPlayer.nationality = "uk 2";
        var playerProfileDto2 = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto2);

        playerProfileCacheItem.Merge(playerProfileDto2, _competitorId, TestData.CultureNl);

        Assert.NotEqual(_defaultPlayer.name, playerProfileCacheItem.GetName(TestData.Culture));
        Assert.NotEqual(_defaultPlayer.nationality, playerProfileCacheItem.GetNationality(TestData.Culture));

        Compare(playerProfileCacheItem, _defaultPlayer, TestData.CultureNl);
    }

    [Fact]
    public void PlayerProfileMergeNoData()
    {
        _defaultPlayer.nationality = null;
        _defaultPlayer.nickname = null;
        _defaultPlayer.weightSpecified = false;
        _defaultPlayer.jersey_numberSpecified = false;
        _defaultPlayer.country_code = null;
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, _dataRouterManager);
        Assert.NotNull(playerProfileCacheItem);

        Compare(playerProfileCacheItem, _defaultPlayer, TestData.Culture);

        _defaultPlayer.name = "John Second Name";
        var playerProfileDto2 = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto2);

        playerProfileCacheItem.Merge(playerProfileDto2, _competitorId, TestData.CultureNl);

        Compare(playerProfileCacheItem, _defaultPlayer, TestData.CultureNl);
    }

    [Fact]
    public async Task PlayerProfileExportImportCreate()
    {
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, new TestDataRouterManager(new CacheManager(), _outputHelper));

        var exportable = await playerProfileCacheItem.ExportAsync();
        Assert.NotNull(exportable);
        var json = JsonConvert.SerializeObject(exportable, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
        var importable = JsonConvert.DeserializeObject<ExportablePlayerProfile>(json);
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(importable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    [Fact]
    public async Task PlayerProfileExportImport()
    {
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, new TestDataRouterManager(new CacheManager(), _outputHelper));

        var exportable = (ExportablePlayerProfile)await playerProfileCacheItem.ExportAsync();
        playerProfileCacheItem.Import(exportable);
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(exportable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    [Fact]
    public async Task PlayerProfileExportImportWithoutJerseyNumber()
    {
        var playerProfileDto = new PlayerProfileDto(_playerWithoutJerseyNumber, DateTime.Now);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, new TestDataRouterManager(new CacheManager(), _outputHelper));

        var exportable = (ExportablePlayerProfile)await playerProfileCacheItem.ExportAsync();
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(exportable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    [Fact]
    public async Task PlayerProfileExportImportMissingData()
    {
        _defaultPlayer.nationality = null;
        _defaultPlayer.nickname = null;
        _defaultPlayer.weightSpecified = false;
        _defaultPlayer.jersey_numberSpecified = false;
        _defaultPlayer.country_code = null;
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, TestData.Culture, new TestDataRouterManager(new CacheManager(), _outputHelper));

        var exportable = (ExportablePlayerProfile)await playerProfileCacheItem.ExportAsync();
        playerProfileCacheItem.Import(exportable);
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(exportable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    private static void Compare(PlayerProfileCacheItem playerCacheItem, PlayerProfileCacheItem playerCacheItem2)
    {
        Assert.Equal(playerCacheItem.Id, playerCacheItem2.Id);
        Assert.Equal(playerCacheItem.Abbreviation, playerCacheItem2.Abbreviation);
        Assert.Equal(playerCacheItem.CompetitorId, playerCacheItem2.CompetitorId);
        Assert.Equal(playerCacheItem.CountryCode, playerCacheItem2.CountryCode);
        Assert.Equal(playerCacheItem.DateOfBirth, playerCacheItem2.DateOfBirth);
        Assert.Equal(playerCacheItem.FullName, playerCacheItem2.FullName);
        Assert.Equal(playerCacheItem.Gender, playerCacheItem2.Gender);
        Assert.Equal(playerCacheItem.Names.Count, playerCacheItem2.Names.Count);
        foreach (var culture in playerCacheItem.Names.Keys)
        {
            Assert.Equal(playerCacheItem.GetName(culture), playerCacheItem2.GetName(culture));
            Assert.Equal(playerCacheItem.GetNationality(culture), playerCacheItem2.GetNationality(culture));
        }
        Assert.Equal(playerCacheItem.Height, playerCacheItem2.Height);
        Assert.Equal(playerCacheItem.Nickname, playerCacheItem2.Nickname);
        Assert.Equal(playerCacheItem.Weight, playerCacheItem2.Weight);
        Assert.Equal(playerCacheItem.Type, playerCacheItem2.Type);
    }

    private static playerExtended GetPlayerExtended()
    {
        return new playerExtended
        {
            type = "what",
            country_code = "uk",
            date_of_birth = "2020-09-25",
            full_name = "John Smith",
            gender = "male",
            height = 180,
            heightSpecified = true,
            id = "sr:player:12345",
            jersey_number = 50,
            jersey_numberSpecified = true,
            name = "John Smith",
            nationality = "uk",
            nickname = "smithy",
            weight = 85,
            weightSpecified = true
        };
    }

    private void Compare(PlayerProfileCacheItem playerCacheItem, playerExtended player, CultureInfo culture)
    {
        Assert.NotNull(playerCacheItem);
        Assert.NotNull(player);

        Assert.Equal(player.id, playerCacheItem.Id.ToString());
        Assert.Equal(_competitorId, playerCacheItem.CompetitorId);
        ComparePlayerCharacteristics(playerCacheItem, player);
        ComparePlayerDirectValues(playerCacheItem, player);

        if (player.full_name == null)
        {
            Assert.Null(playerCacheItem.FullName);
        }
        else
        {
            Assert.Equal(player.full_name, playerCacheItem.FullName);
        }
        if (player.name == null)
        {
            Assert.Null(playerCacheItem.GetName(culture));
        }
        else
        {
            Assert.Equal(player.name, playerCacheItem.GetName(culture));
        }
        if (player.nationality == null)
        {
            Assert.Null(playerCacheItem.GetNationality(culture));
        }
        else
        {
            Assert.Equal(player.nationality, playerCacheItem.GetNationality(culture));
        }
        if (player.name == null)
        {
            Assert.Null(playerCacheItem.Abbreviation);
        }
        else
        {
            Assert.Equal(SdkInfo.GetAbbreviationFromName(player.name), playerCacheItem.Abbreviation);
        }
    }

    private static void ComparePlayerCharacteristics(PlayerProfileCacheItem playerCacheItem, playerExtended player)
    {
        if (player.nickname == null)
        {
            Assert.Null(playerCacheItem.Nickname);
        }
        else
        {
            Assert.Equal(player.nickname, playerCacheItem.Nickname);
        }
        if (player.date_of_birth == null)
        {
            Assert.Null(playerCacheItem.DateOfBirth);
        }
        else
        {
            Assert.NotNull(playerCacheItem.DateOfBirth);
            Assert.Equal(player.date_of_birth, playerCacheItem.DateOfBirth.Value.ToString("yyyy-MM-dd"));
        }
        if (player.gender == null)
        {
            Assert.Null(playerCacheItem.Gender);
        }
        else
        {
            Assert.Equal(player.gender, playerCacheItem.Gender);
        }
        if (!player.heightSpecified)
        {
            Assert.Null(playerCacheItem.Height);
        }
        else
        {
            Assert.True(player.height > 0);
            Assert.Equal(player.height, playerCacheItem.Height);
        }
        if (!player.weightSpecified)
        {
            Assert.Null(playerCacheItem.Weight);
        }
        else
        {
            Assert.True(player.weight > 0);
            Assert.Equal(player.weight, playerCacheItem.Weight);
        }
    }

    private static void ComparePlayerDirectValues(PlayerProfileCacheItem playerCacheItem, playerExtended player)
    {
        if (player.type == null)
        {
            Assert.Null(playerCacheItem.Type);
        }
        else
        {
            Assert.Equal(player.type, playerCacheItem.Type);
        }
        if (player.country_code == null)
        {
            Assert.Null(playerCacheItem.CountryCode);
        }
        else
        {
            Assert.Equal(player.country_code, playerCacheItem.CountryCode);
        }
    }
}
