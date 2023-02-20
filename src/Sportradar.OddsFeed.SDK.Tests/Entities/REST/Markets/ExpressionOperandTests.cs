/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class ExpressionOperandTests
    {
        private const string SpecifierName = "score";

        private static IReadOnlyDictionary<string, string> GetSpecifiers(string name, string value)
        {
            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { name, value }
            });
        }

        [Fact]
        public async Task Correct_int_value_for_addition_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "2"),
                SpecifierName,
                SimpleMathOperation.Add,
                1);

            Assert.Equal(3, await operand.GetIntValue());
        }

        [Fact]
        public async Task Correct_int_value_for_subtraction_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "2"),
                SpecifierName,
                SimpleMathOperation.Subtract,
                1);

            Assert.Equal(1, await operand.GetIntValue());
        }

        [Fact]
        public async Task Correct_decimal_value_for_addition_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "1.5"),
                SpecifierName,
                SimpleMathOperation.Add,
                1);

            Assert.Equal((decimal)2.5, await operand.GetDecimalValue());
        }

        [Fact]
        public async Task Correct_decimal_value_for_subtraction_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "1.5"),
                SpecifierName,
                SimpleMathOperation.Subtract,
                1);

            Assert.Equal((decimal)0.5, await operand.GetDecimalValue());
        }

        [Fact]
        public async Task Get_string_value_causes_exception()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "1.5"),
                SpecifierName,
                SimpleMathOperation.Subtract,
                1);

            Func<Task<string>> action = async () => await operand.GetStringValue();
            await action.Should().ThrowAsync<NotSupportedException>();
        }

        [Fact]
        public async Task Get_int_value_for_decimal_value_throws()
        {
            var operand = new ExpressionOperand(
               GetSpecifiers(SpecifierName, "1.5"),
               SpecifierName,
               SimpleMathOperation.Add,
               1);

            Func<Task<int>> action = async () => await operand.GetIntValue();
            await action.Should().ThrowAsync<NameExpressionException>();
        }

        [Fact]
        public async Task Get_int_value_with_missing_specifier_throws()
        {
            var operand = new ExpressionOperand(
               GetSpecifiers(SpecifierName, "1.5"),
               "missing",
               SimpleMathOperation.Add,
               1);

            Func<Task<int>> action = async () => await operand.GetIntValue();
            await action.Should().ThrowAsync<NameExpressionException>();
        }
    }
}
