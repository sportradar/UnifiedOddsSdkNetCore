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
    private readonly CultureInfo _cultureFirst = new CultureInfo("en");
    private readonly CultureInfo _cultureSecond = new CultureInfo("de");
    private readonly playerExtended _defaultPlayer;
    private readonly playerCompetitor _defaultPlayerCompetitor;
    private readonly Urn _competitorId = Urn.Parse("sr:competitor:12345");
    private readonly IDataRouterManager _dataRouterManager;

    public PlayerProfileCiTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _defaultPlayer = new playerExtended
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

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, _cultureFirst, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem);
        Compare(playerProfileCacheItem, _defaultPlayer, _cultureFirst);
    }

    [Fact]
    public void PlayerCompetitorCreate()
    {
        var playerCompetitorDto = new PlayerCompetitorDto(_defaultPlayerCompetitor);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, _cultureFirst, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem);
        Assert.Equal(_defaultPlayerCompetitor.id, playerProfileCacheItem.Id.ToString());
        Assert.Equal(_defaultPlayerCompetitor.name, playerProfileCacheItem.GetName(_cultureFirst));
        Assert.Equal(_defaultPlayerCompetitor.nationality, playerProfileCacheItem.GetNationality(_cultureFirst));
        Assert.Equal(_defaultPlayerCompetitor.abbreviation, playerProfileCacheItem.Abbreviation);
    }

    [Fact]
    public void PlayerCompetitorNewCulture()
    {
        var playerCompetitorDto = new PlayerCompetitorDto(_defaultPlayerCompetitor);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, _cultureFirst, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem);
        Assert.Equal(_defaultPlayerCompetitor.id, playerProfileCacheItem.Id.ToString());
        Assert.Equal(_defaultPlayerCompetitor.name, playerProfileCacheItem.GetName(_cultureFirst));
        Assert.Equal(_defaultPlayerCompetitor.nationality, playerProfileCacheItem.GetNationality(_cultureFirst));
        Assert.Equal(_defaultPlayerCompetitor.abbreviation, playerProfileCacheItem.Abbreviation);

        Assert.Null(playerProfileCacheItem.GetName(_cultureSecond));
        Assert.Null(playerProfileCacheItem.GetNationality(_cultureSecond));
    }

    [Fact]
    public async Task PlayerCompetitorExportImport()
    {
        var playerCompetitorDto = new PlayerCompetitorDto(_defaultPlayerCompetitor);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, _cultureFirst, _dataRouterManager);

        var exportable = await playerProfileCacheItem.ExportAsync().ConfigureAwait(false);

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
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerCompetitorDto, _competitorId, _cultureFirst, _dataRouterManager);

        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto);

        playerProfileCacheItem.Merge(playerProfileDto, _competitorId, _cultureSecond);

        Compare(playerProfileCacheItem, _defaultPlayer, _cultureSecond);
    }

    [Fact]
    public void PlayerProfileMerge()
    {
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto);

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, _cultureFirst, _dataRouterManager);
        Assert.NotNull(playerProfileCacheItem);

        Compare(playerProfileCacheItem, _defaultPlayer, _cultureFirst);

        _defaultPlayer.name = "John SecondName";
        _defaultPlayer.nationality = "uk 2";
        var playerProfileDto2 = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto2);

        playerProfileCacheItem.Merge(playerProfileDto2, _competitorId, _cultureSecond);

        Assert.NotEqual(_defaultPlayer.name, playerProfileCacheItem.GetName(_cultureFirst));
        Assert.NotEqual(_defaultPlayer.nationality, playerProfileCacheItem.GetNationality(_cultureFirst));

        Compare(playerProfileCacheItem, _defaultPlayer, _cultureSecond);
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

        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, _cultureFirst, _dataRouterManager);
        Assert.NotNull(playerProfileCacheItem);

        Compare(playerProfileCacheItem, _defaultPlayer, _cultureFirst);

        _defaultPlayer.name = "John Second Name";
        var playerProfileDto2 = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        Assert.NotNull(playerProfileDto2);

        playerProfileCacheItem.Merge(playerProfileDto2, _competitorId, _cultureSecond);

        Compare(playerProfileCacheItem, _defaultPlayer, _cultureSecond);
    }

    [Fact]
    public void PlayerProfileExportImportCreate()
    {
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));

        var exportable = playerProfileCacheItem.ExportAsync().GetAwaiter().GetResult();
        Assert.NotNull(exportable);
        var json = JsonConvert.SerializeObject(exportable, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
        var importable = JsonConvert.DeserializeObject<ExportablePlayerProfile>(json);
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(importable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    [Fact]
    public void PlayerProfileExportImport()
    {
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));

        var exportable = (ExportablePlayerProfile)playerProfileCacheItem.ExportAsync().GetAwaiter().GetResult();
        playerProfileCacheItem.Import(exportable);
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(exportable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    [Fact]
    public void PlayerProfileExportImportMissingData()
    {
        _defaultPlayer.nationality = null;
        _defaultPlayer.nickname = null;
        _defaultPlayer.weightSpecified = false;
        _defaultPlayer.jersey_numberSpecified = false;
        _defaultPlayer.country_code = null;
        var playerProfileDto = new PlayerProfileDto(_defaultPlayer, DateTime.Now);
        var playerProfileCacheItem = new PlayerProfileCacheItem(playerProfileDto, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));

        var exportable = (ExportablePlayerProfile)playerProfileCacheItem.ExportAsync().GetAwaiter().GetResult();
        playerProfileCacheItem.Import(exportable);
        var playerProfileCacheItem2 = new PlayerProfileCacheItem(exportable, _dataRouterManager);

        Assert.NotNull(playerProfileCacheItem2);

        Compare(playerProfileCacheItem, playerProfileCacheItem2);
    }

    private void Compare(PlayerProfileCacheItem playerCacheItem, PlayerProfileCacheItem playerCacheItem2)
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

    private void Compare(PlayerProfileCacheItem playerCacheItem, playerExtended player, CultureInfo culture)
    {
        Assert.NotNull(playerCacheItem);
        Assert.NotNull(player);

        Assert.Equal(player.id, playerCacheItem.Id.ToString());
        Assert.Equal(_competitorId, playerCacheItem.CompetitorId);
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
        if (player.date_of_birth == null)
        {
            Assert.Null(playerCacheItem.DateOfBirth);
        }
        else
        {
            Assert.NotNull(playerCacheItem.DateOfBirth);
            Assert.Equal(player.date_of_birth, playerCacheItem.DateOfBirth.Value.ToString("yyyy-MM-dd"));
        }
        if (player.full_name == null)
        {
            Assert.Null(playerCacheItem.FullName);
        }
        else
        {
            Assert.Equal(player.full_name, playerCacheItem.FullName);
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
        if (player.type == null)
        {
            Assert.Null(playerCacheItem.Type);
        }
        else
        {
            Assert.Equal(player.type, playerCacheItem.Type);
        }
        //if (!player.jersey_numberSpecified)
        //{
        //    Assert.Null(playerCacheItem.);
        //}
        //else
        //{
        //    Assert.IsTrue(player.height > 0);
        //    Assert.Equal(player.height, playerCacheItem.Height);
        //}
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
        if (player.nickname == null)
        {
            Assert.Null(playerCacheItem.Nickname);
        }
        else
        {
            Assert.Equal(player.nickname, playerCacheItem.Nickname);
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
}
