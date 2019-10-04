/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class ScoreTest
    {
        [TestMethod]
        public void to_string_for_integers_returns_correct_value()
        {
            var score = new Score(2, 3);
            Assert.AreEqual("2:3", score.ToString(), "Value returned by ToString is not correct");
        }

        [TestMethod]
        public void to_string_for_decimals_returns_correct_value()
        {
            var score = new Score((decimal)1.5, (decimal)2.5);
            Assert.AreEqual($"{(decimal)1.5}:{(decimal)2.5}", score.ToString(), "Value returned by ToString method is not correct");
        }

        [TestMethod]
        public void parsing_of_correct_value_with_int_succeeds()
        {
            var score = Score.Parse("2:3");
            Assert.AreEqual(2, score.HomeScore, "Value of the HomeScore is not correct");
            Assert.AreEqual(3, score.AwayScore, "Value of the AwayScore is not correct");
        }

        [TestMethod]
        public void parsing_of_correct_value_with_decimal_succeeds()
        {
            var score = Score.Parse($"{(decimal)2.5}:{(decimal)3.5}");
            Assert.AreEqual((decimal)2.5, score.HomeScore, "Value of the HomeScore is not correct");
            Assert.AreEqual((decimal)3.5, score.AwayScore, "Value of the AwayScore is not correct");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void parsing_detects_missing_colon()
        {
            Score.Parse("23");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void parsing_detects_to_many_colons()
        {
            Score.Parse("2:3:4");
        }

        [TestMethod]
        public void parsing_detects_missing_or_invalid_home_score()
        {
            try
            {
                Score.Parse(":3");
                Assert.Fail("Parsing of expression :3 should fail");
            }
            catch (FormatException)
            {

            }

            try
            {
                Score.Parse("d:3");
                Assert.Fail("Parsing of expression d:3 should fail");
            }
            catch (FormatException)
            {

            }
        }

        [TestMethod]
        public void parsing_detects_missing_or_invalid_away_score()
        {
            try
            {
                Score.Parse(":3");
                Assert.Fail("Parsing of expression 3: should fail");
            }
            catch (FormatException)
            {

            }

            try
            {
                Score.Parse("3:d");
                Assert.Fail("Parsing of expression 3:d should fail");
            }
            catch (FormatException)
            {

            }
        }

        [TestMethod]
        public void result_of_addition_with_decimal_values_is_correct()
        {
            var score1 = new Score(2.5M, 3.5M);
            var score2 = new Score(1.5M, 2.5M);

            var result = score1 + score2;
            Assert.AreEqual(4M, result.HomeScore, "Value of HomeScore is not correct");
            Assert.AreEqual(6M, result.AwayScore, "Value of AwayScore is not correct");
        }
    }
}
