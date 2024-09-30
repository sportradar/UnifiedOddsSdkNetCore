// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class MinusNameExpressionTests
{
    private const string Specifier = "quarternr";

    private readonly OperandFactory _operandFactory = new OperandFactory();

    [Fact]
    public async Task ZeroIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "0" } };
        var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("0", name);
    }

    [Fact]
    public async Task NegativeZeroIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "-0" } };
        var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("0", name);
    }

    [Fact]
    public async Task PositiveValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "2" } };
        var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("-2", name);
    }

    [Fact]
    public async Task NegativeValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "-3" } };
        var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("+3", name);
    }

    [Fact]
    public async Task PositiveDecimalValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "2.5" } };
        var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("-2.5", name);
    }

    [Fact]
    public async Task NegativeDecimalValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "-3.25" } };
        var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("+3.25", name);
    }
}
