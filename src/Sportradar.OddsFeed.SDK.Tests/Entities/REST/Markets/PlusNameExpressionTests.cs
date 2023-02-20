/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class PlusNameExpressionTests
    {
        private const string Specifier = "quarternr";

        private readonly IOperandFactory _operandFactory = new OperandFactory();

        [Fact]
        public async Task ZeroIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "0" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);
            Assert.Equal("0", name);
        }

        [Fact]
        public async Task NegativeZeroIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-0" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);
            Assert.Equal("0", name);
        }

        [Fact]
        public async Task PositiveValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "2" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);
            Assert.Equal("+2", name);
        }

        [Fact]
        public async Task NegativeValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-3" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);
            Assert.Equal("-3", name);
        }

        [Fact]
        public async Task PositiveDecimalValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "2.5" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);
            Assert.Equal("+2.5", name);
        }

        [Fact]
        public async Task NegativeDecimalValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-3.25" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = await expression.BuildNameAsync(CultureInfo.InvariantCulture);
            Assert.Equal("-3.25", name);
        }

        [Fact]
        public void DecimalToStringTest()
        {
            var nbrString = "0";
            var nbr = decimal.Parse(nbrString);
            Assert.Equal("0", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "-0";
            nbr = decimal.Parse(nbrString);
            Assert.Equal("0", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "+0.25";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal("+0.25", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "+0.5";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal("+0.5", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "+0.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal("+0.75", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "+1";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal("+1", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "0.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal("+0.75", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "1";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal("+1", SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "-0.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal(nbrString, SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "-1";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal(nbrString, SdkInfo.DecimalToStringWithSign(nbr));

            nbrString = "-1.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.Equal(nbrString, SdkInfo.DecimalToStringWithSign(nbr));
        }
    }
}
