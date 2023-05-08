// /*
// * Copyright (C) Sportradar AG. See LICENSE for full license governing this code
// */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class NameProviderFlexMarketsCacheTests
    {
        private readonly INameProvider _nameProvider;
        private readonly TestDataRouterManager _dataRouterManager;

        public NameProviderFlexMarketsCacheTests(ITestOutputHelper outputHelper)
        {
            var variantMemoryCache = new MemoryCache("VariantCache");
            var invariantMemoryCache = new MemoryCache("InVariantCache");
            var variantsMemoryCache = new MemoryCache("VariantsCache");
            //force markets to be loaded
            var timer = new TestTimer(false);
            timer.FireOnce(TimeSpan.Zero);
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
             {
                 {"score", "1:1" }
             });

            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);

            IMappingValidatorFactory mappingValidatorFactory = new MappingValidatorFactory();

            timer = new TestTimer(true);
            IMarketDescriptionCache variantMdCache = new VariantMarketDescriptionCache(variantMemoryCache, _dataRouterManager, mappingValidatorFactory, cacheManager);
            IMarketDescriptionCache inVariantMdCache = new InvariantMarketDescriptionCache(invariantMemoryCache, _dataRouterManager, mappingValidatorFactory, timer, TestData.Cultures, cacheManager);
            IVariantDescriptionCache variantsMdCache = new VariantDescriptionListCache(variantsMemoryCache, _dataRouterManager, mappingValidatorFactory, timer, TestData.Cultures, cacheManager);

            _nameProvider = new NameProvider(
                new MarketCacheProvider(inVariantMdCache, variantMdCache, variantsMdCache),
                new Mock<IProfileCache>().Object,
                new Mock<INameExpressionFactory>().Object,
                new Mock<ISportEvent>().Object,
                41,
                specifiers,
                ExceptionHandlingStrategy.THROW
            );
        }

        [Theory]
        [InlineData("110", "1:1")]
        [InlineData("114", "2:1")]
        [InlineData("116", "3:1")]
        [InlineData("118", "4:1")]
        [InlineData("120", "5:1")]
        [InlineData("122", "6:1")]
        [InlineData("124", "7:1")]
        [InlineData("126", "1:2")]
        [InlineData("128", "2:2")]
        [InlineData("130", "3:2")]
        [InlineData("132", "4:2")]
        [InlineData("134", "5:2")]
        [InlineData("136", "6:2")]
        [InlineData("138", "1:3")]
        [InlineData("140", "2:3")]
        [InlineData("142", "3:3")]
        [InlineData("144", "4:3")]
        [InlineData("146", "5:3")]
        [InlineData("148", "1:4")]
        [InlineData("150", "2:4")]
        [InlineData("152", "3:4")]
        [InlineData("154", "4:4")]
        [InlineData("156", "1:5")]
        [InlineData("158", "2:5")]
        [InlineData("160", "3:5")]
        [InlineData("162", "1:6")]
        [InlineData("164", "2:6")]
        [InlineData("166", "1:7")]
        public async Task OutcomeNamesForFlexMarketAreCorrect(string outcomeId, string expected)
        {
            var result = await _nameProvider.GetOutcomeNameAsync(outcomeId, TestData.Culture).ConfigureAwait(false);
            result.Should().Be(expected);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        }
    }
}
