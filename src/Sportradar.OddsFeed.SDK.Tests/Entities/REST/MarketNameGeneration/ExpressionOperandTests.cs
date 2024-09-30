// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class ExpressionOperandTests
{
    private const string SpecifierName = "score";

    private static IReadOnlyDictionary<string, string> GetSpecifiers(string name, string value)
    {
        return new Dictionary<string, string> { { name, value } };
    }

    [Fact]
    public async Task CorrectIntValueForAdditionIsReturned()
    {
        var operand = new ExpressionOperand(
            GetSpecifiers(SpecifierName, "2"),
            SpecifierName,
            SimpleMathOperation.Add,
            1);

        Assert.Equal(3, await operand.GetIntValue());
    }

    [Fact]
    public async Task CorrectIntValueForSubtractionIsReturned()
    {
        var operand = new ExpressionOperand(
            GetSpecifiers(SpecifierName, "2"),
            SpecifierName,
            SimpleMathOperation.Subtract,
            1);

        Assert.Equal(1, await operand.GetIntValue());
    }

    [Fact]
    public async Task CorrectDecimalValueForAdditionIsReturned()
    {
        var operand = new ExpressionOperand(
            GetSpecifiers(SpecifierName, "1.5"),
            SpecifierName,
            SimpleMathOperation.Add,
            1);

        Assert.Equal((decimal)2.5, await operand.GetDecimalValue());
    }

    [Fact]
    public async Task CorrectDecimalValueForSubtractionIsReturned()
    {
        var operand = new ExpressionOperand(
            GetSpecifiers(SpecifierName, "1.5"),
            SpecifierName,
            SimpleMathOperation.Subtract,
            1);

        Assert.Equal((decimal)0.5, await operand.GetDecimalValue());
    }

    [Fact]
    public async Task GetStringValueThenThrows()
    {
        var operand = new ExpressionOperand(
            GetSpecifiers(SpecifierName, "1.5"),
            SpecifierName,
            SimpleMathOperation.Subtract,
            1);

        var action = async () => await operand.GetStringValue();
        await action.Should().ThrowAsync<NameExpressionException>();
    }

    [Fact]
    public async Task GetIntValueForDecimalValueThrows()
    {
        var operand = new ExpressionOperand(
            GetSpecifiers(SpecifierName, "1.5"),
            SpecifierName,
            SimpleMathOperation.Add,
            1);

        var action = async () => await operand.GetIntValue();
        await action.Should().ThrowAsync<NameExpressionException>();
    }

    [Fact]
    public async Task GetIntValueWithMissingSpecifierThrows()
    {
        var operand = new ExpressionOperand(
            GetSpecifiers(SpecifierName, "1.5"),
            "missing",
            SimpleMathOperation.Add,
            1);

        var action = async () => await operand.GetIntValue();
        await action.Should().ThrowAsync<NameExpressionException>();
    }
}
