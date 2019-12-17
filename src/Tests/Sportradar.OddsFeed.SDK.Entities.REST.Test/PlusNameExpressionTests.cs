/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class PlusNameExpressionTests
    {
        private const string Specifier = "quarternr";

        private readonly IOperandFactory _operandFactory = new OperandFactory();

        [TestMethod]
        public void ZeroIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "0" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).Result;
            Assert.AreEqual("0", name, "Value of name is not correct");
        }

        [TestMethod]
        public void NegativeZeroIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-0" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).Result;
            Assert.AreEqual("0", name, "Value of name is not correct");
        }

        [TestMethod]
        public void PositiveValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string,string>(new Dictionary<string, string> { { Specifier, "2" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).Result;
            Assert.AreEqual("+2", name, "Value of name is not correct");
        }

        [TestMethod]
        public void NegativeValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-3" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).Result;
            Assert.AreEqual("-3", name, "Value of name is not correct");
        }

        [TestMethod]
        public void PositiveDecimalValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "2.5" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).Result;
            Assert.AreEqual("+2.5", name, "Value of name is not correct");
        }

        [TestMethod]
        public void NegativeDecimalValueIsHandledCorrectly()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string> { { Specifier, "-3.25" } });
            var expression = new PlusNameExpression(_operandFactory.BuildOperand(specifiers, Specifier));

            var name = expression.BuildNameAsync(CultureInfo.InvariantCulture).Result;
            Assert.AreEqual("-3.25", name, "Value of name is not correct");
        }

        [TestMethod]
        public void DecimalToStringTest()
        {
            var nbrString = "0";
            var nbr = decimal.Parse(nbrString);
            Assert.AreEqual("0", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: 0, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "-0";
            nbr = decimal.Parse(nbrString);
            Assert.AreEqual("0", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: 0, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "+0.25";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual("+0.25", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: +0.25, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "+0.5";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual("+0.5", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: +0.5, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "+0.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual("+0.75", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: +0.75, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "+1";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual("+1", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: +1, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "0.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual("+0.75", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: +0.75, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "1";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual("+1", SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: +1, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");

            nbrString = "-0.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual(nbrString, SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: {nbrString}, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");
            nbrString = "-1";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual(nbrString, SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: {nbrString}, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");
            nbrString = "-1.75";
            nbr = decimal.Parse(nbrString, CultureInfo.InvariantCulture);
            Assert.AreEqual(nbrString, SdkInfo.DecimalToStringWithSign(nbr), $"Expecting: {nbrString}, Received: {SdkInfo.DecimalToStringWithSign(nbr)}.");
        }
    }
}
