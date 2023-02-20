/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class NameExpressionHelperTests
    {
        [Fact]
        public void MissingOpeningBracketsCauseException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("$competitor1}", out _, out _);
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void MissingClosingBracketsCauseException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("{$competitor1", out _, out _);
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void NoBracketsCauseException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("$competitor1", out _, out _);
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void ToShortExpressionCausesException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("{}", out _, out _);
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void MissingOpeningBracketInDescriptorCausesException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("Competitor $competitor1} to {score} points", out _, out _);
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void MissingClosingBracketInDescriptorCausesException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("Competitor $competitor1} to {score points", out _, out _);
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void MissingOpeningBracketOnBeginningInDescriptorCausesException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("$competitor1} to {score} points", out _, out _);
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void MissingClosingBracketOnEndInDescriptorCausesException()
        {
            Action action = () => NameExpressionHelper.ParseExpression("{$competitor1} to {score", out _, out _);
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
        public void ExpressionWithPlayerProfileOperatorIsParse()
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
}
