/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class NameExpressionHelperTests
    {
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingOpeningBracketsCauseException()
        {
            var expression = "$competitor1}";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingClosingBracketsCauseException()
        {
            var expression = "{$competitor1";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void NoBracketsCauseException()
        {
            var expression = "$competitor1";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ToShortExpressionCausesException()
        {
            var expression = "{}";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);
        }

        [TestMethod]
        public void ExpressionWithNoOperatorIsParsed()
        {
            var expression = "{reply_nr}";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);

            Assert.IsNull(@operator, "Value of operator is not correct");
            Assert.AreEqual("reply_nr", operand, "Value of operator is not correct");
        }

        [TestMethod]
        public void ExpressionWithOrdinalOperatorIsParsed()
        {
            var expression = "{!periodNumber}";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);

            Assert.AreEqual("!", @operator, "Value of operator is not correct");
            Assert.AreEqual("periodNumber", operand, "Value of operator is not correct");
        }

        [TestMethod]
        public void ExpressionWithPlusOperatorIsParsed()
        {
            var expression = "{+score}";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);

            Assert.AreEqual("+", @operator, "Value of operator is not correct");
            Assert.AreEqual("score", operand, "Value of operator is not correct");
        }

        [TestMethod]
        public void ExpressionWithMinusOperatorIsParsed()
        {
            var expression = "{-corners}";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);

            Assert.AreEqual("-", @operator, "Value of operator is not correct");
            Assert.AreEqual("corners", operand, "Value of operator is not correct");
        }

        [TestMethod]
        public void ExpressionWithEntityNameOperatorIsParsed()
        {
            var expression = "{$competitor1}";
            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);

            Assert.AreEqual("$", @operator, "Value of operator is not correct");
            Assert.AreEqual("competitor1", operand, "Value of operator is not correct");
        }

        [TestMethod]
        public void ExpressionWithPlayerProfileOperatorIsParse()
        {
            var expression = "{%player}";

            string @operator;
            string operand;

            NameExpressionHelper.ParseExpression(expression, out @operator, out operand);
            Assert.AreEqual("%", @operator, "Value of the operator is not correct");
            Assert.AreEqual("player", operand, "Value of operand is not correct");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingOpeningBracketInDescriptorCausesException()
        {
            var descriptor = "Competitor $competitor1} to {score} points";
            string format;
            NameExpressionHelper.ParseDescriptor(descriptor, out format);

        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingClosingBracketInDescriptorCausesException()
        {
            var descriptor = "Competitor $competitor1} to {score points";
            string format;
            NameExpressionHelper.ParseDescriptor(descriptor, out format);
        }



        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingOpeningBracketOnBeginningInDescriptorCausesException()
        {
            var descriptor = "$competitor1} to {score} points";
            string format;
            NameExpressionHelper.ParseDescriptor(descriptor, out format);

        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void MissingClosingBracketOnEndInDescriptorCausesException()
        {
            var descriptor = "{$competitor1} to {score";
            string format;
            NameExpressionHelper.ParseDescriptor(descriptor, out format);

        }

        [TestMethod]
        public void SingleExpressionDescriptorIsParsed()
        {
            var descriptor = "{$competitor1}";

            string format;
            var expressions = NameExpressionHelper.ParseDescriptor(descriptor, out format);

            Assert.AreEqual(1, expressions.Count(), "The number of expressions is not correct");
            Assert.AreEqual("{0}", format, "Value of format is not correct");
            Assert.AreEqual(descriptor, expressions.First(), "The expression is not correct");
        }

        [TestMethod]
        public void DoubleExpressionDescriptorIsParsed()
        {
            var descriptor = "{$competitor1} to {score}";

            string format;
            var expressions = NameExpressionHelper.ParseDescriptor(descriptor, out format);

            Assert.AreEqual(2, expressions.Count(), "The number of expressions is not correct");
            Assert.AreEqual("{0} to {1}", format, "Value of format is not correct");
            Assert.AreEqual("{$competitor1}", expressions.First(), "The first expression is not correct");
            Assert.AreEqual("{score}", expressions.Last(), "The second expression is not correct");
        }
    }
}
