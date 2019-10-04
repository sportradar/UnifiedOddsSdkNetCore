/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class ProfileCacheTest
    {
        private ICacheManager _cacheManager;
        private MemoryCache _memoryCache;
        private IProfileCache _profileCache;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _memoryCache = new MemoryCache("test");
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);
            _profileCache = new ProfileCache(_memoryCache, _dataRouterManager, _cacheManager);
        }

        private static URN CreatePlayerUrn(int playerId)
        {
            return new URN("sr", "player", playerId);
        }

        private static URN CreateCompetitorUrn(int competitorId)
        {
            return new URN("sr", "competitor", competitorId);
        }

        private static URN CreateSimpleTeamUrn(int competitorId)
        {
            return new URN("sr", SdkInfo.SimpleTeamIdentifier, competitorId);
        }

        [TestMethod]
        public void player_profile_gets_cached()
        {
            const string callType = "GetPlayerProfileAsync";
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), "Should be called exactly 0 times.");
            Assert.IsTrue(!_memoryCache.Any());
            var player = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(player);
            Assert.AreEqual(1, _memoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");

            //if we call again, should not fetch again
            player = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(player);
            Assert.AreEqual(1, _memoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public void number_of_player_provider_calls_match_is_correct()
        {
            const string callType = "GetPlayerProfileAsync";
            var player1 = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(player1);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");

            var player2 = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(2), TestData.Cultures).Result;
            Assert.IsNotNull(player2);

            Assert.AreNotEqual(player1.Id, player2.Id);
            Assert.AreEqual(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count * 2} times.");
        }

        [TestMethod]
        public void competitor_profile_gets_cached()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());

            var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures).Result;

            Assert.IsNotNull(competitor);
            Assert.AreEqual(35, _memoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");

            //if we call again, should not fetch again
            competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(competitor);
            Assert.AreEqual(35, _memoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public void number_of_competitor_provider_calls_match_is_correct()
        {
            const string callType = "GetCompetitorAsync";
            var competitor1 = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(competitor1);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");

            var competitor2 = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(2), TestData.Cultures).Result;
            Assert.IsNotNull(competitor2);

            Assert.AreNotEqual(competitor1.Id, competitor2.Id);
            Assert.AreEqual(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count * 2} times.");
        }

        [TestMethod]
        public void simpleteam_profile_gets_cached()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());

            var competitor = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;

            Assert.IsNotNull(competitor);
            Assert.AreEqual(1, _memoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");

            //if we call again, should not fetch again
            competitor = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(competitor);
            Assert.AreEqual(1, _memoryCache.Count());

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        public void number_of_simpleteam_provider_calls_match_is_correct()
        {
            const string callType = "GetCompetitorAsync";
            var competitor1 = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(competitor1);

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");

            var competitor2 = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(2), TestData.Cultures).Result;
            Assert.IsNotNull(competitor2);

            Assert.AreNotEqual(competitor1.Id, competitor2.Id);
            Assert.AreEqual(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count * 2} times.");
        }

        [TestMethod]
        public void simpleteam_is_cached_without_betradarId()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            var competitorCI = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;
            Assert.IsNotNull(competitorCI);
            Assert.IsNotNull(competitorCI.ReferenceId);
            Assert.IsNotNull(competitorCI.ReferenceId.ReferenceIds);
            Assert.IsTrue(competitorCI.ReferenceId.ReferenceIds.Any());
            Assert.AreEqual(1, competitorCI.ReferenceId.ReferenceIds.Count);
            Assert.AreEqual(1, competitorCI.ReferenceId.BetradarId);
        }

        [TestMethod]
        public void simpleteam_is_cached_without_referenceIds()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            var simpleTeamDto = CacheSimpleTeam(1, null);

            var competitorCI = _profileCache.GetCompetitorProfileAsync(simpleTeamDto.Competitor.Id, TestData.Cultures).Result;
            Assert.IsNotNull(competitorCI);
            Assert.AreEqual(simpleTeamDto.Competitor.Id, competitorCI.Id);
            Assert.IsNotNull(competitorCI.ReferenceId);
            Assert.IsNotNull(competitorCI.ReferenceId.ReferenceIds);
            Assert.IsTrue(competitorCI.ReferenceId.ReferenceIds.Any());
            Assert.AreEqual(1, competitorCI.ReferenceId.ReferenceIds.Count);
            Assert.AreEqual(simpleTeamDto.Competitor.Id.Id, competitorCI.ReferenceId.BetradarId);
        }

        [TestMethod]
        public void simpleteam_is_cached_with_betradarId()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            var competitorCI = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(2), TestData.Cultures).Result;
            Assert.IsNotNull(competitorCI);
            Assert.IsNotNull(competitorCI.ReferenceId);
            Assert.IsNotNull(competitorCI.ReferenceId.ReferenceIds);
            Assert.IsTrue(competitorCI.ReferenceId.ReferenceIds.Any());
            Assert.AreEqual(2, competitorCI.ReferenceId.ReferenceIds.Count);
            Assert.AreEqual("555", competitorCI.ReferenceId.BetradarId.ToString());
        }

        [TestMethod]
        public void simpleteam_can_be_removed_from_cache()
        {
            Assert.AreEqual(0, _memoryCache.Count());
            var simpleTeamDto = CacheSimpleTeam(654321, null);

            Assert.AreEqual(1, _memoryCache.Count());
            var cacheItem = _memoryCache.GetCacheItem(simpleTeamDto.Competitor.Id.ToString());
            Assert.IsNotNull(cacheItem);
            Assert.AreEqual(simpleTeamDto.Competitor.Id.ToString(), cacheItem.Key);

            _cacheManager.RemoveCacheItem(simpleTeamDto.Competitor.Id, CacheItemType.Competitor, "Test");
            Assert.AreEqual(0, _memoryCache.Count());
            cacheItem = _memoryCache.GetCacheItem(simpleTeamDto.Competitor.Id.ToString());
            Assert.IsNull(cacheItem);
        }

        private SimpleTeamProfileDTO CacheSimpleTeam(int id, IDictionary<string, string> referenceIds)
        {
            var simpleTeam = MessageFactoryRest.GetSimpleTeamCompetitorProfileEndpoint(id, referenceIds);
            var simpleTeamDto = new SimpleTeamProfileDTO(simpleTeam);
            _cacheManager.SaveDto(simpleTeamDto.Competitor.Id, simpleTeamDto, CultureInfo.CurrentCulture, DtoType.SimpleTeamProfile, null);
            return simpleTeamDto;
        }
    }
}