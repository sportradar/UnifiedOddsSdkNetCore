/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class NameProviderFlexMarketsCacheTests
    {
        private INameProvider _nameProvider;

        private MemoryCache _variantMemoryCache;
        private MemoryCache _invariantMemoryCache;
        private MemoryCache _variantsMemoryCache;
        private IMarketDescriptionCache _variantMdCache;
        private IMarketDescriptionCache _inVariantMdCache;
        private IVariantDescriptionCache _variantsMdCache;
        private IMappingValidatorFactory _mappingValidatorFactory;

        private TestTimer _timer;
        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _variantMemoryCache = new MemoryCache("VariantCache");
            _invariantMemoryCache = new MemoryCache("InVariantCache");
            _variantsMemoryCache = new MemoryCache("VariantsCache");
            //force markets to be loaded
            _timer = new TestTimer(false);
            _timer.FireOnce(TimeSpan.Zero);
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                {"score", "1:1" }
            });

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _mappingValidatorFactory = new MappingValidatorFactory();

            _timer = new TestTimer(true);
            _variantMdCache = new VariantMarketDescriptionCache(_variantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _cacheManager);
            _inVariantMdCache = new InvariantMarketDescriptionCache(_invariantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, TestData.Cultures, _cacheManager);
            _variantsMdCache = new VariantDescriptionListCache(_variantsMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, TestData.Cultures, _cacheManager);

            _nameProvider = new NameProvider(
                                            new MarketCacheProvider(_inVariantMdCache, _variantMdCache, _variantsMdCache),
                                            new Mock<IProfileCache>().Object,
                                            new Mock<INameExpressionFactory>().Object,
                                            new Mock<ISportEvent>().Object,
                                            41,
                                            specifiers,
                                            ExceptionHandlingStrategy.THROW
                                        );
        }

        [TestMethod]
        public void outcome_names_for_flex_market_are_correct()
        {
            Assert.AreEqual("1:1", _nameProvider.GetOutcomeNameAsync("110", TestData.Culture).Result, "Name of outcome with id=110 is not correct");
            Assert.AreEqual("2:1", _nameProvider.GetOutcomeNameAsync("114", TestData.Culture).Result, "Name of outcome with id=114 is not correct");
            Assert.AreEqual("3:1", _nameProvider.GetOutcomeNameAsync("116", TestData.Culture).Result, "Name of outcome with id=116 is not correct");
            Assert.AreEqual("4:1", _nameProvider.GetOutcomeNameAsync("118", TestData.Culture).Result, "Name of outcome with id=118 is not correct");
            Assert.AreEqual("5:1", _nameProvider.GetOutcomeNameAsync("120", TestData.Culture).Result, "Name of outcome with id=120 is not correct");
            Assert.AreEqual("6:1", _nameProvider.GetOutcomeNameAsync("122", TestData.Culture).Result, "Name of outcome with id=122 is not correct");
            Assert.AreEqual("7:1", _nameProvider.GetOutcomeNameAsync("124", TestData.Culture).Result, "Name of outcome with id=124 is not correct");
            Assert.AreEqual("1:2", _nameProvider.GetOutcomeNameAsync("126", TestData.Culture).Result, "Name of outcome with id=126 is not correct");
            Assert.AreEqual("2:2", _nameProvider.GetOutcomeNameAsync("128", TestData.Culture).Result, "Name of outcome with id=128 is not correct");
            Assert.AreEqual("3:2", _nameProvider.GetOutcomeNameAsync("130", TestData.Culture).Result, "Name of outcome with id=130 is not correct");
            Assert.AreEqual("4:2", _nameProvider.GetOutcomeNameAsync("132", TestData.Culture).Result, "Name of outcome with id=132 is not correct");
            Assert.AreEqual("5:2", _nameProvider.GetOutcomeNameAsync("134", TestData.Culture).Result, "Name of outcome with id=134 is not correct");
            Assert.AreEqual("6:2", _nameProvider.GetOutcomeNameAsync("136", TestData.Culture).Result, "Name of outcome with id=136 is not correct");
            Assert.AreEqual("1:3", _nameProvider.GetOutcomeNameAsync("138", TestData.Culture).Result, "Name of outcome with id=138 is not correct");
            Assert.AreEqual("2:3", _nameProvider.GetOutcomeNameAsync("140", TestData.Culture).Result, "Name of outcome with id=140 is not correct");
            Assert.AreEqual("3:3", _nameProvider.GetOutcomeNameAsync("142", TestData.Culture).Result, "Name of outcome with id=142 is not correct");
            Assert.AreEqual("4:3", _nameProvider.GetOutcomeNameAsync("144", TestData.Culture).Result, "Name of outcome with id=144 is not correct");
            Assert.AreEqual("5:3", _nameProvider.GetOutcomeNameAsync("146", TestData.Culture).Result, "Name of outcome with id=146 is not correct");
            Assert.AreEqual("1:4", _nameProvider.GetOutcomeNameAsync("148", TestData.Culture).Result, "Name of outcome with id=148 is not correct");
            Assert.AreEqual("2:4", _nameProvider.GetOutcomeNameAsync("150", TestData.Culture).Result, "Name of outcome with id=150 is not correct");
            Assert.AreEqual("3:4", _nameProvider.GetOutcomeNameAsync("152", TestData.Culture).Result, "Name of outcome with id=152 is not correct");
            Assert.AreEqual("4:4", _nameProvider.GetOutcomeNameAsync("154", TestData.Culture).Result, "Name of outcome with id=154 is not correct");
            Assert.AreEqual("1:5", _nameProvider.GetOutcomeNameAsync("156", TestData.Culture).Result, "Name of outcome with id=156 is not correct");
            Assert.AreEqual("2:5", _nameProvider.GetOutcomeNameAsync("158", TestData.Culture).Result, "Name of outcome with id=158 is not correct");
            Assert.AreEqual("3:5", _nameProvider.GetOutcomeNameAsync("160", TestData.Culture).Result, "Name of outcome with id=160 is not correct");
            Assert.AreEqual("1:6", _nameProvider.GetOutcomeNameAsync("162", TestData.Culture).Result, "Name of outcome with id=162 is not correct");
            Assert.AreEqual("2:6", _nameProvider.GetOutcomeNameAsync("164", TestData.Culture).Result, "Name of outcome with id=164 is not correct");
            Assert.AreEqual("1:7", _nameProvider.GetOutcomeNameAsync("166", TestData.Culture).Result, "Name of outcome with id=166 is not correct");
        }
    }
}
