/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Castle.Core.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class TournamentTest
    {
        private const string TourInfoXml = "tournament_info.xml";
        private static tournamentInfoEndpoint _apiData;
        private static TournamentInfoDTO _dtoData;
        private static TournamentInfoCI _ciData;
        private SportEventCache _sportEventCache;
        private MemoryCache _memoryCache;
        private TestTimer _timer;

        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;
        private readonly List<CultureInfo> _cultures = new List<CultureInfo> {TestData.Culture};

        [TestInitialize]
        public void Init()
        {
            _memoryCache = new MemoryCache("sportEventCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _timer = new TestTimer(false);
            _sportEventCache = new SportEventCache(_memoryCache, _dataRouterManager,
                new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5), TestData.Cultures.First(),
                    new MemoryCache("FixtureTimestampCache")), _timer, TestData.Cultures, _cacheManager);

            var deserializer = new Deserializer<tournamentInfoEndpoint>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentInfoMapperFactory();

            var dataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>(
                TestData.RestXmlPath + TourInfoXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _apiData = deserializer.Deserialize(
                dataFetcher.GetData(new Uri(string.Format(TestData.RestXmlPath + TourInfoXml))));
            _dtoData = dataProvider.GetDataAsync("", "en").Result;
            _ciData = (TournamentInfoCI) _sportEventCache.GetEventCacheItem(URN.Parse("sr:tournament:40"));
        }

        [TestMethod]
        public void TestsAreInSportEntityFactoryTest()
        {
            Assert.IsNotNull(_dtoData);
        }

        [TestMethod]
        public void MergeTournamentGroupBase()
        {
            Assert.IsNotNull(_ciData);
            Assert.IsNotNull(_apiData);
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);

            Assert.AreEqual(URN.Parse(_apiData.tournament.id), _ciData.Id);
            Assert.IsNotNull(_apiData.groups);
            List<GroupCI> groups = _ciData.GetGroupsAsync(_cultures).Result.ToList();
            Assert.IsNotNull(groups);

            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            Assert.AreEqual(1, _apiData.groups.Length);
            Assert.AreEqual(_apiData.groups.Length, groups.Count);

            List<CompetitorCI> groupCompetitorIds = groups[0].Competitors.ToList();
            List<team> sapiTeams = _apiData.groups[0].competitor.ToList();
            Assert.AreEqual(sapiTeams.Count, groups[0].Competitors.Count());

            foreach (var sapiTeam in sapiTeams)
            {
                Assert.IsTrue(groupCompetitorIds.Select(s => s.Id).Contains(URN.Parse(sapiTeam.id)));
            }
        }

        [TestMethod]
        public void MergeTournamentGroupRemoveById()
        {
            // set group id
            _apiData.groups[0].id = "1";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // change group id
            _apiData.groups[0].id = "2";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // remove group
            _apiData.groups = null;
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupByIdName()
        {
            // set group name
            _apiData.groups[0].id = "1";
            _apiData.groups[0].name = "Name1";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // change group name
            _apiData.groups[0].id = "2";
            _apiData.groups[0].name = "Name2";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // remove group
            _apiData.groups = null;
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupRemoveByName()
        {
            // set group name
            _apiData.groups[0].name = "Name1";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // change group name
            _apiData.groups[0].name = "Name2";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // remove group
            _apiData.groups = null;
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupRemoveName()
        {
            // set group name
            _apiData.groups[0].name = "Name1";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // change group name
            _apiData.groups[0].name = null;
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // remove group
            _apiData.groups = null;
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupSplit()
        {
            // default
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // set group name
            _apiData.groups[0].id = "1";
            _apiData.groups[0].name = "Name1";
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // split group
            var oldGroup = _apiData.groups[0];
            var newGroup = new tournamentGroup();
            newGroup.id = "2";
            newGroup.name = "Name2";
            //newGroup.competitor;
            var oldGroupCompetitors = _apiData.groups[0].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            int i = _apiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_apiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }

            newGroup.competitor = newGroupCompetitors.ToArray();
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _apiData.groups = new[] {oldGroup, newGroup};
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);

            // remove group
            _apiData.groups = null;
            _dtoData = new TournamentInfoDTO(_apiData);
            _ciData.Merge(_dtoData, TestData.Culture, false);
            VerifyTournamentGroups(_apiData.groups, _ciData.GetGroupsAsync(_cultures).Result);
        }

        private void VerifyTournamentGroups(IEnumerable<tournamentGroup> apiGroups, IEnumerable<GroupCI> ciTourGroups)
        {
            var sapiGroups = apiGroups?.ToList();
            var ciGroups = ciTourGroups?.ToList();
            if (sapiGroups.IsNullOrEmpty())
            {
                Assert.IsTrue(ciGroups.IsNullOrEmpty());
                return;
            }

            Assert.AreEqual(sapiGroups.Count, ciGroups.Count);
            foreach (var sapiGroup in sapiGroups)
            {
                if (!string.IsNullOrEmpty(sapiGroup.id))
                {
                    Assert.IsTrue(ciGroups.Exists(a => a.Id.Equals(sapiGroup.id)));
                }

                if (!string.IsNullOrEmpty(sapiGroup.name))
                {
                    Assert.IsTrue(ciGroups.Exists(a => a.Name.Equals(sapiGroup.name)));
                }

                GroupCI matchingGroup = ciGroups.Where(f =>
                    !string.IsNullOrEmpty(sapiGroup.id) && f.Id.Equals(sapiGroup.id)
                    || !string.IsNullOrEmpty(sapiGroup.name) && f.Name.Equals(sapiGroup.name)
                    || string.IsNullOrEmpty(sapiGroup.id) && string.IsNullOrEmpty(sapiGroup.name) &&
                    string.IsNullOrEmpty(f.Id) && string.IsNullOrEmpty(f.Name)).FirstOrDefault();

                Assert.AreEqual(sapiGroup.competitor.Length, matchingGroup.Competitors.Count());

                foreach (var sapiTeam in sapiGroup.competitor)
                {
                    Assert.IsTrue(matchingGroup.Competitors.Select(s => s.Id).Contains(URN.Parse(sapiTeam.id)));
                }
            }
        }
    }
}
