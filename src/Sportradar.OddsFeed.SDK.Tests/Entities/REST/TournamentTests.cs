/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
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
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class TournamentTests
    {
        private const string TourInfoXml = @"Summarys/summary_sr_tournament_40_en.xml";
        private const string SeasonInfoXml = @"Summarys/summary_sr_season_80242_en.xml";
        private tournamentInfoEndpoint _tourApiData;
        private TournamentInfoDTO _tourDtoData;
        private TournamentInfoCI _tourCiData;
        private tournamentInfoEndpoint _seasonApiData;
        private TournamentInfoDTO _seasonDtoData;
        private TournamentInfoCI _seasonCiData;

        private readonly List<CultureInfo> _cultures = new List<CultureInfo> { TestData.Culture };

        public TournamentTests(ITestOutputHelper outputHelper)
        {
            var memoryCache = new MemoryCache("sportEventCache");

            var cacheManager = new CacheManager();
            var dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

            var timer = new TestTimer(false);
            var sportEventCache = new SportEventCache(memoryCache,
                dataRouterManager,
                new SportEventCacheItemFactory(dataRouterManager, new SemaphorePool(5, ExceptionHandlingStrategy.THROW), TestData.Cultures.First(), new MemoryCache("FixtureTimestampCache")),
                timer,
                TestData.Cultures3,
                cacheManager);

            var deserializer = new Deserializer<tournamentInfoEndpoint>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentInfoMapperFactory();

            var tourDataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>(TestData.RestXmlPath + TourInfoXml, dataFetcher, deserializer, mapperFactory);
            _tourApiData = deserializer.Deserialize(dataFetcher.GetData(new Uri(string.Format(TestData.RestXmlPath + TourInfoXml))));
            _tourDtoData = tourDataProvider.GetDataAsync("", "en").GetAwaiter().GetResult();
            _tourCiData = (TournamentInfoCI)sportEventCache.GetEventCacheItem(URN.Parse("sr:tournament:40"));

            var seasonDataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDTO>(TestData.RestXmlPath + SeasonInfoXml, dataFetcher, deserializer, mapperFactory);
            _seasonApiData = deserializer.Deserialize(dataFetcher.GetData(new Uri(string.Format(TestData.RestXmlPath + SeasonInfoXml))));
            _seasonDtoData = seasonDataProvider.GetDataAsync("", "en").GetAwaiter().GetResult();
            _seasonCiData = (TournamentInfoCI)sportEventCache.GetEventCacheItem(URN.Parse("sr:season:80242"));
        }

        [Fact]
        public async Task BaseEntitiesSetupCorrectly()
        {
            Assert.NotNull(_tourApiData);
            Assert.NotNull(_tourDtoData);
            Assert.NotNull(_tourCiData);
            Assert.NotNull(_seasonApiData);
            Assert.NotNull(_seasonDtoData);
            Assert.NotNull(_seasonCiData);

            var tourGroups = await _tourCiData.GetGroupsAsync(TestData.Cultures3);
            Assert.NotNull(tourGroups);

            var seasonGroups = await _seasonCiData.GetGroupsAsync(TestData.Cultures3);
            Assert.NotNull(seasonGroups);

            Assert.Equal(TestData.Cultures3.Count, _tourCiData.LoadedSummaries.Count);
            Assert.Equal(TestData.Cultures3.Count, _seasonCiData.LoadedSummaries.Count);

            VerifyTournamentGroups(_tourApiData.groups, _tourCiData.GetGroupsAsync(_cultures).GetAwaiter().GetResult());

            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, _tourCiData.GetCompetitorsIdsAsync(_cultures).GetAwaiter().GetResult());

            VerifyTournamentGroups(_seasonApiData.groups, _seasonCiData.GetGroupsAsync(_cultures).GetAwaiter().GetResult());

            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, _seasonCiData.GetCompetitorsIdsAsync(_cultures).GetAwaiter().GetResult());
        }

        [Fact]
        public async Task MergeTournamentGroupBase()
        {
            Assert.NotNull(_tourCiData);
            Assert.NotNull(_tourApiData);
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);

            Assert.Equal(URN.Parse(_tourApiData.tournament.id), _tourCiData.Id);
            Assert.NotNull(_tourApiData.groups);
            var groups = (await _tourCiData.GetGroupsAsync(_cultures)).ToList();
            Assert.NotNull(groups);

            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));

            Assert.Single(_tourApiData.groups);
            Assert.Equal(_tourApiData.groups.Length, groups.Count);

            var groupCompetitorIds = groups[0].CompetitorsIds.ToList();
            var sapiTeams = _tourApiData.groups[0].competitor.ToList();
            Assert.Equal(sapiTeams.Count, groups[0].CompetitorsIds.Count());

            foreach (var sapiTeam in sapiTeams)
            {
                Assert.Contains(URN.Parse(sapiTeam.id), groupCompetitorIds);
            }

            var tourCiDataCompetitors = await _tourCiData.GetCompetitorsIdsAsync(_cultures).ConfigureAwait(false);
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, tourCiDataCompetitors);
        }

        [Fact]
        public async Task MergeTournamentGroupRemoveById()
        {
            // set group id
            _tourApiData.groups[0].id = "1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // change group id
            _tourApiData.groups[0].id = "2";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task MergeTournamentGroupByIdName()
        {
            // set group name
            _tourApiData.groups[0].id = "1";
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // change group name
            _tourApiData.groups[0].id = "2";
            _tourApiData.groups[0].name = "Name2";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task MergeTournamentGroupRemoveByName()
        {
            // set group name
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // change group name
            _tourApiData.groups[0].name = "Name2";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task MergeTournamentGroupRemoveName()
        {
            // set group name
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            _tourCiData.LoadedSummaries.Add(TestData.Culture);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // change group name
            _tourApiData.groups[0].name = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task MergeTournamentGroupSplit()
        {
            // default
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // set group name
            _tourApiData.groups[0].id = "1";
            _tourApiData.groups[0].name = "Name1";
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

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
            _tourApiData.groups = new[] { oldGroup, newGroup };
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));

            // remove group
            _tourApiData.groups = null;
            _tourDtoData = new TournamentInfoDTO(_tourApiData);
            _tourCiData.Merge(_tourDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_tourApiData.groups, await _tourCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_tourApiData.competitors, _tourApiData.groups, await _tourCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task SeasonGroupsMergedCorrectly()
        {
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task SeasonGroupSplitGroupCompetitors()
        {
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));

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

            var newGroup = new tournamentGroup { id = "2", name = "Name2", competitor = newGroupCompetitors.ToArray() };
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] { oldGroup, newGroup };
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task SeasonGroupRemoveGroupCompetitor()
        {
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));

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

            var newGroup = new tournamentGroup { id = "2", name = "Name2", competitor = newGroupCompetitors.ToArray() };
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] { oldGroup, newGroup };
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task SeasonGroupAddGroupCompetitor()
        {
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));

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
            oldGroupCompetitors.Add(new team { id = $"{compId.Prefix}:{compId.Type}:{compId.Id / 2}", name = "John Doe" });

            var newGroup = new tournamentGroup { id = "2", name = "Name2", competitor = newGroupCompetitors.ToArray() };
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] { oldGroup, newGroup };
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task SeasonGroupReplaceGroupCompetitor()
        {
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));

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
            var newGroup = new tournamentGroup { competitor = newGroupCompetitors.ToArray() };
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] { oldGroup, newGroup };
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task SeasonGroupMultipleChanges()
        {
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));

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

            var newGroup = new tournamentGroup { competitor = newGroupCompetitors.ToArray() };
            var newGroup2 = new tournamentGroup { id = "2", name = "Group2", competitor = oldGroupCompetitors2.ToArray() };
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] { oldGroup, newGroup, newGroup2 };
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Culture, false);
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(_cultures));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(_cultures));
        }

        [Fact]
        public async Task SeasonGroupMultipleCultures()
        {
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(TestData.Cultures3));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(TestData.Cultures3));

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

            var newGroup = new tournamentGroup { competitor = newGroupCompetitors.ToArray() };
            var newGroup2 = new tournamentGroup { id = "2", name = "Group2", competitor = oldGroupCompetitors2.ToArray() };
            oldGroup.competitor = oldGroupCompetitors.ToArray();
            _seasonApiData.groups = new[] { oldGroup, newGroup, newGroup2 };
            _seasonDtoData = new TournamentInfoDTO(_seasonApiData);
            _seasonCiData.Merge(_seasonDtoData, TestData.Cultures3.ElementAt(0), false);
            VerifyTournamentGroups(_seasonApiData.groups, await _seasonCiData.GetGroupsAsync(TestData.Cultures3));
            VerifyTournamentCompetitors(_seasonApiData.competitors, _seasonApiData.groups, await _seasonCiData.GetCompetitorsIdsAsync(TestData.Cultures3));
        }

        private static void VerifyTournamentGroups(IEnumerable<tournamentGroup> apiGroups, IEnumerable<GroupCI> ciTourGroups)
        {
            var sapiGroups = apiGroups?.ToList();
            var ciGroups = ciTourGroups?.ToList();

            //if api groups are empty, so must be ci groups
            if (sapiGroups.IsNullOrEmpty())
            {
                Assert.True(ciGroups.IsNullOrEmpty());
                return;
            }

            Assert.NotNull(sapiGroups);
            Assert.NotNull(ciGroups);
            Assert.Equal(sapiGroups.Count, ciGroups.Count);
            foreach (var sapiGroup in sapiGroups)
            {
                // find by id
                if (!string.IsNullOrEmpty(sapiGroup.id))
                {
                    Assert.True(ciGroups.Exists(a => a.Id.Equals(sapiGroup.id)));
                }

                //find by name
                if (!string.IsNullOrEmpty(sapiGroup.name))
                {
                    Assert.True(ciGroups.Exists(a => a.Name.Equals(sapiGroup.name)));
                }

                var matchingGroup = MergerHelper.FindExistingGroup(ciGroups, new GroupDTO(sapiGroup));

                Assert.Equal(sapiGroup.competitor.Length, matchingGroup.CompetitorsIds.Count());

                // each competitor in api group must also be in matching group
                foreach (var sapiTeam in sapiGroup.competitor)
                {
                    Assert.Contains(URN.Parse(sapiTeam.id), matchingGroup.CompetitorsIds);
                }
            }
        }

        private static void VerifyTournamentCompetitors(IEnumerable<team> apiCompetitors, IEnumerable<tournamentGroup> apiGroups, IEnumerable<URN> ciCompetitorsIds)
        {
            var sapiComps = apiCompetitors?.ToList();
            var sapiGroups = apiGroups?.ToList();
            var ciComps = ciCompetitorsIds?.ToList();

            // no competitors defined on api
            if (sapiComps.IsNullOrEmpty())
            {
                // no groups defined
                if (sapiGroups.IsNullOrEmpty())
                {
                    Assert.Null(ciComps);
                }
                else
                {
                    // groups defined
                    Assert.NotNull(sapiGroups);
                    Assert.NotNull(ciComps);
                    Assert.True(ciComps.Any());
                    // all competitors in all groups are listed in ci competitors (on root level)
                    foreach (var sapiGroup in sapiGroups)
                    {
                        foreach (var team in sapiGroup.competitor)
                        {
                            Assert.Contains(ciComps, a => a.Equals(URN.Parse(team.id)));
                        }
                    }

                    // all ci competitors are at least in one group
                    foreach (var ciCompetitorId in ciComps)
                    {
                        Assert.Contains(sapiGroups.SelectMany(s => s.competitor), team => ciCompetitorId.ToString().Equals(team.id));
                    }

                    // all ci competitors are listed only once
                    foreach (var ciCompetitorId in ciComps)
                    {
                        Assert.Equal(1, ciComps.Count(cId => cId.Equals(ciCompetitorId)));
                    }
                }
            }
            else
            {
                Assert.NotNull(sapiComps);
                Assert.NotNull(ciComps);
                // all ci competitors are the same as api competitors
                Assert.Equal(sapiComps.Count, ciComps.Count);

                // all ci competitors are in sapi competitors
                foreach (var ciCompetitorId in ciComps)
                {
                    Assert.Contains(sapiComps, team => ciCompetitorId.ToString().Equals(team.id));
                }

                // all ci competitors are listed only once
                foreach (var ciCompetitorId in ciComps)
                {
                    Assert.Equal(1, ciComps.Count(cId => cId.Equals(ciCompetitorId)));
                }
            }
        }
    }
}
