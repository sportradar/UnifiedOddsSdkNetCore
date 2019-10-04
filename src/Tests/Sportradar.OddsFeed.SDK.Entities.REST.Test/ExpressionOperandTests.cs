/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class ExpressionOperandTests
    {
        private const string SpecifierName = "score";

        private IReadOnlyDictionary<string, string> GetSpecifiers(string name, string value)
        {
            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { name, value }
            });
        }

        [TestMethod]
        public void correct_int_value_for_addition_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "2"),
                SpecifierName,
                SimpleMathOperation.ADD,
                1);

            Assert.AreEqual(3, operand.GetIntValue().Result, "Int value returned by operand is not correct");
        }

        [TestMethod]
        public void correct_int_value_for_subtraction_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "2"),
                SpecifierName,
                SimpleMathOperation.SUBTRACT,
                1);

            Assert.AreEqual(1, operand.GetIntValue().Result, "Int value returned by operand is not correct");
        }

        [TestMethod]
        public void correct_decimal_value_for_addition_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "1.5"),
                SpecifierName,
                SimpleMathOperation.SUBTRACT,
                1);

            Assert.AreEqual((decimal)0.5, operand.GetDecimalValue().Result, "Decimal value returned by operand is not correct");
        }

        [TestMethod]
        public void correct_decimal_value_for_subtraction_is_returned()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "1.5"),
                SpecifierName,
                SimpleMathOperation.SUBTRACT,
                1);

            Assert.AreEqual((decimal)0.5, operand.GetDecimalValue().Result, "Decimal value returned by operand is not correct");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void get_string_value_causes_exception()
        {
            var operand = new ExpressionOperand(
                GetSpecifiers(SpecifierName, "1.5"),
                SpecifierName,
                SimpleMathOperation.SUBTRACT,
                1);

            try
            {
                var x = operand.GetStringValue().Result;
                Assert.IsNotNull(x);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions.First();
            }
        }

        [TestMethod]
        public void get_int_value_for_decimal_value_throws()
        {
            var operand = new ExpressionOperand(
               GetSpecifiers(SpecifierName, "1.5"),
               SpecifierName,
               SimpleMathOperation.ADD,
               1);

            try
            {
                var x = operand.GetIntValue().Result;
                Assert.IsNotNull(x);
            }
            catch (NameExpressionException)
            {
                return;
            }
            Assert.Fail("A NameExpressionException should be thrown");
        }

        [TestMethod]
        public void get_int_value_with_missing_specifier_throws()
        {
            var operand = new ExpressionOperand(
               GetSpecifiers(SpecifierName, "1.5"),
               "missing",
               SimpleMathOperation.ADD,
               1);

            try
            {
                var x = operand.GetIntValue().Result;
                Assert.IsNotNull(x);
            }
            catch (NameExpressionException)
            {
                return;
            }
            Assert.Fail("A NameExpressionException should be thrown");
        }
    }
}