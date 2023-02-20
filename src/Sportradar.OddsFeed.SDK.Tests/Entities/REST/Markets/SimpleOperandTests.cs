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
    public class SimpleOperandTests
    {
        private const string Specifier = "score";

        [Fact]
        public async Task Correct_int_value_for_int_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "2" }
            });

            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.Equal(2, await operand.GetIntValue());
        }

        [Fact]
        public async Task Get_int_value_for_decimal_value_fails()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);

            Func<Task<int>> action = () => operand.GetIntValue();
            await action.Should().ThrowAsync<NameExpressionException>();
        }

        [Fact]
        public async Task Correct_decimal_value_for_int_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1" }
            });

            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.Equal(1, await operand.GetDecimalValue());
        }

        [Fact]
        public async Task Correct_decimal_value_for_decimal_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.Equal((decimal)1.5, await operand.GetDecimalValue());
        }

        [Fact]
        public async Task Correct_string_value_for_int_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.Equal("1", await operand.GetStringValue());
        }

        [Fact]
        public async Task Correct_string_value_for_decimal_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.Equal("1.5", await operand.GetStringValue());
        }

        [Fact]
        public async Task Wrong_specifier_on_get_int_value_fails()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, "missing");

            Func<Task<int>> action = () => operand.GetIntValue();
            await action.Should().ThrowAsync<NameExpressionException>();
        }

        [Fact]
        public async Task Wrong_specifier_on_get_decimal_value_fails()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, "missing");

            Func<Task<decimal>> action = () => operand.GetDecimalValue();
            await action.Should().ThrowAsync<NameExpressionException>();
        }

        [Fact]
        public async Task Wrong_specifier_on_get_string_value_fails()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, "missing");

            Func<Task<string>> action = () => operand.GetStringValue();
            await action.Should().ThrowAsync<NameExpressionException>();
        }
    }
}
