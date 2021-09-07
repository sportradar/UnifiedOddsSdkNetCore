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
using Sportradar.OddsFeed.SDK.Common;
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
        private const string TourInfoXml = @"Summarys\summary_sr_tournament_40.en.xml";
        private const string SeasonInfoXml = @"Summarys\summary_sr_season_80242.en.xml";
        private static tournamentInfoEndpoint _tourApiData;
        private static TournamentInfoDTO _tourDtoData;
        private static TournamentInfoCI _tourCiData;
        private static tournamentInfoEndpoint _seasonApiData;
        private static TournamentInfoDTO _seasonDtoData;
        private static TournamentInfoCI _seasonCiData;
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
            _sportEventCache = new SportEventCache(_memoryCache,
                                                _dataRouterManager,
                                                new SportEventCacheItemFactory(_dataRouterManager, new SemaphorePool(5, ExceptionHandlingStrategy.THROW), TestData.Cultures.First(), new MemoryCache("FixtureTimestampCache")),
                                                _timer,
                                                TestData.Cultures3,
                                                _cacheManager);

            var deserializer = new Deserializer<tournamentInfoEndpoint>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentInfoMapperFactory();

            var tourDataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>(TestData.RestXmlPath + TourInfoXml, dataFetcher, deserializer, mapperFactory);
            _tourApiData = deserializer.Deserialize(dataFetcher.GetData(new Uri(string.Format(TestData.RestXmlPath + TourInfoXml))));
            _tourDtoData = tourDataProvider.GetDataAsync("", "en").Result;
            _tourCiData = (TournamentInfoCI) _sportEventCache.GetEventCacheItem(URN.Parse("sr:tournament:40"));

            var seasonDataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>(TestData.RestXmlPath + SeasonInfoXml, dataFetcher, deserializer, mapperFactory);
            _seasonApiData = deserializer.Deserialize(dataFetcher.GetData(new Uri(string.Format(TestData.RestXmlPath + SeasonInfoXml))));
            _seasonDtoData = seasonDataProvider.GetDataAsync("", "en").Result;
            _seasonCiData = (TournamentInfoCI) _sportEventCache.GetEventCacheItem(URN.Parse("sr:season:80242"));
        }

        [TestMethod]
        public void BaseEntitiesSetupCorrectly()
        {
            Assert.IsNotNull(_tourApiData);
            Assert.IsNotNull(_tourDtoData);
            Assert.IsNotNull(_tourCiData);
            Assert.IsNotNull(_seasonApiData);
            Assert.IsNotNull(_seasonDtoData);
            Assert.IsNotNull(_seasonCiData);

            Assert.IsNotNull(_tourCiData.GetGroupsAsync(TestData.Cultures3).Result);
            Assert.IsNotNull(_seasonCiData.GetGroupsAsync(TestData.Cultures3).Result);

            Assert.AreEqual(TestData.Cultures3.Count, _tourCiData.LoadedSummaries.Count);
            Assert.AreEqual(TestData.Cultures3.Count, _seasonCiData.LoadedSummaries.Count);

            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);

            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);

            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupBase()
        {
            Assert.IsNotNull(_tourCiData);
            Assert.IsNotNull(_tourApiData);
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);

            Assert.AreEqual(URN.Parse(_tourApiData.tournament.id), _tourCiData.Id);
            Assert.IsNotNull(_tourApiData.groups);
            var groups = _tourCiData.GetGroupsAsync(_cultures).Result.ToList();
            Assert.IsNotNull(groups);

            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);

            Assert.AreEqual(1, _tourApiData.groups.Length);
            Assert.AreEqual(_tourApiData.groups.Length, groups.Count);

            var groupCompetitorIds = groups[0].CompetitorsIds.ToList();
            var sapiTeams = _tourApiData.groups[0].competitor.ToList();
            Assert.AreEqual(sapiTeams.Count, groups[0].CompetitorsIds.Count());

            foreach (var sapiTeam in sapiTeams)
            {
                Assert.IsTrue(groupCompetitorIds.Contains(URN.Parse(sapiTeam.id)));
            }

            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupRemoveById()
        {
            // set group id
            _tourApiData.groups[0].id = "1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // change group id
            _tourApiData.groups[0].id = "2";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupByIdName()
        {
            // set group name
            _tourApiData.groups[0].id = "1";
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // change group name
            _tourApiData.groups[0].id = "2";
            _tourApiData.groups[0].name = "Name2";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupRemoveByName()
        {
            // set group name
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // change group name
            _tourApiData.groups[0].name = "Name2";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupRemoveName()
        {
            // set group name
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // change group name
            _tourApiData.groups[0].name = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void MergeTournamentGroupSplit()
        {
            // default
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // set group name
            _tourApiData.groups[0].id = "1";
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // split group
            var oldGroup = _tourApiData.groups[0];
            var newGroup = new tournamentGroup();
            newGroup.id = "2";
            newGroup.name = "Name2";
            var oldGroupCompetitors = _tourApiData.groups[0].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            var i = _tourApiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_tourApiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }

            newGroup.competitor = newGroupCompetitors.ToArray();
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _tourApiData.groups = new[] {oldGroup, newGroup};
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void SeasonGroupsMergedCorrectly()
        {
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void SeasonGroupSplitGroupCompetitors()
        {
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);

            var oldGroup = _seasonApiData.groups[0];
            var oldGroupCompetitors = _seasonApiData.groups[0].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            var i = _seasonApiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_seasonApiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }
            
            var newGroup = new tournamentGroup {id = "2", name = "Name2", competitor = newGroupCompetitors.ToArray()};
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] {oldGroup, newGroup};
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void SeasonGroupRemoveGroupCompetitor()
        {
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);

            var oldGroup = _seasonApiData.groups[0];
            var oldGroupCompetitors = _seasonApiData.groups[0].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            var i = _seasonApiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_seasonApiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }

            oldGroupCompetitors.RemoveAt(0);
            
            var newGroup = new tournamentGroup {id = "2", name = "Name2", competitor = newGroupCompetitors.ToArray()};
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] {oldGroup, newGroup};
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void SeasonGroupAddGroupCompetitor()
        {
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);

            var oldGroup = _seasonApiData.groups[0];
            var oldGroupCompetitors = _seasonApiData.groups[0].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            var i = _seasonApiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_seasonApiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }

            var compId = URN.Parse(oldGroupCompetitors[0].id);
            oldGroupCompetitors.Add(new team{ id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 2}", name = "John Doe"});
            
            var newGroup = new tournamentGroup {id = "2", name = "Name2", competitor = newGroupCompetitors.ToArray()};
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] {oldGroup, newGroup};
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void SeasonGroupReplaceGroupCompetitor()
        {
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);

            var oldGroup = _seasonApiData.groups[0];
            var oldGroupCompetitors = _seasonApiData.groups[0].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            var i = _seasonApiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_seasonApiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }

            // replace competitor in group
            var compId = URN.Parse(oldGroupCompetitors[0].id);
            oldGroupCompetitors[0].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 2}";
            var newGroup = new tournamentGroup {competitor = newGroupCompetitors.ToArray()};
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] {oldGroup, newGroup};
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void SeasonGroupMultipleChanges()
        {
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);

            var oldGroup = _seasonApiData.groups[0];
            var oldGroupCompetitors = _seasonApiData.groups[0].competitor.ToList();
            var oldGroupCompetitors2 = _seasonApiData.groups[1].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            var i = _seasonApiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_seasonApiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }
            
            var compId = URN.Parse(oldGroupCompetitors[0].id);
            oldGroupCompetitors[0].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 2}";
            
            oldGroupCompetitors2[0].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 3}";
            oldGroupCompetitors2[1].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 4}";
            oldGroupCompetitors2[2].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 5}";

            var newGroup = new tournamentGroup {competitor = newGroupCompetitors.ToArray()};
            var newGroup2 = new tournamentGroup {id = "2", name = "Group2", competitor = oldGroupCompetitors2.ToArray()};
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] {oldGroup, newGroup, newGroup2};
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).Result);
        }

        [TestMethod]
        public void SeasonGroupMultipleCultures()
        {
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(TestData.Cultures3).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(TestData.Cultures3).Result);
            
            var oldGroup = _seasonApiData.groups[0];
            var oldGroupCompetitors = _seasonApiData.groups[0].competitor.ToList();
            var oldGroupCompetitors2 = _seasonApiData.groups[1].competitor.ToList();
            var newGroupCompetitors = new List<team>();
            var i = _seasonApiData.groups[0].competitor.Length / 2;
            while (i > 0)
            {
                newGroupCompetitors.Add(_seasonApiData.groups[0].competitor[i]);
                oldGroupCompetitors.RemoveAt(i);
                i--;
            }
            
            var compId = URN.Parse(oldGroupCompetitors[0].id);
            oldGroupCompetitors[0].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 2}";
            
            oldGroupCompetitors2[0].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 3}";
            oldGroupCompetitors2[1].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 4}";
            oldGroupCompetitors2[2].id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 5}";

            var newGroup = new tournamentGroup {competitor = newGroupCompetitors.ToArray()};
            var newGroup2 = new tournamentGroup {id = "2", name = "Group2", competitor = oldGroupCompetitors2.ToArray()};
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] {oldGroup, newGroup, newGroup2};
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Cultures3.ElementAt(0), false);
            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(TestData.Cultures3).Result);
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(TestData.Cultures3).Result);
        }

        private void VerifyTournamentGroups(IEnumerable<tournamentGroup> apiGroups, IEnumerable<GroupCI> ciTourGroups)
        {
            var sapiGroups = apiGroups?.ToList();
            var ciGroups = ciTourGroups?.ToList();

            //if api groups are empty, so must be ci groups
            if (sapiGroups.IsNullOrEmpty())
            {
                Assert.IsTrue(ciGroups.IsNullOrEmpty());
                return;
            }

            Assert.IsNotNull(sapiGroups);
            Assert.IsNotNull(ciGroups);
            Assert.AreEqual(sapiGroups.Count, ciGroups.Count);
            foreach (var sapiGroup in sapiGroups)
            {
                // find by id
                if (!string.IsNullOrEmpty(sapiGroup.id))
                {
                    Assert.IsTrue(ciGroups.Exists(a => a.Id.Equals(sapiGroup.id)));
                }

                //find by name
                if (!string.IsNullOrEmpty(sapiGroup.name))
                {
                    Assert.IsTrue(ciGroups.Exists(a => a.Name.Equals(sapiGroup.name)));
                }

                var matchingGroup = MergerHelper.FindExistingGroup(ciGroups, new GroupDTO(sapiGroup));

                Assert.AreEqual(sapiGroup.competitor.Length, matchingGroup.CompetitorsIds.Count());

                // each competitor in api group must also be in matching group
                foreach (var sapiTeam in sapiGroup.competitor)
                {
                    Assert.IsTrue(matchingGroup.CompetitorsIds.Contains(URN.Parse(sapiTeam.id)));
                }
            }
        }

        private void VerifyTournamentCompetitors(IEnumerable<team> apiCompetitors, IEnumerable<tournamentGroup> apiGroups, IEnumerable<URN> ciCompetitorsIds)
        {
            var sapiComps = apiCompetitors?.ToList();
            var sapiGroups = apiGroups?.ToList();
            var ciComps = ciCompetitorsIds?.ToList();

            // no competitors defined on api
            if (sapiComps.IsNullOrEmpty())
            {
                // no groups defined
                if(sapiGroups.IsNullOrEmpty())
                {
                    Assert.IsNull(ciComps);
                }
                else
                {
                    // groups defined
                    Assert.IsNotNull(sapiGroups);
                    Assert.IsNotNull(ciComps);
                    Assert.IsTrue(ciComps.Any());
                    // all competitors in all groups are listed in ci competitors (on root level)
                    foreach (var sapiGroup in sapiGroups)
                    {
                        foreach (var team in sapiGroup.competitor)
                        {
                            Assert.IsTrue(ciComps.Any(a=>a.Equals(URN.Parse(team.id))));
                        }
                    }

                    // all ci competitors are at least in one group
                    foreach (var ciCompetitorId in ciComps)
                    {
                        Assert.IsTrue(sapiGroups.SelectMany(s=>s.competitor).Any(team => ciCompetitorId.ToString().Equals(team.id)));
                    }
                    
                    // all ci competitors are listed only once
                    foreach (var ciCompetitorId in ciComps)
                    {
                        Assert.AreEqual(1, ciComps.Count(cId=>cId.Equals(ciCompetitorId)));
                    }
                }
            }
            else
            {
                Assert.IsNotNull(sapiComps);
                Assert.IsNotNull(ciComps);
                // all ci competitors are the same as api competitors
                Assert.AreEqual(sapiComps.Count, ciComps.Count);

                // all ci competitors are in sapi competitors
                foreach (var ciCompetitorId in ciComps)
                {
                    Assert.IsTrue(sapiComps.Any(team=> ciCompetitorId.ToString().Equals(team.id)));
                }
                    
                // all ci competitors are listed only once
                foreach (var ciCompetitorId in ciComps)
                {
                    Assert.AreEqual(1, ciComps.Count(cId=>cId.Equals(ciCompetitorId)));
                }
            }
        }
    }
}
