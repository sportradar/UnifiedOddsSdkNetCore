/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class OperandFactoryTests
    {
        private readonly IOperandFactory _factory = new OperandFactory();

        private const string Specifier = "total";

        private static IReadOnlyDictionary<string, string> BuildSpecifiers(string name, string value)
        {
            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                {name, value}
            });
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void missing_left_parenthesis_throws()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                "total+1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void missing_right_parenthesis_throws()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                "(total+1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void missing_operator_parenthesis_throws()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                "(total1");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void two_operands_throws()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                "(total+1+2");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void two_operands_throws1()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                "(total-1-2");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void two_operands_throws2()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                "(total+1-2");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void decimal_static_value_throws()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                $"({Specifier}+1.1)");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void string_static_value_throws()
        {
            _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                $"({Specifier}+abc)");
        }


        [TestMethod]
        public void result_of_addition_is_correct()
        {
            var operand = _factory.BuildOperand(
                BuildSpecifiers(Specifier, "1"),
                $"({Specifier}+1)");

            Assert.AreEqual(2, operand.GetIntValue().Result, "Value of the operand is not correct");
        }

        [TestMethod]
        public void result_subtraction_is_correct()
        {
            var operand = _factory.BuildOperand(
                BuildSpecifiers(Specifier, "2"),
                $"({Specifier}-1)");

            Assert.AreEqual(1, operand.GetIntValue().Result, "Value of the operand is not correct");
        }
    }
}