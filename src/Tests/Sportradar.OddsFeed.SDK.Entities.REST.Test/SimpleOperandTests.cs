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
    public class SimpleOperandTests
    {
        private const string Specifier = "score";

        [TestMethod]
        public void correct_int_value_for_int_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "2" }
            });

            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.AreEqual(2, operand.GetIntValue().Result, "Int value returned by the operand is not correct");
        }

        [TestMethod]
        [ExpectedException(typeof(NameExpressionException))]
        public void get_int_value_for_decimal_value_fails()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);
            try
            {
                var x = operand.GetIntValue().Result;
                Assert.IsNotNull(x);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions.First();
            }
        }

        [TestMethod]
        public void correct_decimal_value_for_int_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1" }
            });

            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.AreEqual(1, operand.GetDecimalValue().Result, "Decimal value returned by the operand is not correct");
        }

        [TestMethod]
        public void correct_decimal_value_for_decimal_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.AreEqual((decimal)1.5, operand.GetDecimalValue().Result, "Decimal value returned by the operand is not correct");
        }

        [TestMethod]
        public void correct_string_value_for_int_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.AreEqual("1", operand.GetStringValue().Result, "Decimal value returned by the operand is not correct");
        }

        [TestMethod]
        public void correct_string_value_for_decimal_value_is_returned()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, Specifier);
            Assert.AreEqual("1.5", operand.GetStringValue().Result, "Decimal value returned by the operand is not correct");
        }

        [TestMethod]
        [ExpectedException(typeof(NameExpressionException))]
        public void wrong_specifier_on_get_int_value_fails()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, "missing");
            try
            {
                var x = operand.GetIntValue().Result;
                Assert.IsNotNull(x);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions.First();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NameExpressionException))]
        public void wrong_specifier_on_get_decimal_value_fails()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, "missing");
            try
            {
                var x = operand.GetDecimalValue().Result;
                Assert.IsNotNull(x);
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions.First();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NameExpressionException))]
        public void wrong_specifier_on_get_string_value_failles()
        {
            var specifiers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { Specifier, "1.5" }
            });
            var operand = new SimpleOperand(specifiers, "missing");
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
    }
}
