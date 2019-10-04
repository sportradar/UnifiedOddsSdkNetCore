/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class PlayerProfileExpressionTest
    {
        private readonly IOperandFactory _operandFactory = new OperandFactory();

        private IProfileCache _profileCache;
        private CacheManager _cacheManager;
        private TestDataRouterManager _dataRouterManager;

        [TestInitialize]
        public void Init()
        {
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);
            _profileCache = new ProfileCache(new MemoryCache("test"), _dataRouterManager, _cacheManager);
        }

        [TestMethod]
        public void player_profile_expression_returns_correct_value()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = new PlayerProfileExpression(_profileCache, _operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "player"));

            var result = expression.BuildNameAsync(TestData.Culture).Result;
            Assert.AreEqual(
                _profileCache.GetPlayerProfileAsync(URN.Parse("sr:player:1"), new [] { TestData.Culture }).Result.GetName(TestData.Culture),
                result, "The result is not correct");
        }
    }
}
