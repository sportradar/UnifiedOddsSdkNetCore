/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class ProfileCacheTests
    {
        private readonly ICacheManager _cacheManager;
        private readonly MemoryCache _memoryCache;
        private readonly IProfileCache _profileCache;
        private readonly TestDataRouterManager _dataRouterManager;

        public ProfileCacheTests(ITestOutputHelper outputHelper)
        {
            _memoryCache = new MemoryCache("test");
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);
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

        [Fact]
        public void PlayerProfileGetsCached()
        {
            const string callType = "GetPlayerProfileAsync";
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType));
            Assert.True(!_memoryCache.Any());
            var player = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures).Result;
            Assert.NotNull(player);
            Assert.Single(_memoryCache);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));

            //if we call again, should not fetch again
            player = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures).Result;
            Assert.NotNull(player);
            Assert.Single(_memoryCache);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public void NumberOfPlayerProfileProviderCallsMatchIsCorrect()
        {
            const string callType = "GetPlayerProfileAsync";
            var player1 = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(1), TestData.Cultures).Result;
            Assert.NotNull(player1);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));

            var player2 = _profileCache.GetPlayerProfileAsync(CreatePlayerUrn(2), TestData.Cultures).Result;
            Assert.NotNull(player2);

            Assert.NotEqual(player1.Id, player2.Id);
            Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public void CompetitorProfileGetsCached()
        {
            const string callType = "GetCompetitorAsync";
            Assert.NotNull(_memoryCache);
            Assert.True(!_memoryCache.Any());

            var competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures).Result;

            Assert.NotNull(competitor);
            Assert.Equal(35, _memoryCache.Count());

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));

            //if we call again, should not fetch again
            competitor = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures).Result;
            Assert.NotNull(competitor);
            Assert.Equal(35, _memoryCache.Count());

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public void NumberOfCompetitorProfileProviderCallsMatchIsCorrect()
        {
            const string callType = "GetCompetitorAsync";
            var competitor1 = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(1), TestData.Cultures).Result;
            Assert.NotNull(competitor1);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));

            var competitor2 = _profileCache.GetCompetitorProfileAsync(CreateCompetitorUrn(2), TestData.Cultures).Result;
            Assert.NotNull(competitor2);

            Assert.NotEqual(competitor1.Id, competitor2.Id);
            Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public void SimpleTeamProfileGetsCached()
        {
            const string callType = "GetCompetitorAsync";
            Assert.NotNull(_memoryCache);
            Assert.True(!_memoryCache.Any());

            var competitor = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;

            Assert.NotNull(competitor);
            Assert.Single(_memoryCache);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));

            //if we call again, should not fetch again
            competitor = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;
            Assert.NotNull(competitor);
            Assert.Single(_memoryCache);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public void NumberOfSimpleTeamProviderCallsMatchIsCorrect()
        {
            const string callType = "GetCompetitorAsync";
            var competitor1 = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;
            Assert.NotNull(competitor1);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));

            var competitor2 = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(2), TestData.Cultures).Result;
            Assert.NotNull(competitor2);

            Assert.NotEqual(competitor1.Id, competitor2.Id);
            Assert.Equal(TestData.Cultures.Count * 2, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public void SimpleTeamIsCachedWithoutBetradarId()
        {
            Assert.Empty(_memoryCache);
            var competitorCI = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(1), TestData.Cultures).Result;
            Assert.NotNull(competitorCI);
            Assert.NotNull(competitorCI.ReferenceId);
            Assert.NotNull(competitorCI.ReferenceId.ReferenceIds);
            Assert.True(competitorCI.ReferenceId.ReferenceIds.Any());
            Assert.Equal(1, competitorCI.ReferenceId.ReferenceIds.Count);
            Assert.Equal(1, competitorCI.ReferenceId.BetradarId);
        }

        [Fact]
        public void SimpleTeamIsCachedWithoutReferenceIds()
        {
            Assert.Empty(_memoryCache);
            var simpleTeamDto = CacheSimpleTeam(1, null);

            var competitorCI = _profileCache.GetCompetitorProfileAsync(simpleTeamDto.Competitor.Id, TestData.Cultures).Result;
            Assert.NotNull(competitorCI);
            Assert.Equal(simpleTeamDto.Competitor.Id, competitorCI.Id);
            Assert.NotNull(competitorCI.ReferenceId);
            Assert.NotNull(competitorCI.ReferenceId.ReferenceIds);
            Assert.True(competitorCI.ReferenceId.ReferenceIds.Any());
            Assert.Equal(1, competitorCI.ReferenceId.ReferenceIds.Count);
            Assert.Equal(simpleTeamDto.Competitor.Id.Id, competitorCI.ReferenceId.BetradarId);
        }

        [Fact]
        public void SimpleTeamIsCachedWithBetradarId()
        {
            Assert.Empty(_memoryCache);
            var competitorCI = _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(2), TestData.Cultures).Result;
            Assert.NotNull(competitorCI);
            Assert.NotNull(competitorCI.ReferenceId);
            Assert.NotNull(competitorCI.ReferenceId.ReferenceIds);
            Assert.True(competitorCI.ReferenceId.ReferenceIds.Any());
            Assert.Equal(2, competitorCI.ReferenceId.ReferenceIds.Count);
            Assert.Equal("555", competitorCI.ReferenceId.BetradarId.ToString());
        }

        [Fact]
        public void SimpleTeamCanBeRemovedFromCache()
        {
            Assert.Empty(_memoryCache);
            var simpleTeamDto = CacheSimpleTeam(654321, null);

            Assert.Single(_memoryCache);
            var cacheItem = _memoryCache.GetCacheItem(simpleTeamDto.Competitor.Id.ToString());
            Assert.NotNull(cacheItem);
            Assert.Equal(simpleTeamDto.Competitor.Id.ToString(), cacheItem.Key);

            _cacheManager.RemoveCacheItem(simpleTeamDto.Competitor.Id, CacheItemType.Competitor, "Test");
            Assert.Empty(_memoryCache);
            cacheItem = _memoryCache.GetCacheItem(simpleTeamDto.Competitor.Id.ToString());
            Assert.Null(cacheItem);
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
