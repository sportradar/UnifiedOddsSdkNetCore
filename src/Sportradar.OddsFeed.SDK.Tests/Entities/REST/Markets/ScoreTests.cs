/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class ScoreTests
    {
        [Fact]
        public void To_string_for_integers_returns_correct_value()
        {
            var score = new Score(2, 3);
            Assert.Equal("2:3", score.ToString());
        }

        [Fact]
        public void To_string_for_decimals_returns_correct_value()
        {
            var score = new Score((decimal)1.5, (decimal)2.5);
            Assert.Equal($"{(decimal)1.5}:{(decimal)2.5}", score.ToString());
        }

        [Fact]
        public void Parsing_of_correct_value_with_int_succeeds()
        {
            var score = Score.Parse("2:3");
            Assert.Equal(2, score.HomeScore);
            Assert.Equal(3, score.AwayScore);
        }

        [Fact]
        public void Parsing_of_correct_value_with_decimal_succeeds()
        {
            var score = Score.Parse($"{(decimal)2.5}:{(decimal)3.5}");
            Assert.Equal((decimal)2.5, score.HomeScore);
            Assert.Equal((decimal)3.5, score.AwayScore);
        }

        [Fact]
        public void Parsing_detects_missing_colon()
        {
            Action action = () => Score.Parse("23");
            _ = action.Should().Throw<FormatException>();
        }

        [Fact]
        public void Parsing_detects_to_many_colons()
        {
            Action action = () => Score.Parse("2:3:4");
            _ = action.Should().Throw<FormatException>();
        }

        [Fact]
        public void Parsing_detects_missing_or_invalid_home_score()
        {
            Action action1 = () => Score.Parse(":3");
            _ = action1.Should().Throw<FormatException>();

            Action action2 = () => Score.Parse("d:3");
            _ = action2.Should().Throw<FormatException>();
        }

        [Fact]
        public void Parsing_detects_missing_or_invalid_away_score()
        {
            Action action1 = () => Score.Parse("3:");
            _ = action1.Should().Throw<FormatException>();

            Action action2 = () => Score.Parse("3:d");
            _ = action2.Should().Throw<FormatException>();
        }

        [Fact]
        public void Result_of_addition_with_decimal_values_is_correct()
        {
            var score1 = new Score(2.5M, 3.5M);
            var score2 = new Score(1.5M, 2.5M);

            var result = score1 + score2;
            Assert.Equal(4M, result.HomeScore);
            Assert.Equal(6M, result.AwayScore);
        }
    }
}
