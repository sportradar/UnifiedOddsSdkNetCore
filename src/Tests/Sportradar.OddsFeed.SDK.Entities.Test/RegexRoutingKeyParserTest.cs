/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class RegexRoutingKeyParserTest
    {
        /// <summary>
        /// Class under test
        /// </summary>
        private static readonly IRoutingKeyParser Parser = new RegexRoutingKeyParser();

        [TestMethod]
        public void TestBetSettlementKeyIsParsedCorrectly()
        {
            const string key = @"lo.-.live.bet_settlement.5.sr:match.9583179";
            var sportId = Parser.GetSportId(key, "bet_settlement");
            Assert.AreEqual(sportId.Id, 5, "The parsed sportId is not correct");
        }

        [TestMethod]
        public void TestOddsChangeKeyIsParsedCorrectly()
        {
            const string key = @"hi.-.live.odds_change.6.sr:match.9536715";
            var sportId = Parser.GetSportId(key, "odds_change");
            Assert.AreEqual(sportId.Id, 6, "The parsed sportId is not correct");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestRegexRoutingKeyThrows()
        {
            //wrong message type name: expected: odds_change, actual: oddschange
            const string key = @"hi.-.live.oddschange.6.sr:match.9536715";
            Parser.GetSportId(key, "odds_change");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestRegexRoutingKeyThrows1()
        {
            //wrong message type name: expected: odds_change, actual: alive
            const string key = @"hi.-.live.odds_change.6.sr:match.9536715";
            Parser.GetSportId(key, "alive");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestRegexRoutingKeyThrows2()
        {
            //missing dot before st:match
            const string key = @"hi.-.live.odds_change.6sr:match.9536715";
            Parser.GetSportId(key, "odds_change");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void TestRegexRoutingKeyThrows3()
        {
            //wrong sport id(6b) - it should be a long
            const string key = @"hi.-.live.odds_change.6b.sr:match.9536715";
            Parser.GetSportId(key, "odds_change");
        }
    }
}
