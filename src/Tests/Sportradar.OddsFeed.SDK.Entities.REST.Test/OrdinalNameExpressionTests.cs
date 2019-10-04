/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class OrdinalNameExpressionTests
    {
        private readonly IOperandFactory _operandFactory = new OperandFactory();

        [TestMethod]
        public void OrdinalNameExpressionTest()
        {
            var specifiers = new Dictionary<string, string>{ {"reply_nr", "1"} };
            var expression = new OrdinalNameExpression(_operandFactory.BuildOperand(new ReadOnlyDictionary<string, string>(specifiers), "reply_nr"));

            var result = expression.BuildNameAsync(new CultureInfo("en")).Result;
            Assert.AreEqual("1st", result, "The result is not correct");
        }
    }
}
    