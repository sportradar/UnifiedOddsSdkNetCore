/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            return new URN("sr", "simple_team", competitorId);
        }

        [TestMethod]
        public void PlayerProfileGetsCached()
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
        public void NumberOfPlayerProfileProviderCallsMatchIsCorrect()
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
        public void CompetitorProfileGetsCached()
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
        public void NumberOfCompetitorProfileProviderCallsMatchIsCorrect()
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
        public void SimpleteamProfileGetsCached()
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
        public void NumberOfSimpleteamProviderCallsMatchIsCorrect()
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
        public void SimpleteamIsCachedWithoutBetradarId()
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
        public void SimpleteamIsCachedWithoutReferenceIds()
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
        public void SimpleteamIsCachedWithBetradarId()
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
        public void SimpleteamCanBeRemovedFromCache()
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

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsRequestForOneShouldNotInvokeApiRequests()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());

            var cultures = new List<CultureInfo> { TestData.Culture };
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            
            var cid = StaticRandom.I1000;
            for (var i = 0; i < 10; i++)
            {
                Debug.Print(i.ToString());
                var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), cultures).Result;
                Assert.IsNotNull(competitor);
            }

            Assert.AreEqual(1, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called {cultures.Count} times.");
        }

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsWithLanguagesRequestForOneShouldNotInvokeApiRequests()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());

            var cid = StaticRandom.I1000;
            for (var i = 1; i < 10; i++)
            {
                Debug.Print(i.ToString());
                var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), TestData.Cultures).Result;
                Assert.IsNotNull(competitor);
            }

            Assert.AreEqual(1, _memoryCache.Count(s => s.Key.Contains(":competitor:")));

            Assert.AreEqual(TestData.Cultures.Count , _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsWithLanguagesAreCachedAndApiCalledOnceForEach()
        {
            const string callType = "GetCompetitorAsync";
            const int cidCount = 100;
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());

            for (var i = 0; i < 1000; i++)
            {
                Debug.Print(i.ToString());
                var cid = i < cidCount ? i + 1 : StaticRandom.I(cidCount);
                var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), TestData.Cultures).Result;
                Assert.IsNotNull(competitor);
            }

            Assert.AreEqual(cidCount, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(TestData.Cultures.Count * cidCount, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count * cidCount} times.");
        }

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsRequestForOneShouldNotInvokeApiRequestsDelayed()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());

            var cultures = new List<CultureInfo> { TestData.Culture };
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            _dataRouterManager.AddDelay(TimeSpan.FromSeconds(5), true, 100);

            var cid = StaticRandom.I1000;
            for (var i = 0; i < 10; i++)
            {
                Debug.Print(i.ToString());
                var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), cultures).Result;
                Assert.IsNotNull(competitor);
            }

            Assert.AreEqual(1, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called {cultures.Count} times.");
        }

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsWithLanguagesRequestForOneShouldNotInvokeApiRequestsDelayed()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            _dataRouterManager.AddDelay(TimeSpan.FromSeconds(5), true, 100);

            var cid = StaticRandom.I1000;
            for (var i = 1; i < 10; i++)
            {
                Debug.Print(i.ToString());
                var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), TestData.Cultures).Result;
                Assert.IsNotNull(competitor);
            }

            Assert.AreEqual(1, _memoryCache.Count(s => s.Key.Contains(":competitor:")));

            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        [Timeout(180000)]
        public void MultipleCompetitorsWithLanguagesAreCachedAndApiCalledOnceForEachDelayed()
        {
            const string callType = "GetCompetitorAsync";
            const int cidCount = 100;
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            _dataRouterManager.AddDelay(TimeSpan.FromSeconds(10), true, 10);

            for (var i = 0; i < 1000; i++)
            {
                Debug.Print($"{DateTime.Now} - {i}");
                var cid = i < cidCount ? i + 1 : StaticRandom.I(cidCount);
                var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), TestData.Cultures).Result;
                Assert.IsNotNull(competitor);
            }

            Assert.AreEqual(cidCount, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(TestData.Cultures.Count * cidCount, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count * cidCount} times.");
        }

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsRequestForOneShouldNotInvokeApiRequestsDelayedAsync()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());

            var cultures = new List<CultureInfo> { TestData.Culture };
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            _dataRouterManager.AddDelay(TimeSpan.FromSeconds(5), true, 100);

            var cid = StaticRandom.I1000;
            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                var task = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), cultures);
                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();

            Assert.AreEqual(1, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called {cultures.Count} times.");
        }

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsWithLanguagesRequestForOneShouldNotInvokeApiRequestsDelayedAsync()
        {
            const string callType = "GetCompetitorAsync";
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            _dataRouterManager.AddDelay(TimeSpan.FromSeconds(5), true, 100);

            var tasks = new List<Task>();
            var cid = StaticRandom.I1000;
            for (var i = 1; i < 10; i++)
            {
                var task = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), TestData.Cultures);
                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();

            Assert.AreEqual(1, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count} times.");
        }

        [TestMethod]
        [Timeout(30000)]
        public void MultipleCompetitorsWithLanguagesAreCachedAndApiCalledOnceForEachDelayedAsync()
        {
            const string callType = "GetCompetitorAsync";
            const int cidCount = 100;
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            _dataRouterManager.AddDelay(TimeSpan.FromSeconds(10), true, 10);

            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var cid = i < cidCount ? i + 1 : StaticRandom.I(cidCount);
                var task = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), TestData.Cultures);
                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();

            Assert.AreEqual(cidCount, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(TestData.Cultures.Count * cidCount, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count * cidCount} times.");
        }

        [TestMethod]
        [Timeout(120000)]
        public void MultipleCompetitorsWithLanguagesAreCachedAndApiCalledOnceForEachDelayedAsyncPerf()
        {
            const string callType = "GetCompetitorAsync";
            const int cidCount = 1000;
            Assert.IsNotNull(_memoryCache);
            Assert.IsTrue(!_memoryCache.Any());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 times.");
            _dataRouterManager.AddDelay(TimeSpan.FromSeconds(10), true, 10);

            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
            {
                var cid = i < cidCount ? i + 1 : StaticRandom.I(cidCount);
                var task = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(cid), TestData.Cultures);
                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();

            Assert.AreEqual(cidCount, _memoryCache.Count(s => s.Key.Contains(":competitor:")));
            Assert.AreEqual(TestData.Cultures.Count * cidCount, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly {TestData.Cultures.Count * cidCount} times.");
        }
    }
}