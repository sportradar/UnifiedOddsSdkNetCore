// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class OperandFactoryTests
{
    private readonly IOperandFactory _factory = new OperandFactory();

    private const string Specifier = "total";

    private static IReadOnlyDictionary<string, string> BuildSpecifiers(string name, string value)
    {
        return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            {name, value}
        });
    }

    [Theory]
    [InlineData("total+1)")]
    [InlineData("(total+1")]
    [InlineData("(total1")]
    [InlineData("(total+1+2")]
    [InlineData("(total-1-2")]
    [InlineData("(total+1-2")]
    [InlineData("(total+1.1)")]
    [InlineData("(total+abc)")]
    [InlineData("(total+1+2)")]
    [InlineData("(total-1-2)")]
    [InlineData("(total*1)")]
    public void InvalidFormatThrowsFormatException(string value)
    {
        Action action = () => _factory.BuildOperand(BuildSpecifiers(Specifier, "1"), value);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public async Task ResultOfAdditionIsCorrect()
    {
        var operand = _factory.BuildOperand(BuildSpecifiers(Specifier, "1"), $"({Specifier}+1)");

        Assert.Equal(2, await operand.GetIntValue());
    }

    [Fact]
    public async Task ResultSubtractionIsCorrect()
    {
        var operand = _factory.BuildOperand(BuildSpecifiers(Specifier, "2"), $"({Specifier}-1)");

        Assert.Equal(1, await operand.GetIntValue());
    }

    [Fact]
    public async Task WhenNoMathOperationThenReturnSimpleOperand()
    {
        var operand = _factory.BuildOperand(BuildSpecifiers(Specifier, "1"), $"{Specifier}");

        Assert.IsAssignableFrom<SimpleOperand>(operand);
        Assert.Equal(1, await operand.GetIntValue());
    }
}
