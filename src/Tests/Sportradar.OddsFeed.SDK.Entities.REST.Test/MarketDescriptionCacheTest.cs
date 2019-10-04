/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using RMF = Sportradar.OddsFeed.SDK.Test.Shared.MessageFactoryRest;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class MarketDescriptionCacheTest
    {
        private static readonly CultureInfo FirstCulture = new CultureInfo("en");
        private static readonly CultureInfo SecondCulture = new CultureInfo("de");

        private MemoryCache _variantMemoryCache;
        private MemoryCache _invariantMemoryCache;
        private IMarketDescriptionCache _variantMdCache;
        private IMarketDescriptionCache _inVariantMdCache;
        private IMappingValidatorFactory _mappingValidatorFactory;

        private TestTimer _timer;
        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _variantMemoryCache = new MemoryCache("VariantCache");
            _invariantMemoryCache = new MemoryCache("InVariantCache");

            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);

            _mappingValidatorFactory = new MappingValidatorFactory();

            _timer = new TestTimer(true);
            _variantMdCache = new VariantMarketDescriptionCache(_variantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _cacheManager);
            _inVariantMdCache = new InvariantMarketDescriptionCache(_invariantMemoryCache, _dataRouterManager, _mappingValidatorFactory, _timer, TestData.Cultures, _cacheManager);
        }

        [TestMethod]
        public void MarketDescriptionMergeTest()
        {
            var msg1 = RMF.GetDescMarket(10);
            var msg2 = RMF.GetDescMarket(msg1);

            var ci = MarketDescriptionCacheItem.Build(new MarketDescriptionDTO(msg1), _mappingValidatorFactory, FirstCulture, "test");
            ci.Merge(new MarketDescriptionDTO(msg2), SecondCulture);

            Assert.AreEqual(msg1.id, ci.Id);
            Assert.AreEqual(msg1.name, ci.GetName(FirstCulture));
            Assert.AreEqual(msg1.description, ci.GetDescription(FirstCulture));
            Assert.AreEqual(msg1.variant, ci.Variant);
            Assert.AreEqual(msg1.mappings.Length, ci.Mappings.Count());
            Assert.AreEqual(msg1.outcomes.Length, ci.Outcomes.Count());
            Assert.AreEqual(msg1.specifiers.Length, ci.Specifiers.Count());

            for (var i = 0; i < msg1.mappings.Length; i++)
            {
                ValidateMapping(msg1.mappings[i], ci.Mappings.ToArray()[i]);
            }

            for (var i = 0; i < msg1.outcomes.Length; i++)
            {
                ValidateOutcome(msg1.outcomes[i], ci.Outcomes.ToArray()[i], FirstCulture);
            }

            for (var i = 0; i < msg1.specifiers.Length; i++)
            {
                ValidateSpecifier(msg1.specifiers[i], ci.Specifiers.ToArray()[i]);
            }

            Assert.AreEqual(msg2.id, ci.Id);
            Assert.AreEqual(msg2.name, ci.GetName(SecondCulture));
            Assert.AreEqual(msg2.description, ci.GetDescription(SecondCulture));
            Assert.AreEqual(msg2.variant, ci.Variant); // TODO: variant is not overriding
            Assert.AreEqual(msg2.mappings.Length, ci.Mappings.Count());
            Assert.AreEqual(msg2.outcomes.Length, ci.Outcomes.Count());
            Assert.AreEqual(msg2.specifiers.Length, ci.Specifiers.Count());

            for (var i = 0; i < msg2.mappings.Length; i++)
            {
                ValidateMapping(msg2.mappings[i], ci.Mappings.ToArray()[i]);
            }

            for (var i = 0; i < msg2.outcomes.Length; i++)
            {
                ValidateOutcome(msg2.outcomes[i], ci.Outcomes.ToArray()[i], SecondCulture);
            }

            for (var i = 0; i < msg2.specifiers.Length; i++)
            {
                ValidateSpecifier(msg2.specifiers[i], ci.Specifiers.ToArray()[i]);
            }

            Assert.AreNotEqual(ci.GetName(FirstCulture), ci.GetName(SecondCulture));
            Assert.AreNotEqual(ci.GetDescription(FirstCulture), ci.GetDescription(SecondCulture));

            Assert.IsTrue(ci.HasTranslationsFor(FirstCulture));
            Assert.IsTrue(ci.HasTranslationsFor(SecondCulture));
            Assert.IsFalse(ci.HasTranslationsFor(new CultureInfo("es")));
        }

        private static void ValidateMapping(mappingsMapping msg, MarketMappingCacheItem ci)
        {
            var ciMarketId = ci.MarketSubTypeId == null ? ci.MarketTypeId.ToString() : $"{ci.MarketTypeId}:{ci.MarketSubTypeId}";
            Assert.AreEqual(msg.market_id, ciMarketId);
            Assert.AreEqual(msg.product_id, ci.ProducerId);
            if (!string.IsNullOrEmpty(msg.product_ids))
            {
                var ids = msg.product_ids.Split(new[] {SdkInfo.MarketMappingProductsDelimiter}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                Assert.AreEqual(ids.Count, ci.ProducerIds.Count());
                foreach (var id in ids)
                {
                    Assert.IsTrue(ci.ProducerIds.Contains(id), $"Missing producerId:{id}.");
                }
            }
            else
            {
                Assert.AreEqual(1, ci.ProducerIds.Count());
                Assert.AreEqual(msg.product_id, ci.ProducerIds.First());
            }
            Assert.AreEqual(msg.sport_id, ci.SportId.ToString());
            Assert.AreEqual(msg.sov_template, ci.SovTemplate);
            Assert.AreEqual(msg.valid_for, ci.Validator.ToString());

            if (msg.mapping_outcome != null)
            {
                Assert.AreEqual(msg.mapping_outcome.Length, ci.OutcomeMappings.Count());

                for (var i = 0; i < msg.mapping_outcome.Length; i++)
                {
                    ValidateMappingOutcome(msg.mapping_outcome[i], ci.OutcomeMappings.ToArray()[i]);
                }
            }
        }

        private static void ValidateMappingOutcome(mappingsMappingMapping_outcome msg, OutcomeMappingCacheItem ci)
        {
            Assert.AreEqual(msg.outcome_id, ci.OutcomeId);
            Assert.AreEqual(msg.product_outcome_id, ci.ProducerOutcomeId);
            Assert.IsNotNull(ci.ProducerOutcomeNames);
            Assert.AreEqual(2, ci.ProducerOutcomeNames.Count);
            Assert.AreEqual(msg.product_outcome_name, ci.ProducerOutcomeNames.First().Value);
        }

        private static void ValidateOutcome(desc_outcomesOutcome msg, MarketOutcomeCacheItem ci, CultureInfo culture)
        {
            Assert.AreEqual(msg.id, ci.Id);
            Assert.AreEqual(msg.name, ci.GetName(culture));
            Assert.AreEqual(msg.description, ci.GetDescription(culture));
        }

        private static void ValidateSpecifier(desc_specifiersSpecifier msg, MarketSpecifierCacheItem ci)
        {
            Assert.AreEqual(msg.type, ci.Type);
            Assert.AreEqual(msg.name, ci.Name);
        }

        [TestMethod]
        public void VariantMarketDescriptionCacheIsCachingTest()
        {
            const string callType = "GetVariantMarketDescriptionAsync";
            Assert.AreEqual(0, _variantMemoryCache.Count());
            Assert.AreEqual(0, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly 0 times.");

            var market =_variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", TestData.Cultures).Result;
            Assert.IsNotNull(market);
            Assert.AreEqual(1, _variantMemoryCache.Count());
            Assert.AreEqual(3, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly 3 times.");

            market = _variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", TestData.Cultures).Result;
            Assert.IsNotNull(market);
            Assert.AreEqual(1, _variantMemoryCache.Count());
            Assert.AreEqual(3, _dataRouterManager.GetCallCount(callType), $"{callType} - no new call should be made.");
        }

        [TestMethod]
        public void InVariantMarketDescriptionCacheIsCachingTest()
        {
            const string callType = "GetMarketDescriptionsAsync";
            var market = _inVariantMdCache.GetMarketDescriptionAsync(178, "lcoo:markettext:1", TestData.Cultures).Result;
            Assert.IsNotNull(market);
            Assert.AreEqual(178, market.Id);
            Assert.AreEqual(750, _invariantMemoryCache.Count());
            Assert.AreEqual(3, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly 3 times.");

            market = _inVariantMdCache.GetMarketDescriptionAsync(178, "lcoo:markettext:1", TestData.Cultures).Result;
            Assert.IsNotNull(market);
            Assert.AreEqual(750, _invariantMemoryCache.Count());
            Assert.AreEqual(3, _dataRouterManager.GetCallCount(callType), $"{callType} - no new call should be made.");
        }

        [TestMethod]
        public void InVariantMarketDescriptionCacheIsAutoCachingTest()
        {
            const string callType = "GetMarketDescriptionsAsync";
            //The data is automatically fetched when the cache is constructed and timer is started in the cache constructor
            Assert.AreEqual(750, _invariantMemoryCache.Count());
            Assert.AreEqual(3, _dataRouterManager.GetCallCount(callType), $"{callType} should be called exactly 3 times.");
        }
    }
}
