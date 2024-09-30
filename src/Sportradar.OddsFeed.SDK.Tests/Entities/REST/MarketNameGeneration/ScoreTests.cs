// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Wmhelp.XPath2;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.MarketNameGeneration;

public class ScoreTests
{
    [Fact]
    public void ToStringForIntegersReturnsCorrectValue()
    {
        var score = new Score(2, 3);

        Assert.Equal("2:3", score.ToString());
    }

    [Fact]
    public void ToStringForDecimalsReturnsCorrectValue()
    {
        const decimal homeScore = 1.5M;
        const decimal awayScore = 2.5M;
        var score = new Score(homeScore, awayScore);

        Assert.Equal($"{homeScore}:{awayScore}", score.ToString());
    }

    [Fact]
    public void ParsingOfCorrectValueWithIntSucceeds()
    {
        var score = Score.Parse("2:3");

        Assert.Equal(2, score.HomeScore);
        Assert.Equal(3, score.AwayScore);
    }

    [Fact]
    public void ParsingOfCorrectValueWithDecimalSucceeds()
    {
        const decimal homeScore = 1.5M;
        const decimal awayScore = 2.5M;
        var score = new Score(homeScore, awayScore);

        Assert.Equal(homeScore, score.HomeScore);
        Assert.Equal(awayScore, score.AwayScore);
    }

    [Fact]
    public void ParsingDetectsMissingColon()
    {
        Action action = () => Score.Parse("23");
        _ = action.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsingDetectsTooManyColons()
    {
        Action action = () => Score.Parse("2:3:4");
        _ = action.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsingDetectsMissingHomeScore()
    {
        Action action1 = () => Score.Parse(":3");
        _ = action1.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsingDetectsInvalidHomeScore()
    {
        Action action2 = () => Score.Parse("d:3");
        _ = action2.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsingDetectsMissingAwayScore()
    {
        Action action1 = () => Score.Parse("3:");
        _ = action1.Should().Throw<FormatException>();
    }

    [Fact]
    public void ParsingDetectsInvalidAwayScore()
    {
        Action action2 = () => Score.Parse("3:d");
        _ = action2.Should().Throw<FormatException>();
    }

    [Fact]
    public void ResultOfAdditionWithDecimalValuesIsCorrect()
    {
        var score1 = new Score(2.5M, 3.5M);
        var score2 = new Score(1.5M, 2.5M);

        var result = score1 + score2;

        Assert.Equal(4M, result.HomeScore);
        Assert.Equal(6M, result.AwayScore);
    }

    [Fact]
    public void TryParsingOfCorrectValueWithIntSucceeds()
    {
        var success = Score.TryParse("2:3", out var score);

        Assert.True(success);
        Assert.Equal(2, score.HomeScore);
        Assert.Equal(3, score.AwayScore);
    }

    [Fact]
    public void TryParsingOfCorrectValueWithDecimalSucceeds()
    {
        const decimal homeScore = 1.5M;
        const decimal awayScore = 3.5M;
        var success = Score.TryParse($"{homeScore}:{awayScore}", out var score);

        Assert.True(success);
        Assert.Equal(homeScore, score.HomeScore);
        Assert.Equal(awayScore, score.AwayScore);
    }

    [Theory]
    [InlineData("23")]
    [InlineData("2:3:4")]
    [InlineData(":3")]
    [InlineData("d:3")]
    [InlineData("3:")]
    [InlineData("3:a")]
    [InlineData("(3:3)")]
    public void TryParsingDoesNotThrowWhenInputWrong(string scoreString)
    {
        var success = Score.TryParse(scoreString, out var score);

        Assert.False(success);
        Assert.Null(score);
    }

    [Fact]
    public void CompareTwoScoresWhenEqualThenReturnTrue()
    {
        var score1 = Score.Parse("2:3");
        var score2 = Score.Parse("2:3");

        var success = Equals(score1, score2);

        Assert.True(success);
    }

    [Fact]
    public void CompareTwoScoresWhenNotEqualThenReturnFalse()
    {
        var score1 = Score.Parse("2:3");
        var score2 = Score.Parse("2:5");

        var success = Equals(score1, score2);

        Assert.False(success);
    }

    [Fact]
    public void CompareTwoScoresWhenSecondIsNullThenReturnFalse()
    {
        var score = Score.Parse("2:3");

        var success = score.Equals(null);

        Assert.False(success);
    }

    [Fact]
    public void CompareTwoScoresWhenFirstIsNullThenReturnFalse()
    {
        var score = Score.Parse("2:3");

        var success = Equals(null, score);

        Assert.False(success);
    }

    [Fact]
    public void CompareTwoScoresWhenSecondIsNotScoreThenReturnFalse()
    {
        var score = Score.Parse("2:3");

        // ReSharper disable once SuspiciousTypeConversion.Global
        var success = score.Equals(new Integer(2));

        Assert.False(success);
    }
}
