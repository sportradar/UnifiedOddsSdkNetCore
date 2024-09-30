// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class SimpleOperandTests
{
    private const string Specifier = "score";

    [Fact]
    public async Task CorrectIntValueForIntIsReturned()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "2" } };

        var operand = new SimpleOperand(specifiers, Specifier);
        Assert.Equal(2, await operand.GetIntValue());
    }

    [Fact]
    public async Task GetIntValueForDecimalValueFails()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1.5" } };
        var operand = new SimpleOperand(specifiers, Specifier);

        var action = () => operand.GetIntValue();
        await action.Should().ThrowAsync<NameExpressionException>();
    }

    [Fact]
    public async Task CorrectDecimalValueForIntValueIsReturned()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1" } };

        var operand = new SimpleOperand(specifiers, Specifier);
        Assert.Equal(1, await operand.GetDecimalValue());
    }

    [Fact]
    public async Task CorrectDecimalValueForDecimalValueIsReturned()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1.5" } };

        var operand = new SimpleOperand(specifiers, Specifier);
        Assert.Equal((decimal)1.5, await operand.GetDecimalValue());
    }

    [Fact]
    public async Task CorrectStringValueForIntValueIsReturned()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1" } };

        var operand = new SimpleOperand(specifiers, Specifier);
        Assert.Equal("1", await operand.GetStringValue());
    }

    [Fact]
    public async Task CorrectStringValueForDecimalValueIsReturned()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1.5" } };

        var operand = new SimpleOperand(specifiers, Specifier);
        Assert.Equal("1.5", await operand.GetStringValue());
    }

    [Fact]
    public async Task WrongSpecifierOnGetIntValueFails()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1.5" } };
        var operand = new SimpleOperand(specifiers, "missing");

        var action = () => operand.GetIntValue();
        await action.Should().ThrowAsync<NameExpressionException>();
    }

    [Fact]
    public async Task WrongSpecifierOnGetDecimalValueFails()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1.5" } };
        var operand = new SimpleOperand(specifiers, "missing");

        var action = () => operand.GetDecimalValue();
        await action.Should().ThrowAsync<NameExpressionException>();
    }

    [Fact]
    public async Task WrongSpecifierOnGetStringValueFails()
    {
        var specifiers = new Dictionary<string, string> { { Specifier, "1.5" } };
        var operand = new SimpleOperand(specifiers, "missing");

        var action = () => operand.GetStringValue();
        await action.Should().ThrowAsync<NameExpressionException>();
    }
}
