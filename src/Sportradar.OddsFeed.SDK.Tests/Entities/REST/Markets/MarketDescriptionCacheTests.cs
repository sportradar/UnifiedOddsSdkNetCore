/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using RMF = Sportradar.OddsFeed.SDK.Tests.Common.MessageFactoryRest;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class MarketDescriptionCacheTests
    {
        private static readonly CultureInfo FirstCulture = new CultureInfo("en");
        private static readonly CultureInfo SecondCulture = new CultureInfo("de");

        private readonly MemoryCache _variantMemoryCache;
        private readonly MemoryCache _invariantMemoryCache;
        private readonly IMarketDescriptionCache _variantMdCache;
        private readonly IMarketDescriptionCache _inVariantMdCache;
        private readonly IMappingValidatorFactory _mappingValidatorFactory;
        private readonly TestDataRouterManager _dataRouterManager;

        public MarketDescriptionCacheTests(ITestOutputHelper outputHelper)
        {
            _variantMemoryCache = new MemoryCache("VariantCache");
            _invariantMemoryCache = new MemoryCache("InVariantCache");

            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

            _mappingValidatorFactory = new MappingValidatorFactory();

            var timer = new TestTimer(true);
            _variantMdCache = new VariantMarketDescriptionCache(_variantMemoryCache, _dataRouterManager, _mappingValidatorFactory, cacheManager);
            _inVariantMdCache = new InvariantMarketDescriptionCache(_invariantMemoryCache, _dataRouterManager, _mappingValidatorFactory, timer, TestData.Cultures, cacheManager);
        }

        [Fact]
        public void MarketDescriptionMergeTest()
        {
            var msg1 = RMF.GetDescMarket(10);
            var msg2 = RMF.GetDescMarket(msg1);

            var ci = MarketDescriptionCacheItem.Build(new MarketDescriptionDTO(msg1), _mappingValidatorFactory, FirstCulture, "test");
            ci.Merge(new MarketDescriptionDTO(msg2), SecondCulture);

            Assert.Equal(msg1.id, ci.Id);
            Assert.Equal(msg1.name, ci.GetName(FirstCulture));
            Assert.Equal(msg1.description, ci.GetDescription(FirstCulture));
            Assert.Equal(msg1.variant, ci.Variant);
            Assert.Equal(msg1.mappings.Length, ci.Mappings.Count());
            Assert.Equal(msg1.outcomes.Length, ci.Outcomes.Count());
            Assert.Equal(msg1.specifiers.Length, ci.Specifiers.Count());

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

            Assert.Equal(msg2.id, ci.Id);
            Assert.Equal(msg2.name, ci.GetName(SecondCulture));
            Assert.Equal(msg2.description, ci.GetDescription(SecondCulture));
            Assert.Equal(msg2.variant, ci.Variant); // TODO: variant is not overriding
            Assert.Equal(msg2.mappings.Length, ci.Mappings.Count());
            Assert.Equal(msg2.outcomes.Length, ci.Outcomes.Count());
            Assert.Equal(msg2.specifiers.Length, ci.Specifiers.Count());

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

            Assert.NotEqual(ci.GetName(FirstCulture), ci.GetName(SecondCulture));
            Assert.NotEqual(ci.GetDescription(FirstCulture), ci.GetDescription(SecondCulture));

            Assert.True(ci.HasTranslationsFor(FirstCulture));
            Assert.True(ci.HasTranslationsFor(SecondCulture));
            Assert.False(ci.HasTranslationsFor(new CultureInfo("es")));
        }

        private static void ValidateMapping(mappingsMapping msg, MarketMappingCacheItem ci)
        {
            var ciMarketId = ci.MarketSubTypeId == null ? ci.MarketTypeId.ToString() : $"{ci.MarketTypeId}:{ci.MarketSubTypeId}";
            Assert.Equal(msg.market_id, ciMarketId);
            if (!string.IsNullOrEmpty(msg.product_ids))
            {
                var ids = msg.product_ids.Split(new[] { SdkInfo.MarketMappingProductsDelimiter }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                Assert.Equal(ids.Count, ci.ProducerIds.Count());
                foreach (var id in ids)
                {
                    Assert.True(ci.ProducerIds.Contains(id), $"Missing producerId:{id}.");
                }
            }
            else
            {
                Assert.Single(ci.ProducerIds);
                Assert.Equal(msg.product_id, ci.ProducerIds.First());
            }
            Assert.Equal(msg.sport_id, ci.SportId.ToString());
            Assert.Equal(msg.sov_template, ci.SovTemplate);
            Assert.Equal(msg.valid_for, ci.Validator.ToString());

            if (msg.mapping_outcome != null)
            {
                Assert.Equal(msg.mapping_outcome.Length, ci.OutcomeMappings.Count());

                for (var i = 0; i < msg.mapping_outcome.Length; i++)
                {
                    ValidateMappingOutcome(msg.mapping_outcome[i], ci.OutcomeMappings.ToArray()[i]);
                }
            }
        }

        private static void ValidateMappingOutcome(mappingsMappingMapping_outcome msg, OutcomeMappingCacheItem ci)
        {
            Assert.Equal(msg.outcome_id, ci.OutcomeId);
            Assert.Equal(msg.product_outcome_id, ci.ProducerOutcomeId);
            Assert.NotNull(ci.ProducerOutcomeNames);
            Assert.Equal(2, ci.ProducerOutcomeNames.Count);
            Assert.Equal(msg.product_outcome_name, ci.ProducerOutcomeNames.First().Value);
        }

        private static void ValidateOutcome(desc_outcomesOutcome msg, MarketOutcomeCacheItem ci, CultureInfo culture)
        {
            Assert.Equal(msg.id, ci.Id);
            Assert.Equal(msg.name, ci.GetName(culture));
            Assert.Equal(msg.description, ci.GetDescription(culture));
        }

        private static void ValidateSpecifier(desc_specifiersSpecifier msg, MarketSpecifierCacheItem ci)
        {
            Assert.Equal(msg.type, ci.Type);
            Assert.Equal(msg.name, ci.Name);
        }

        [Fact]
        public async Task VariantMarketDescriptionCacheIsCachingTest()
        {
            const string callType = "GetVariantMarketDescriptionAsync";
            Assert.Empty(_variantMemoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType));

            var market = await _variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", TestData.Cultures);
            Assert.NotNull(market);
            Assert.Single(_variantMemoryCache);
            Assert.Equal(3, _dataRouterManager.GetCallCount(callType));

            market = await _variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", TestData.Cultures);
            Assert.NotNull(market);
            Assert.Single(_variantMemoryCache);
            Assert.Equal(3, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public async Task InVariantMarketDescriptionCacheIsCachingTest()
        {
            const string callType = "GetMarketDescriptionsAsync";
            var market = await _inVariantMdCache.GetMarketDescriptionAsync(178, "lcoo:markettext:1", TestData.Cultures);
            Assert.NotNull(market);
            Assert.Equal(178, market.Id);
            Assert.Equal(TestData.InvariantListCacheCount, _invariantMemoryCache.Count());
            Assert.Equal(3, _dataRouterManager.GetCallCount(callType));

            market = await _inVariantMdCache.GetMarketDescriptionAsync(178, "lcoo:markettext:1", TestData.Cultures);
            Assert.NotNull(market);
            Assert.Equal(TestData.InvariantListCacheCount, _invariantMemoryCache.Count());
            Assert.Equal(3, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public void InVariantMarketDescriptionCacheIsAutoCachingTest()
        {
            const string callType = "GetMarketDescriptionsAsync";
            //The data is automatically fetched when the cache is constructed and timer is started in the cache constructor
            Assert.Equal(TestData.InvariantListCacheCount, _invariantMemoryCache.Count());
            Assert.Equal(3, _dataRouterManager.GetCallCount(callType));
        }
    }
}
