// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class PlusNameExpressionTests
{
    private const string Specifier = "quarternr";
    private readonly OperandFactory _operandFactory = new OperandFactory();

    [Fact]
    public async Task ZeroIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "0" } };
        var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("0", name);
    }

    [Fact]
    public async Task NegativeZeroIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "-0" } };
        var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("0", name);
    }

    [Fact]
    public async Task PositiveValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "2" } };
        var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("+2", name);
    }

    [Fact]
    public async Task NegativeValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "-3" } };
        var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("-3", name);
    }

    [Fact]
    public async Task PositiveDecimalValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "2.5" } };
        var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("+2.5", name);
    }

    [Fact]
    public async Task NegativeDecimalValueIsHandledCorrectly()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "-3.25" } };
        var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

        var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);

        Assert.Equal("-3.25", name);
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("-0", "0")]
    [InlineData("+0.25", "+0.25")]
    [InlineData("+0.5", "+0.5")]
    [InlineData("+0.75", "+0.75")]
    [InlineData("+1", "+1")]
    [InlineData("0.75", "+0.75")]
    [InlineData("1", "+1")]
    [InlineData("-0.75", "-0.75")]
    [InlineData("-1", "-1")]
    [InlineData("-1.75", "-1.75")]
    public void DecimalToString(string input, string expectedResult)
    {
        var nbr = decimal.Parse(input, CultureInfo.InvariantCulture);

        Assert.Equal(expectedResult, SdkInfo.DecimalToStringWithSign(nbr));
    }
}
