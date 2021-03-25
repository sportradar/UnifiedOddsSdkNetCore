/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class NameProviderPlayerProfileTests
    {
        private MemoryCache _memoryCache;
        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;
        private IProfileCache _profileCache;
        private INameProvider _nameProvider;

        [TestInitialize]
        public void Init()
        {
            _memoryCache = new MemoryCache("cache");
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);
            _profileCache = new ProfileCache(_memoryCache, _dataRouterManager, _cacheManager);

            _nameProvider = new NameProvider(
                                            new Mock<IMarketCacheProvider>().Object,
                                            _profileCache,
                                            new Mock<INameExpressionFactory>().Object,
                                            new Mock<ISportEvent>().Object,
                                            1,
                                            null,
                                            ExceptionHandlingStrategy.THROW);
        }

        [TestMethod]
        public void id_for_one_player_profiles_gets_correct_name()
        {
            var name = _nameProvider.GetOutcomeNameAsync("sr:player:2", new CultureInfo("en")).Result;
            Assert.AreEqual("Cole, Ashley", name, "The generated name is not correct");
        }

        [TestMethod]
        public void composite_id_for_two_player_profiles_gets_correct_name()
        {
            var name = _nameProvider.GetOutcomeNameAsync("sr:player:1,sr:player:2", new CultureInfo("en")).Result;
            Assert.AreEqual("van Persie, Robin,Cole, Ashley", name, "The generated name is not correct");
        }

        [TestMethod]
        public void player_profile_is_called_only_once()
        {
            const string callType = "GetPlayerProfileAsync";
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 time.");
            var name = _nameProvider.GetOutcomeNameAsync("sr:player:2", TestData.Cultures.First()).Result;
            Assert.AreEqual("Cole, Ashley", name, "The generated name is not correct");
            Assert.AreEqual(1, _memoryCache.Count());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 1 time.");
        }

        [TestMethod]
        public void competitor_and_player_profile_is_called_correctly()
        {
            const string callType1 = "GetPlayerProfileAsync";
            const string callType2 = "GetCompetitorAsync";
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType1), $"{callType1} should be called 0 time.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType2), $"{callType2} should be called 0 time.");
            var name = _nameProvider.GetOutcomeNameAsync("sr:player:1,sr:competitor:1", TestData.Cultures.First()).Result;
            Assert.AreEqual("van Persie, Robin,Queens Park Rangers", name, "The generated name is not correct");
            Assert.AreEqual(35, _memoryCache.Count());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(callType1), $"{callType1} should be called 1 time.");
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(callType2), $"{callType2} should be called 1 time.");
        }

        [TestMethod]
        public void player_profile_is_called_only_once_per_culture()
        {
            const string callType = "GetPlayerProfileAsync";
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 time.");
            foreach (var cultureInfo in TestData.Cultures)
            {
                var name = _nameProvider.GetOutcomeNameAsync("sr:player:2", cultureInfo).Result;
                Assert.AreEqual("Cole, Ashley", name, "The generated name is not correct");
            }
            Assert.AreEqual(1, _memoryCache.Count());
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called {TestData.Cultures.Count} time.");
        }

        [TestMethod]
        public void competitor_profile_is_called_only_once()
        {
            const string callType = "GetCompetitorAsync";
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 time.");
            var name = _nameProvider.GetOutcomeNameAsync("sr:competitor:1", TestData.Cultures.First()).Result;
            Assert.AreEqual("Queens Park Rangers", name, "The generated name is not correct");
            Assert.AreEqual(35, _memoryCache.Count());
            Assert.AreEqual(1, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 1 time.");
        }

        [TestMethod]
        public void competitor_profile_is_called_only_once_per_culture()
        {
            const string callType = "GetCompetitorAsync";
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called 0 time.");
            foreach (var cultureInfo in TestData.Cultures)
            {
                var name = _nameProvider.GetOutcomeNameAsync("sr:competitor:1", cultureInfo).Result;
                Assert.AreEqual("Queens Park Rangers", name, "The generated name is not correct");
            }
            Assert.AreEqual(35, _memoryCache.Count());
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType), $"{callType} should be called {TestData.Cultures.Count} time.");
        }

        [TestMethod]
        public void competitor_profile_is_called_when_wanted_player_profile()
        {
            var match = new TestSportEntityFactory().BuildSportEvent<IMatch>(URN.Parse("sr:match:1"), URN.Parse("sr:sport:5"), TestData.Cultures, ExceptionHandlingStrategy.THROW);

            _nameProvider = new NameProvider(
                                            new Mock<IMarketCacheProvider>().Object,
                                            _profileCache,
                                            new Mock<INameExpressionFactory>().Object,
                                            match,
                                            1,
                                            null,
                                            ExceptionHandlingStrategy.THROW);

            const string callType1 = "GetPlayerProfileAsync";
            const string callType2 = "GetCompetitorAsync";
            Assert.AreEqual(0, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType1), $"{callType1} should be called 0 time.");
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType2), $"{callType2} should be called 0 time.");
            var name = _nameProvider.GetOutcomeNameAsync("sr:player:1", TestData.Cultures.First()).Result;
            Assert.AreEqual("van Persie, Robin", name, "The generated name is not correct");
            Assert.AreEqual(61+2, _memoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType1), $"{callType1} should be called 0 time.");
            Assert.AreEqual(2, _dataRouterManager.GetCallCount(callType2), $"{callType2} should be called 2 time.");
        }
    }
}
