/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class MarketDescriptionManagerTest
    {
        private MemoryCache _variantMarketDescriptionMemoryCache;
        private MemoryCache _variantDescriptionMemoryCache;
        private MemoryCache _invariantMarketDescriptionMemoryCache;
        private IMarketDescriptionCache _variantMarketDescriptionCache;
        private IVariantDescriptionCache _variantDescriptionListCache;
        private IMarketDescriptionCache _invariantMarketDescriptionCache;
        private IMarketCacheProvider _marketCacheProvider;
        private IMappingValidatorFactory _mappingValidatorFactory;

        private TestTimer _timer;
        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;
        private TestProducersProvider _producersProvider;

        [TestInitialize]
        public void Init()
        {
            _variantMarketDescriptionMemoryCache = new MemoryCache("variantMarketDescriptionCache");
            _variantDescriptionMemoryCache = new MemoryCache("variantDescriptionCache");
            _invariantMarketDescriptionMemoryCache = new MemoryCache("invariantMarketDescriptionCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);
            _producersProvider = new TestProducersProvider();

            _mappingValidatorFactory = new MappingValidatorFactory();

            _timer = new TestTimer(true);
            _variantMarketDescriptionCache = new VariantMarketDescriptionCache(_variantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _cacheManager);
            _variantDescriptionListCache = new VariantDescriptionListCache(_variantDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, TestData.Cultures, _cacheManager);
            _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(_invariantMarketDescriptionMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, TestData.Cultures, _cacheManager);

            _marketCacheProvider = new MarketCacheProvider(_invariantMarketDescriptionCache, _variantMarketDescriptionCache, _variantDescriptionListCache);
        }

        [TestMethod]
        public void MarketDescriptionManagerInitTest()
        {
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            Assert.IsNotNull(marketDescriptionManager);
        }

        [TestMethod]
        public void MarketDescriptionManagerGetMarketDescriptionsTest()
        {
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            var marketDescriptions = marketDescriptionManager.GetMarketDescriptionsAsync().Result.ToList();
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestCalls["GetVariantDescriptionsAsync"]);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestCalls["GetMarketDescriptionsAsync"]);
            Assert.AreEqual(750, marketDescriptions.Count);
            Assert.AreEqual(115, marketDescriptions[0].Id);
        }

        [TestMethod]
        public void MarketDescriptionManagerGetMarketMappingTest()
        {
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            var marketMapping = marketDescriptionManager.GetMarketMappingAsync(115, _producersProvider.GetProducers().First()).Result.ToList();
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestCalls["GetVariantDescriptionsAsync"]);
            Assert.AreEqual(TestData.Cultures.Count, _dataRouterManager.RestCalls["GetMarketDescriptionsAsync"]);
            Assert.AreEqual(3, marketMapping.Count);
            Assert.AreEqual("6:14", marketMapping[0].MarketId);
        }
    }
}
