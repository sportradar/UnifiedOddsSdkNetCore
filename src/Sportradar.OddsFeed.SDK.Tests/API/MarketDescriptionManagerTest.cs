/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class MarketDescriptionManagerTest
    {
        private readonly IMarketDescriptionCache _variantMarketDescriptionCache;
        private readonly IVariantDescriptionCache _variantDescriptionListCache;
        private readonly IMarketDescriptionCache _invariantMarketDescriptionCache;
        private readonly IMarketCacheProvider _marketCacheProvider;

        private readonly TestDataRouterManager _dataRouterManager;
        private readonly TestProducersProvider _producersProvider;

        public MarketDescriptionManagerTest(ITestOutputHelper outputHelper)
        {
            var variantMarketDescriptionMemoryCache = new MemoryCache("variantMarketDescriptionCache");
            var variantDescriptionMemoryCache = new MemoryCache("variantDescriptionCache");
            var invariantMarketDescriptionMemoryCache = new MemoryCache("invariantMarketDescriptionCache");

            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);
            _producersProvider = new TestProducersProvider();

            IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

            var timerVdl = new TestTimer(true);
            var timerIdl = new TestTimer(true);
            _variantMarketDescriptionCache = new VariantMarketDescriptionCache(variantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, cacheManager);
            _variantDescriptionListCache = new VariantDescriptionListCache(variantDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, timerVdl, TestData.Cultures, cacheManager);
            _invariantMarketDescriptionCache = new InvariantMarketDescriptionCache(invariantMarketDescriptionMemoryCache, _dataRouterManager, mappingValidatorFactory, timerIdl, TestData.Cultures, cacheManager);

            _marketCacheProvider = new MarketCacheProvider(_invariantMarketDescriptionCache, _variantMarketDescriptionCache, _variantDescriptionListCache);
        }

        [Fact]
        public void MarketDescriptionManagerInit()
        {
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            Assert.NotNull(marketDescriptionManager);
        }

        [Fact]
        public async Task MarketDescriptionManagerGetMarketDescriptions()
        {
            // calls from initialization are done
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointVariantDescriptions]);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointMarketDescriptions]);
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            var marketDescriptions = (await marketDescriptionManager.GetMarketDescriptionsAsync()).ToList();
            // no new calls should be done, since already everything loaded
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointVariantDescriptions]);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointMarketDescriptions]);
            Assert.Equal(TestData.InvariantListCacheCount, marketDescriptions.Count);
        }

        [Fact]
        public async Task MarketDescriptionManagerGetMarketMapping()
        {
            var marketDescriptionManager = new MarketDescriptionManager(TestConfigurationInternal.GetConfig(), _marketCacheProvider, _invariantMarketDescriptionCache, _variantDescriptionListCache, _variantMarketDescriptionCache);
            var marketMapping = (await marketDescriptionManager.GetMarketMappingAsync(115, _producersProvider.GetProducers().First())).ToList();
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointVariantDescriptions]);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.RestMethodCalls[TestDataRouterManager.EndpointMarketDescriptions]);
            Assert.Equal(3, marketMapping.Count);
            Assert.Equal("6:14", marketMapping[0].MarketId);
        }
    }
}
