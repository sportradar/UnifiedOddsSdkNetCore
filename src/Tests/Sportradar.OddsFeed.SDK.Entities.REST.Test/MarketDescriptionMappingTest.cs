/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class MarketDescriptionMappingTest
    {
        private MemoryCache _variantMemoryCache;
        private MemoryCache _invariantMemoryCache;
        private IMarketDescriptionCache _variantMdCache;
        private IMarketDescriptionCache _inVariantMdCache;
        private IMappingValidatorFactory _mappingValidatorFactory;

        private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(1);
        private SdkTimer _timer;
        private CacheManager _cacheManager;
        private IDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _variantMemoryCache = new MemoryCache("VariantCache");
            _invariantMemoryCache = new MemoryCache("InVariantCache");

            _timer = new SdkTimer(_timerInterval, _timerInterval);

            _mappingValidatorFactory = new MappingValidatorFactory();

            _variantMdCache = new VariantMarketDescriptionCache(_variantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _cacheManager);
            _inVariantMdCache = new InvariantMarketDescriptionCache(_invariantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, new[] {TestData.Culture}, _cacheManager);
        }

        [TestMethod]
        public void InVariantMarketDescriptionCacheGetMarketMappingsTest()
        {
            var market1 = _inVariantMdCache.GetMarketDescriptionAsync(399, null, new[] {TestData.Culture}).Result;
            var market2 = _variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", new[] {TestData.Culture}).Result;

            Assert.IsNotNull(market1);
            Assert.IsNotNull(market2);
            Assert.AreEqual(399, market1.Id);
            Assert.AreEqual(10030, market2.Id);
            Assert.IsTrue(market1.Mappings.Any());
            Assert.IsTrue(market2.Outcomes.Any());
        }

        [TestMethod]
        public void InVariantMarketWithSpecifiersDescriptionCacheGetMarketMappingsTest()
        {
            var market1 = _inVariantMdCache.GetMarketDescriptionAsync(203, null, TestData.Cultures3).Result;

            Assert.IsNotNull(market1);
            Assert.AreEqual(203, market1.Id);
            Assert.IsTrue(market1.Mappings.Any());
            Assert.AreEqual(2, market1.Mappings.Count());
            Assert.IsTrue(market1.Outcomes.Any());
        }
    }
}
