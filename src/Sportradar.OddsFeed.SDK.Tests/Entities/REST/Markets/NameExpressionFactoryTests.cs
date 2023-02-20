/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class NameExpressionFactoryTests
    {
        private readonly ISportEvent _sportEvent = new Mock<ISportEvent>().Object;
        private readonly NameExpressionFactory _factory = new NameExpressionFactory(new OperandFactory(), new Mock<IProfileCache>().Object);

        [Fact]
        public void Cardinal_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = _factory.BuildExpression(_sportEvent, specifiers, null, "player");

            expression.Should().NotBeNull();
            expression.Should().BeOfType<CardinalNameExpression>();
        }

        [Fact]
        public void Plus_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = _factory.BuildExpression(_sportEvent, specifiers, "+", "player");

            expression.Should().NotBeNull();
            expression.Should().BeOfType<PlusNameExpression>();
        }

        [Fact]
        public void Minus_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = _factory.BuildExpression(_sportEvent, specifiers, "-", "player");

            expression.Should().NotBeNull();
            expression.Should().BeOfType<MinusNameExpression>();
        }

        [Fact]
        public void Entity_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = _factory.BuildExpression(_sportEvent, specifiers, "$", "competitor1");

            expression.Should().NotBeNull();
            expression.Should().BeOfType<EntityNameExpression>();
        }

        [Fact]
        public void Ordinal_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = _factory.BuildExpression(_sportEvent, specifiers, "!", "player");

            expression.Should().NotBeNull();
            expression.Should().BeOfType<OrdinalNameExpression>();
        }

        [Fact]
        public void Player_profile_name_expression_is_build()
        {
            var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
            var expression = _factory.BuildExpression(_sportEvent, specifiers, "%", "player");

            expression.Should().NotBeNull();
            expression.Should().BeOfType<PlayerProfileExpression>();
        }
    }
}
