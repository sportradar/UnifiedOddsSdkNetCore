/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class PlayerProfileExpressionTests
    {
        private readonly IOperandFactory _operandFactory = new OperandFactory();
        private readonly IProfileCache _profileCache;

        public PlayerProfileExpressionTests(ITestOutputHelper outputHelper)
        {
            var cacheManager = new CacheManager();
            var dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);
            _profileCache = new ProfileCache(new MemoryCache("test"), dataRouterManager, cacheManager);
        }

        [Fact]
        public async Task PlayerProfileExpressionReturnsCorrectValue()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_profileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));

            var result = await expression.BuildNameAsync(TestData.Culture);
            var profile = await _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new[] { TestData.Culture });

            Assert.Equal(profile.GetName(TestData.Culture), result);
        }
    }
}
