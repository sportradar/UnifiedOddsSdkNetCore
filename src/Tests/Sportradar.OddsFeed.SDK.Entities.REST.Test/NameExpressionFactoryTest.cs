/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class NameExpressionFactoryTest
    {
        private readonly ISportEvent _sportEvent = new Mock<ISportEvent>().Object;

        private readonly NameExpressionFactory _factory = new NameExpressionFactory(new OperandFactory(), new Mock<IProfileCache>().Object);

        [TestMethod]
        public void cardinal_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };

            var expression = _factory.BuildExpression(_sportEvent, specifiers, null, "player");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(CardinalNameExpression));
        }

        [TestMethod]
        public void plus_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };

            var expression = _factory.BuildExpression(_sportEvent, specifiers, "+", "player");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(PlusNameExpression));
        }

        [TestMethod]
        public void minus_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };

            var expression = _factory.BuildExpression(_sportEvent, specifiers, "-", "player");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(MinusNameExpression));
        }

        [TestMethod]
        public void entity_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };

            var expression = _factory.BuildExpression(_sportEvent, specifiers, "$", "competitor1");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(EntityNameExpression));
        }

        [TestMethod]
        public void ordinal_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };

            var expression = _factory.BuildExpression(_sportEvent, specifiers, "!", "player");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(OrdinalNameExpression));
        }

        [TestMethod]
        public void player_profile_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };

            var expression = _factory.BuildExpression(_sportEvent, specifiers, "%", "player");
            Assert.IsNotNull(expression);
            Assert.IsInstanceOfType(expression, typeof(PlayerProfileExpression));
        }

    }
}
