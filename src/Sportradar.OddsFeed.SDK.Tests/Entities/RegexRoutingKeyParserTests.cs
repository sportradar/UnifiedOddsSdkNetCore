/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class RegexRoutingKeyParserTests
    {
        /// <summary>
        /// Class under test
        /// </summary>
        private static readonly IRoutingKeyParser Parser = new RegexRoutingKeyParser();

        [Fact]
        public void TestBetSettlementKeyIsParsedCorrectly()
        {
            const string key = "lo.-.live.bet_settlement.5.sr:match.9583179";
            var sportId = Parser.GetSportId(key, "bet_settlement");
            Assert.Equal(5, sportId.Id);
        }

        [Fact]
        public void TestOddsChangeKeyIsParsedCorrectly()
        {
            const string key = "hi.-.live.odds_change.6.sr:match.9536715";
            var sportId = Parser.GetSportId(key, "odds_change");
            Assert.Equal(6, sportId.Id);
        }

        [Fact]
        public void TestRegexRoutingKeyThrows()
        {
            //wrong message type name: expected: odds_change, actual: oddschange
            const string key = "hi.-.live.oddschange.6.sr:match.9536715";
            Action action = () => Parser.GetSportId(key, "odds_change");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void TestRegexRoutingKeyForWrongType()
        {
            //wrong message type name: expected: odds_change, actual: alive
            const string key = "hi.-.live.odds_change.6.sr:match.9536715";
            var sportId = Parser.GetSportId(key, "alive");
            Assert.Null(sportId);
        }

        [Fact]
        public void TestTryRegexRoutingKeyForWrongType()
        {
            //wrong message type name: expected: odds_change, actual: alive
            const string key = "hi.-.live.odds_change.6.sr:match.9536715";
            var result = Parser.TryGetSportId(key, "alive", out var sportId);
            Assert.False(result);
            Assert.Null(sportId);
        }

        [Fact]
        public void TestRegexRoutingKeyThrows2()
        {
            //missing dot before st:match
            const string key = "hi.-.live.odds_change.6sr:match.9536715";
            Action action = () => Parser.GetSportId(key, "odds_change");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void TestRegexRoutingKeyThrows3()
        {
            //wrong sport id(6b) - it should be a long
            const string key = "hi.-.live.odds_change.6b.sr:match.9536715";
            Action action = () => Parser.GetSportId(key, "odds_change");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void TestAliveKey()
        {
            const string key = "-.-.-.alive.-.-.-.-";
            var sportId = Parser.GetSportId(key, "alive");
            Assert.Null(sportId);
        }

        [Fact]
        public void TestSnapshotCompleteKey()
        {
            const string key = "-.-.-.snapshot_complete.-.-.-.-";
            var sportId = Parser.GetSportId(key, "snapshot_complete");
            Assert.Null(sportId);
        }
    }
}
