/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class MinusNameExpressionTests
    {
        private const string Specifier = "quarternr";

        private readonly IOperandFactory _operandFactory = new OperandFactory();

        [Fact]
        public void ZeroIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "0" } });
            var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).GetAwaiter().GetResult();
            Assert.Equal("0", name);
        }

        [Fact]
        public void NegativeZeroIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-0" } });
            var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).GetAwaiter().GetResult();
            Assert.Equal("0", name);
        }

        [Fact]
        public void PositiveValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "2" } });
            var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).GetAwaiter().GetResult();
            Assert.Equal("-2", name);
        }

        [Fact]
        public void NegativeValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-3" } });
            var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).GetAwaiter().GetResult();
            Assert.Equal("+3", name);
        }

        [Fact]
        public void PositiveDecimalValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "2.5" } });
            var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).GetAwaiter().GetResult();
            Assert.Equal("-2.5", name);
        }

        [Fact]
        public void NegativeDecimalValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-3.25" } });
            var expression = new MinusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).GetAwaiter().GetResult();
            Assert.Equal("+3.25", name);
        }
    }
}
