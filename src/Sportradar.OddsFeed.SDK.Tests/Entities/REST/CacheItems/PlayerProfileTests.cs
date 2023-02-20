using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.CacheItems
{
    public class PlayerProfileTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly CultureInfo _cultureFirst = new CultureInfo("en");
        private readonly CultureInfo _cultureSecond = new CultureInfo("de");
        private readonly playerExtended _defaultPlayer;
        private readonly playerCompetitor _defaultPlayerCompetitor;
        private readonly URN _competitorId = URN.Parse("sr:competitor:12345");
        private readonly IDataRouterManager _dataRouterManager;

        public PlayerProfileTests(ITestOutputHelper outputHelper)
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
        public void PlayerTest()
        {
            var sportEntityDTO = new SportEntityDTO("sr:player:1", "Sport Entity Name");
            var playerCI = new SportEntityCI(sportEntityDTO);

            Assert.NotNull(playerCI);
            Assert.Equal(sportEntityDTO.Id, playerCI.Id);
        }

        [Fact]
        public void PlayerProfileCreateTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.NotNull(playerProfileDTO);

            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.NotNull(playerProfileCI);

            Compare(playerProfileCI, _defaultPlayer, _cultureFirst);
        }

        [Fact]
        public void PlayerCompetitorCreateTest()
        {
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            Assert.NotNull(playerCompetitorDTO);

            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.NotNull(playerProfileCI);

            Assert.Equal(_defaultPlayerCompetitor.id, playerProfileCI.Id.ToString());
            Assert.Equal(_defaultPlayerCompetitor.name, playerProfileCI.GetName(_cultureFirst));
            Assert.Equal(_defaultPlayerCompetitor.nationality, playerProfileCI.GetNationality(_cultureFirst));
            Assert.Equal(_defaultPlayerCompetitor.abbreviation, playerProfileCI.Abbreviation);
        }

        [Fact]
        public void PlayerCompetitorNewCultureTest()
        {
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            Assert.NotNull(playerCompetitorDTO);

            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.NotNull(playerProfileCI);

            Assert.Equal(_defaultPlayerCompetitor.id, playerProfileCI.Id.ToString());
            Assert.Equal(_defaultPlayerCompetitor.name, playerProfileCI.GetName(_cultureFirst));
            Assert.Equal(_defaultPlayerCompetitor.nationality, playerProfileCI.GetNationality(_cultureFirst));
            Assert.Equal(_defaultPlayerCompetitor.abbreviation, playerProfileCI.Abbreviation);

            Assert.Null(playerProfileCI.GetName(_cultureSecond));
            Assert.Null(playerProfileCI.GetNationality(_cultureSecond));
        }

        [Fact]
        public async Task PlayerCompetitorExportImportTest()
        {
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);

            var exportable = await playerProfileCI.ExportAsync().ConfigureAwait(false);
            Assert.NotNull(exportable);
            var json = JsonConvert.SerializeObject(exportable, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
            var importable = JsonConvert.DeserializeObject<ExportablePlayerProfileCI>(json);
            var playerProfileCI2 = new PlayerProfileCI(importable, _dataRouterManager);

            Assert.NotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        [Fact]
        public void PlayerCompetitorAddProfileTest()
        {
            _defaultPlayerCompetitor.nationality = null;
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);

            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.NotNull(playerProfileDTO);

            playerProfileCI.Merge(playerProfileDTO, _competitorId, _cultureSecond);

            Compare(playerProfileCI, _defaultPlayer, _cultureSecond);
        }

        [Fact]
        public void PlayerProfileMergeTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.NotNull(playerProfileDTO);

            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.NotNull(playerProfileCI);

            Compare(playerProfileCI, _defaultPlayer, _cultureFirst);

            _defaultPlayer.name = "John SecondName";
            _defaultPlayer.nationality = "uk 2";
            var playerProfileDTO2 = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.NotNull(playerProfileDTO2);

            playerProfileCI.Merge(playerProfileDTO2, _competitorId, _cultureSecond);

            Assert.NotEqual(_defaultPlayer.name, playerProfileCI.GetName(_cultureFirst));
            Assert.NotEqual(_defaultPlayer.nationality, playerProfileCI.GetNationality(_cultureFirst));

            Compare(playerProfileCI, _defaultPlayer, _cultureSecond);
        }

        [Fact]
        public void PlayerProfileMergeNoDataTest()
        {
            _defaultPlayer.nationality = null;
            _defaultPlayer.nickname = null;
            _defaultPlayer.weightSpecified = false;
            _defaultPlayer.jersey_numberSpecified = false;
            _defaultPlayer.country_code = null;
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.NotNull(playerProfileDTO);

            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.NotNull(playerProfileCI);

            Compare(playerProfileCI, _defaultPlayer, _cultureFirst);

            _defaultPlayer.name = "John Second Name";
            var playerProfileDTO2 = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.NotNull(playerProfileDTO2);

            playerProfileCI.Merge(playerProfileDTO2, _competitorId, _cultureSecond);

            Compare(playerProfileCI, _defaultPlayer, _cultureSecond);
        }

        [Fact]
        public void PlayerProfileExportImportCreateTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));

            var exportable = playerProfileCI.ExportAsync().Result;
            Assert.NotNull(exportable);
            var json = JsonConvert.SerializeObject(exportable, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
            var importable = JsonConvert.DeserializeObject<ExportablePlayerProfileCI>(json);
            var playerProfileCI2 = new PlayerProfileCI(importable, _dataRouterManager);

            Assert.NotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        [Fact]
        public void PlayerProfileExportImportTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));

            var exportable = (ExportablePlayerProfileCI)playerProfileCI.ExportAsync().Result;
            playerProfileCI.Import(exportable);
            var playerProfileCI2 = new PlayerProfileCI(exportable, _dataRouterManager);

            Assert.NotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        [Fact]
        public void PlayerProfileExportImportMissingDataTest()
        {
            _defaultPlayer.nationality = null;
            _defaultPlayer.nickname = null;
            _defaultPlayer.weightSpecified = false;
            _defaultPlayer.jersey_numberSpecified = false;
            _defaultPlayer.country_code = null;
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));

            var exportable = (ExportablePlayerProfileCI)playerProfileCI.ExportAsync().Result;
            playerProfileCI.Import(exportable);
            var playerProfileCI2 = new PlayerProfileCI(exportable, _dataRouterManager);

            Assert.NotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        private void Compare(PlayerProfileCI playerCI, PlayerProfileCI playerCI2)
        {
            Assert.Equal(playerCI.Id, playerCI2.Id);
            Assert.Equal(playerCI.Abbreviation, playerCI2.Abbreviation);
            Assert.Equal(playerCI.CompetitorId, playerCI2.CompetitorId);
            Assert.Equal(playerCI.CountryCode, playerCI2.CountryCode);
            Assert.Equal(playerCI.DateOfBirth, playerCI2.DateOfBirth);
            Assert.Equal(playerCI.FullName, playerCI2.FullName);
            Assert.Equal(playerCI.Gender, playerCI2.Gender);
            Assert.Equal(playerCI.Names.Count, playerCI2.Names.Count);
            foreach (var culture in playerCI.Names.Keys)
            {
                Assert.Equal(playerCI.GetName(culture), playerCI2.GetName(culture));
                Assert.Equal(playerCI.GetNationality(culture), playerCI2.GetNationality(culture));
            }
            Assert.Equal(playerCI.Height, playerCI2.Height);
            Assert.Equal(playerCI.Nickname, playerCI2.Nickname);
            Assert.Equal(playerCI.Weight, playerCI2.Weight);
            Assert.Equal(playerCI.Type, playerCI2.Type);
        }

        private void Compare(PlayerProfileCI playerCI, playerExtended player, CultureInfo culture)
        {
            Assert.NotNull(playerCI);
            Assert.NotNull(player);

            Assert.Equal(player.id, playerCI.Id.ToString());
            Assert.Equal(_competitorId, playerCI.CompetitorId);
            if (player.type == null)
            {
                Assert.Null(playerCI.Type);
            }
            else
            {
                Assert.Equal(player.type, playerCI.Type);
            }
            if (player.country_code == null)
            {
                Assert.Null(playerCI.CountryCode);
            }
            else
            {
                Assert.Equal(player.country_code, playerCI.CountryCode);
            }
            if (player.date_of_birth == null)
            {
                Assert.Null(playerCI.DateOfBirth);
            }
            else
            {
                Assert.NotNull(playerCI.DateOfBirth);
                Assert.Equal(player.date_of_birth, playerCI.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
            if (player.full_name == null)
            {
                Assert.Null(playerCI.FullName);
            }
            else
            {
                Assert.Equal(player.full_name, playerCI.FullName);
            }
            if (player.gender == null)
            {
                Assert.Null(playerCI.Gender);
            }
            else
            {
                Assert.Equal(player.gender, playerCI.Gender);
            }
            if (!player.heightSpecified)
            {
                Assert.Null(playerCI.Height);
            }
            else
            {
                Assert.True(player.height > 0);
                Assert.Equal(player.height, playerCI.Height);
            }
            if (!player.weightSpecified)
            {
                Assert.Null(playerCI.Weight);
            }
            else
            {
                Assert.True(player.weight > 0);
                Assert.Equal(player.weight, playerCI.Weight);
            }
            if (player.type == null)
            {
                Assert.Null(playerCI.Type);
            }
            else
            {
                Assert.Equal(player.type, playerCI.Type);
            }
            //if (!player.jersey_numberSpecified)
            //{
            //    Assert.Null(playerCI.);
            //}
            //else
            //{
            //    Assert.IsTrue(player.height > 0);
            //    Assert.Equal(player.height, playerCI.Height);
            //}
            if (player.name == null)
            {
                Assert.Null(playerCI.GetName(culture));
            }
            else
            {
                Assert.Equal(player.name, playerCI.GetName(culture));
            }
            if (player.nationality == null)
            {
                Assert.Null(playerCI.GetNationality(culture));
            }
            else
            {
                Assert.Equal(player.nationality, playerCI.GetNationality(culture));
            }
            if (player.nickname == null)
            {
                Assert.Null(playerCI.Nickname);
            }
            else
            {
                Assert.Equal(player.nickname, playerCI.Nickname);
            }
            if (player.name == null)
            {
                Assert.Null(playerCI.Abbreviation);
            }
            else
            {
                Assert.Equal(SdkInfo.GetAbbreviationFromName(player.name), playerCI.Abbreviation);
            }
        }
    }
}
