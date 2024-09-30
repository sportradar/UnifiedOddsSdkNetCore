// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class NameExpressionFactoryTests
{
    private readonly ISportEvent _sportEvent = new Mock<IMatch>().Object;
    private readonly NameExpressionFactory _factory = new NameExpressionFactory(new OperandFactory(), new Mock<IProfileCache>().Object);

    [Fact]
    public void CardinalNameExpressionIsBuild()
    {
        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = _factory.BuildExpression(_sportEvent, specifiers, null, "player");

        expression.Should().NotBeNull();
        expression.Should().BeOfType<CardinalNameExpression>();
    }

    [Fact]
    public void PlusNameExpressionIsBuild()
    {
        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = _factory.BuildExpression(_sportEvent, specifiers, "+", "player");

        expression.Should().NotBeNull();
        expression.Should().BeOfType<PlusNameExpression>();
    }

    [Fact]
    public void MinusNameExpressionIsBuild()
    {
        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = _factory.BuildExpression(_sportEvent, specifiers, "-", "player");

        expression.Should().NotBeNull();
        expression.Should().BeOfType<MinusNameExpression>();
    }

    [Fact]
    public void EntityNameExpressionIsBuild()
    {
        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = _factory.BuildExpression(_sportEvent, specifiers, "$", "competitor1");

        expression.Should().NotBeNull();
        expression.Should().BeOfType<EntityNameExpression>();
    }

    [Fact]
    public void OrdinalNameExpressionIsBuild()
    {
        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = _factory.BuildExpression(_sportEvent, specifiers, "!", "player");

        expression.Should().NotBeNull();
        expression.Should().BeOfType<OrdinalNameExpression>();
    }

    [Fact]
    public void PlayerProfileNameExpressionIsBuild()
    {
        var specifiers = new Dictionary<string, string> { { "player", "sr:player:1" } };
        var expression = _factory.BuildExpression(_sportEvent, specifiers, "%", "player");

        expression.Should().NotBeNull();
        expression.Should().BeOfType<PlayerProfileExpression>();
    }
}
