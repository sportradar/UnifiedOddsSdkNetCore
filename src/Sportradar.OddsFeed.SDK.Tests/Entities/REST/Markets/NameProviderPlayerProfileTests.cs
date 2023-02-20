/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class NameProviderPlayerProfileTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly MemoryCache _memoryCache;
        private readonly TestDataRouterManager _dataRouterManager;
        private readonly IProfileCache _profileCache;
        private INameProvider _nameProvider;

        public NameProviderPlayerProfileTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _memoryCache = new MemoryCache("cache");
            var cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(cacheManager, _outputHelper);
            _profileCache = new ProfileCache(_memoryCache, _dataRouterManager, cacheManager);

            _nameProvider = new NameProvider(
                new Mock<IMarketCacheProvider>().Object,
                _profileCache,
                new Mock<INameExpressionFactory>().Object,
                new Mock<ISportEvent>().Object,
                1,
                null,
                ExceptionHandlingStrategy.THROW);
        }

        [Fact]
        public async Task Id_for_one_player_profiles_gets_correct_name()
        {
            var name = await _nameProvider.GetOutcomeNameAsync("sr:player:2", new CultureInfo("en"));
            Assert.Equal("Cole, Ashley", name);
        }

        [Fact]
        public async Task Composite_id_for_two_player_profiles_gets_correct_name()
        {
            var name = await _nameProvider.GetOutcomeNameAsync("sr:player:1,sr:player:2", new CultureInfo("en"));
            Assert.Equal("van Persie, Robin,Cole, Ashley", name);
        }

        [Fact]
        public async Task Player_profile_is_called_only_once()
        {
            const string callType = "GetPlayerProfileAsync";
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType));
            var name = await _nameProvider.GetOutcomeNameAsync("sr:player:2", TestData.Cultures.First());
            Assert.Equal("Cole, Ashley", name);
            Assert.Single(_memoryCache);
            Assert.Equal(1, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public async Task Competitor_and_player_profile_is_called_correctly()
        {
            const string callType1 = "GetPlayerProfileAsync";
            const string callType2 = "GetCompetitorAsync";
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType1));
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType2));
            var name = await _nameProvider.GetOutcomeNameAsync("sr:player:1,sr:competitor:1", TestData.Cultures.First());
            Assert.Equal("van Persie, Robin,Queens Park Rangers", name);
            Assert.Equal(35, _memoryCache.Count());
            Assert.Equal(1, _dataRouterManager.GetCallCount(callType1));
            Assert.Equal(1, _dataRouterManager.GetCallCount(callType2));
        }

        [Fact]
        public async Task Player_profile_is_called_only_once_per_culture()
        {
            const string callType = "GetPlayerProfileAsync";
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType));
            foreach (var cultureInfo in TestData.Cultures)
            {
                var name = await _nameProvider.GetOutcomeNameAsync("sr:player:2", cultureInfo);
                Assert.Equal("Cole, Ashley", name);
            }
            Assert.Single(_memoryCache);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public async Task Competitor_profile_is_called_only_once()
        {
            const string callType = "GetCompetitorAsync";
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType));
            var name = await _nameProvider.GetOutcomeNameAsync("sr:competitor:1", TestData.Cultures.First());
            Assert.Equal("Queens Park Rangers", name);
            Assert.Equal(35, _memoryCache.Count());
            Assert.Equal(1, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public async Task Competitor_profile_is_called_only_once_per_culture()
        {
            const string callType = "GetCompetitorAsync";
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType));
            foreach (var cultureInfo in TestData.Cultures)
            {
                var name = await _nameProvider.GetOutcomeNameAsync("sr:competitor:1", cultureInfo);
                Assert.Equal("Queens Park Rangers", name);
            }
            Assert.Equal(35, _memoryCache.Count());
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public async Task Competitor_profile_is_called_when_wanted_player_profile()
        {
            var match = new TestSportEntityFactory(_outputHelper).BuildSportEvent<IMatch>(URN.Parse("sr:match:1"), URN.Parse("sr:sport:5"), TestData.Cultures.ToList(), ExceptionHandlingStrategy.THROW);

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
            Assert.Empty(_memoryCache);
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType1));
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType2));
            var name = await _nameProvider.GetOutcomeNameAsync("sr:player:1", TestData.Cultures.First());
            Assert.Equal("van Persie, Robin", name);
            Assert.Equal(61 + 2, _memoryCache.Count());
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType1));
            Assert.Equal(2, _dataRouterManager.GetCallCount(callType2));
        }
    }
}
