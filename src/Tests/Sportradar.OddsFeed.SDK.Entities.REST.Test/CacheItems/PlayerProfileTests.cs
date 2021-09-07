using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test.CacheItems
{
    [TestClass]
    public class PlayerProfileTests
    {
        private readonly CultureInfo _cultureFirst = new CultureInfo("en");
        private readonly CultureInfo _cultureSecond = new CultureInfo("de");
        private playerExtended _defaultPlayer;
        private playerCompetitor _defaultPlayerCompetitor;
        private readonly URN _competitorId = URN.Parse("sr:competitor:12345");
        private IDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Setup()
        {
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
            _dataRouterManager = new TestDataRouterManager(new CacheManager());
        }

        [TestMethod]
        public void PlayerTest()
        {
            var sportEntityDTO = new SportEntityDTO("sr:player:1", "Sport Entity Name");
            var playerCI = new SportEntityCI(sportEntityDTO);

            Assert.IsNotNull(playerCI);
            Assert.AreEqual(sportEntityDTO.Id, playerCI.Id);
        }

        [TestMethod]
        public void PlayerProfileCreateTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.IsNotNull(playerProfileDTO);

            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.IsNotNull(playerProfileCI);

            Compare(playerProfileCI, _defaultPlayer, _cultureFirst);
        }

        [TestMethod]
        public void PlayerCompetitorCreateTest()
        {
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            Assert.IsNotNull(playerCompetitorDTO);

            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.IsNotNull(playerProfileCI);

            Assert.AreEqual(_defaultPlayerCompetitor.id, playerProfileCI.Id.ToString());
            Assert.AreEqual(_defaultPlayerCompetitor.name, playerProfileCI.GetName(_cultureFirst));
            Assert.AreEqual(_defaultPlayerCompetitor.nationality, playerProfileCI.GetNationality(_cultureFirst));
            Assert.AreEqual(_defaultPlayerCompetitor.abbreviation, playerProfileCI.Abbreviation);
        }

        [TestMethod]
        public void PlayerCompetitorNewCultureTest()
        {
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            Assert.IsNotNull(playerCompetitorDTO);

            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.IsNotNull(playerProfileCI);

            Assert.AreEqual(_defaultPlayerCompetitor.id, playerProfileCI.Id.ToString());
            Assert.AreEqual(_defaultPlayerCompetitor.name, playerProfileCI.GetName(_cultureFirst));
            Assert.AreEqual(_defaultPlayerCompetitor.nationality, playerProfileCI.GetNationality(_cultureFirst));
            Assert.AreEqual(_defaultPlayerCompetitor.abbreviation, playerProfileCI.Abbreviation);

            Assert.IsNull(playerProfileCI.GetName(_cultureSecond));
            Assert.IsNull(playerProfileCI.GetNationality(_cultureSecond));
        }

        [TestMethod]
        public void PlayerCompetitorExportImportTest()
        {
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);

            var exportable = playerProfileCI.ExportAsync().Result;
            Assert.IsNotNull(exportable);
            var json = JsonConvert.SerializeObject(exportable, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            var importable = JsonConvert.DeserializeObject<ExportablePlayerProfileCI>(json);
            var playerProfileCI2 = new PlayerProfileCI(importable, _dataRouterManager);

            Assert.IsNotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        [TestMethod]
        public void PlayerCompetitorAddProfileTest()
        {
            _defaultPlayerCompetitor.nationality = null;
            var playerCompetitorDTO = new PlayerCompetitorDTO(_defaultPlayerCompetitor);
            var playerProfileCI = new PlayerProfileCI(playerCompetitorDTO, _competitorId, _cultureFirst, _dataRouterManager);

            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.IsNotNull(playerProfileDTO);

            playerProfileCI.Merge(playerProfileDTO, _competitorId, _cultureSecond);

            Compare(playerProfileCI, _defaultPlayer, _cultureSecond);
        }

        [TestMethod]
        public void PlayerProfileMergeTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.IsNotNull(playerProfileDTO);

            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.IsNotNull(playerProfileCI);

            Compare(playerProfileCI, _defaultPlayer, _cultureFirst);

            _defaultPlayer.name = "John SecondName";
            _defaultPlayer.nationality = "uk 2";
            var playerProfileDTO2 = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.IsNotNull(playerProfileDTO2);

            playerProfileCI.Merge(playerProfileDTO2, _competitorId, _cultureSecond);

            Assert.AreNotEqual(_defaultPlayer.name, playerProfileCI.GetName(_cultureFirst));
            Assert.AreNotEqual(_defaultPlayer.nationality, playerProfileCI.GetNationality(_cultureFirst));

            Compare(playerProfileCI, _defaultPlayer, _cultureSecond);
        }

        [TestMethod]
        public void PlayerProfileMergeNoDataTest()
        {
            _defaultPlayer.nationality = null;
            _defaultPlayer.nickname = null;
            _defaultPlayer.weightSpecified = false;
            _defaultPlayer.jersey_numberSpecified = false;
            _defaultPlayer.country_code = null;
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.IsNotNull(playerProfileDTO);

            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, _dataRouterManager);
            Assert.IsNotNull(playerProfileCI);

            Compare(playerProfileCI, _defaultPlayer, _cultureFirst);

            _defaultPlayer.name = "John Second Name";
            var playerProfileDTO2 = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            Assert.IsNotNull(playerProfileDTO2);

            playerProfileCI.Merge(playerProfileDTO2, _competitorId, _cultureSecond);

            Compare(playerProfileCI, _defaultPlayer, _cultureSecond);
        }

        [TestMethod]
        public void PlayerProfileExportImportCreateTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager()));

            var exportable = playerProfileCI.ExportAsync().Result;
            Assert.IsNotNull(exportable);
            var json = JsonConvert.SerializeObject(exportable, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            var importable = JsonConvert.DeserializeObject<ExportablePlayerProfileCI>(json);
            var playerProfileCI2 = new PlayerProfileCI(importable, _dataRouterManager);

            Assert.IsNotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        [TestMethod]
        public void PlayerProfileExportImportTest()
        {
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager()));

            var exportable = (ExportablePlayerProfileCI) playerProfileCI.ExportAsync().Result;
            playerProfileCI.Import(exportable);
            var playerProfileCI2 = new PlayerProfileCI(exportable, _dataRouterManager);

            Assert.IsNotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        [TestMethod]
        public void PlayerProfileExportImportMissingDataTest()
        {
            _defaultPlayer.nationality = null;
            _defaultPlayer.nickname = null;
            _defaultPlayer.weightSpecified = false;
            _defaultPlayer.jersey_numberSpecified = false;
            _defaultPlayer.country_code = null;
            var playerProfileDTO = new PlayerProfileDTO(_defaultPlayer, DateTime.Now);
            var playerProfileCI = new PlayerProfileCI(playerProfileDTO, _competitorId, _cultureFirst, new TestDataRouterManager(new CacheManager()));

            var exportable = (ExportablePlayerProfileCI) playerProfileCI.ExportAsync().Result;
            playerProfileCI.Import(exportable);
            var playerProfileCI2 = new PlayerProfileCI(exportable, _dataRouterManager);

            Assert.IsNotNull(playerProfileCI2);

            Compare(playerProfileCI, playerProfileCI2);
        }

        private void Compare(PlayerProfileCI playerCI, PlayerProfileCI playerCI2)
        {
            Assert.AreEqual(playerCI.Id, playerCI2.Id);
            Assert.AreEqual(playerCI.Abbreviation, playerCI2.Abbreviation);
            Assert.AreEqual(playerCI.CompetitorId, playerCI2.CompetitorId);
            Assert.AreEqual(playerCI.CountryCode, playerCI2.CountryCode);
            Assert.AreEqual(playerCI.DateOfBirth, playerCI2.DateOfBirth);
            Assert.AreEqual(playerCI.FullName, playerCI2.FullName);
            Assert.AreEqual(playerCI.Gender, playerCI2.Gender);
            Assert.AreEqual(playerCI.Names.Count, playerCI2.Names.Count);
            foreach (var culture in playerCI.Names.Keys)
            {
                Assert.AreEqual(playerCI.GetName(culture), playerCI2.GetName(culture));
                Assert.AreEqual(playerCI.GetNationality(culture), playerCI2.GetNationality(culture));
            }
            Assert.AreEqual(playerCI.Height, playerCI2.Height);
            Assert.AreEqual(playerCI.Nickname, playerCI2.Nickname);
            Assert.AreEqual(playerCI.Weight, playerCI2.Weight);
            Assert.AreEqual(playerCI.Type, playerCI2.Type);
        }

        private void Compare(PlayerProfileCI playerCI, playerExtended player, CultureInfo culture)
        {
            Assert.IsNotNull(playerCI);
            Assert.IsNotNull(player);

            Assert.AreEqual(player.id, playerCI.Id.ToString());
            Assert.AreEqual(_competitorId, playerCI.CompetitorId);
            if (player.type == null)
            {
                Assert.IsNull(playerCI.Type);
            }
            else
            {
                Assert.AreEqual(player.type, playerCI.Type);
            }
            if (player.country_code == null)
            {
                Assert.IsNull(playerCI.CountryCode);
            }
            else
            {
                Assert.AreEqual(player.country_code, playerCI.CountryCode);
            }
            if (player.date_of_birth == null)
            {
                Assert.IsNull(playerCI.DateOfBirth);
            }
            else
            {
                Assert.AreEqual(player.date_of_birth, playerCI.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            }
            if (player.full_name == null)
            {
                Assert.IsNull(playerCI.FullName);
            }
            else
            {
                Assert.AreEqual(player.full_name, playerCI.FullName);
            }
            if (player.gender == null)
            {
                Assert.IsNull(playerCI.Gender);
            }
            else
            {
                Assert.AreEqual(player.gender, playerCI.Gender);
            }
            if (!player.heightSpecified)
            {
                Assert.IsNull(playerCI.Height);
            }
            else
            {
                Assert.IsTrue(player.height > 0);
                Assert.AreEqual(player.height, playerCI.Height);
            }
            if (!player.weightSpecified)
            {
                Assert.IsNull(playerCI.Weight);
            }
            else
            {
                Assert.IsTrue(player.weight > 0);
                Assert.AreEqual(player.weight, playerCI.Weight);
            }
            if (player.type == null)
            {
                Assert.IsNull(playerCI.Type);
            }
            else
            {
                Assert.AreEqual(player.type, playerCI.Type);
            }
            //if (!player.jersey_numberSpecified)
            //{
            //    Assert.IsNull(playerCI.);
            //}
            //else
            //{
            //    Assert.IsTrue(player.height > 0);
            //    Assert.AreEqual(player.height, playerCI.Height);
            //}
            if (player.name == null)
            {
                Assert.IsNull(playerCI.GetName(culture));
            }
            else
            {
                Assert.AreEqual(player.name, playerCI.GetName(culture));
            }
            if (player.nationality == null)
            {
                Assert.IsNull(playerCI.GetNationality(culture));
            }
            else
            {
                Assert.AreEqual(player.nationality, playerCI.GetNationality(culture));
            }
            if (player.nickname == null)
            {
                Assert.IsNull(playerCI.Nickname);
            }
            else
            {
                Assert.AreEqual(player.nickname, playerCI.Nickname);
            }
            if (player.name == null)
            {
                Assert.IsNull(playerCI.Abbreviation);
            }
            else
            {
                Assert.AreEqual(SdkInfo.GetAbbreviationFromName(player.name), playerCI.Abbreviation);
            }
        }
    }
}
