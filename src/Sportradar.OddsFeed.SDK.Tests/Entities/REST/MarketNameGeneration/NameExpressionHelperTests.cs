// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class NameExpressionHelperTests
{
    [Fact]
    public void MissingOpeningBracketsThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("$competitor1}", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void MissingClosingBracketsThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("{$competitor1", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void NoBracketsThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("$competitor1", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void ToShortExpressionThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("{}", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void MissingOpeningBracketInDescriptorThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("Competitor $competitor1} to {score} points", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void MissingClosingBracketInDescriptorThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("Competitor {$competitor1} to {score points", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void MissingOpeningBracketOnBeginningInDescriptorThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("$competitor1} to {score} points", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void MissingClosingBracketOnEndInDescriptorThrows()
    {
        var action = () => NameExpressionHelper.ParseExpression("{$competitor1} to {score", out _, out _);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void ExpressionWithNoOperatorIsParsed()
    {
        NameExpressionHelper.ParseExpression("{reply_nr}", out var @operator, out var operand);

        Assert.Null(@operator);
        Assert.Equal("reply_nr", operand);
    }

    [Fact]
    public void ExpressionWithOrdinalOperatorIsParsed()
    {
        NameExpressionHelper.ParseExpression("{!periodNumber}", out var @operator, out var operand);

        Assert.Equal("!", @operator);
        Assert.Equal("periodNumber", operand);
    }

    [Fact]
    public void ExpressionWithPlusOperatorIsParsed()
    {
        NameExpressionHelper.ParseExpression("{+score}", out var @operator, out var operand);

        Assert.Equal("+", @operator);
        Assert.Equal("score", operand);
    }

    [Fact]
    public void ExpressionWithMinusOperatorIsParsed()
    {
        NameExpressionHelper.ParseExpression("{-corners}", out var @operator, out var operand);

        Assert.Equal("-", @operator);
        Assert.Equal("corners", operand);
    }

    [Fact]
    public void ExpressionWithEntityNameOperatorIsParsed()
    {
        NameExpressionHelper.ParseExpression("{$competitor1}", out var @operator, out var operand);

        Assert.Equal("$", @operator);
        Assert.Equal("competitor1", operand);
    }

    [Fact]
    public void ExpressionWithPlayerProfileOperatorIsParsed()
    {
        NameExpressionHelper.ParseExpression("{%player}", out var @operator, out var operand);

        Assert.Equal("%", @operator);
        Assert.Equal("player", operand);
    }

    [Fact]
    public void SingleExpressionDescriptorIsParsed()
    {
        const string descriptor = "{$competitor1}";

        var expressions = NameExpressionHelper.ParseDescriptor(descriptor, out var format);

        Assert.Single(expressions);
        Assert.Equal("{0}", format);
        Assert.Equal(descriptor, expressions.First());
    }

    [Fact]
    public void DoubleExpressionDescriptorIsParsed()
    {
        var expressions = NameExpressionHelper.ParseDescriptor("{$competitor1} to {score}", out var format);

        Assert.Equal(2, expressions.Count);
        Assert.Equal("{0} to {1}", format);
        Assert.Equal("{$competitor1}", expressions.First());
        Assert.Equal("{score}", expressions.Last());
    }
}
