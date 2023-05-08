/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class MarketDescriptionMappingTests
    {
        private readonly IMarketDescriptionCache _variantMdCache;
        private readonly IMarketDescriptionCache _inVariantMdCache;

        private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds(1);

        public MarketDescriptionMappingTests(ITestOutputHelper outputHelper)
        {
            var cacheManager = new CacheManager();
            var dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

            var variantMemoryCache = new MemoryCache("VariantCache");
            var invariantMemoryCache = new MemoryCache("InVariantCache");

            var timer = new SdkTimer(_timerInterval, _timerInterval);

            IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

            _variantMdCache = new VariantMarketDescriptionCache(variantMemoryCache, dataRouterManager, mappingValidatorFactory, cacheManager);
            _inVariantMdCache = new InvariantMarketDescriptionCache(invariantMemoryCache, dataRouterManager, mappingValidatorFactory, timer, new[] { TestData.Culture }, cacheManager);
        }

        [Fact]
        public async Task InVariantMarketDescriptionCacheGetMarketMappings()
        {
            var market1 = await _inVariantMdCache.GetMarketDescriptionAsync(399, null, new[] { TestData.Culture });
            var market2 = await _variantMdCache.GetMarketDescriptionAsync(10030, "lcoo:markettext:33421", new[] { TestData.Culture });

            Assert.NotNull(market1);
            Assert.NotNull(market2);
            Assert.Equal(399, market1.Id);
            Assert.Equal(10030, market2.Id);
            Assert.True(market1.Mappings.Any());
            Assert.True(market2.Outcomes.Any());
        }

        [Fact]
        public async Task InVariantMarketWithSpecifiersDescriptionCacheGetMarketMappings()
        {
            var market1 = await _inVariantMdCache.GetMarketDescriptionAsync(203, null, TestData.Cultures3);

            Assert.NotNull(market1);
            Assert.Equal(203, market1.Id);
            Assert.True(market1.Mappings.Any());
            Assert.Equal(3, market1.Mappings.Count());
            Assert.True(market1.Outcomes.Any());
        }
    }
}
