// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Messages;

public class UrnTests
{
    [Fact]
    public void MissingPrefixIsNotAllowed()
    {
        Action action = () => Urn.Parse("match:1234");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void MissingTypeIsNotAllowed()
    {
        Action action = () => Urn.Parse("sr:1234");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void NumberInTypeIsNotAllowed()
    {
        Action action = () => Urn.Parse("sr:match1:12333");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void MinusInTypeIsNotAllowed()
    {
        Action action = () => Urn.Parse("sr:ma-tch:1232");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void UnsupportedTypeIsNotAllowed()
    {
        var urn = Urn.Parse("sr:event_tournament:1232");
        Assert.Equal(ResourceTypeGroup.Unknown, urn.TypeGroup);
    }

    [Fact]
    public void MissingIdIsNotAllowed()
    {
        Action action = () => Urn.Parse("sr:match");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void LetterInIdIsNotAllowed()
    {
        Action action = () => Urn.Parse("sr:match:12a34");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void UnderscoreInIdIsNotAllowed()
    {
        Action action = () => Urn.Parse("sr:match:123_4");
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void SportEventResourceIsSupported()
    {
        const string urnString = "sr:sport_event:1234";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
        Assert.Equal(ResourceTypeGroup.Match, urn.TypeGroup);
    }

    [Fact]
    public void VfPrefixIsAllowed()
    {
        const string urnString = "vf:match:1234";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
    }

    [Fact]
    public void RaceEventResourceIsSupported()
    {
        const string urnString = "sr:race_event:1234";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
        Assert.Equal(ResourceTypeGroup.Stage, urn.TypeGroup);
    }

    [Fact]
    public void SeasonResourceIsSupported()
    {
        const string urnString = "sr:season:1234";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
        Assert.Equal(ResourceTypeGroup.Season, urn.TypeGroup);
    }

    [Fact]
    public void TournamentResourceIsSupported()
    {
        const string urnString = "sr:tournament:1234";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
        Assert.Equal(ResourceTypeGroup.Tournament, urn.TypeGroup);
    }

    [Fact]
    public void SimpleTournamentResourceIsSupported()
    {
        const string urnString = "sr:simple_tournament:1234";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
        Assert.Equal(ResourceTypeGroup.BasicTournament, urn.TypeGroup);
    }

    [Fact]
    public void RaceTournamentResourceIsSupported()
    {
        const string urnString = "sr:race_tournament:1234";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
        Assert.Equal(ResourceTypeGroup.Stage, urn.TypeGroup);
    }

    [Fact]
    public void ParsedUrnHasCorrectValues()
    {
        const string urnString = "sr:sport_event:1234";
        var urn = Urn.Parse(urnString);

        Assert.NotNull(urn);
        Assert.Equal("sr", urn.Prefix);
        Assert.Equal("sport_event", urn.Type);
        Assert.Equal(ResourceTypeGroup.Match, urn.TypeGroup);
        Assert.Equal(1234, urn.Id);
    }

    [Fact]
    public void UrnWithNegativeId()
    {
        const string urnString = "wns:draw:-2143997118";
        var urn = Urn.Parse(urnString);
        Assert.NotNull(urn);
        Assert.Equal(ResourceTypeGroup.Draw, urn.TypeGroup);
    }

    [Fact]
    public void CustomEventUrn()
    {
        const string urnString = "ccc:match:1234";
        var urn = Urn.Parse(urnString);

        Assert.NotNull(urn);
        Assert.Equal("ccc", urn.Prefix);
        Assert.Equal("match", urn.Type);
        Assert.Equal(ResourceTypeGroup.Match, urn.TypeGroup);
        Assert.Equal(1234, urn.Id);
    }

    [Fact]
    public void CustomSimpleTournamentEventUrn()
    {
        const string urnString = "ccc:simple_tournament:1234";
        var urn = Urn.Parse(urnString);

        Assert.NotNull(urn);
        Assert.Equal("ccc", urn.Prefix);
        Assert.Equal("simple_tournament", urn.Type);
        Assert.Equal(ResourceTypeGroup.BasicTournament, urn.TypeGroup);
        Assert.Equal(1234, urn.Id);
    }

    [Fact]
    public void SrGroupRoundUrn()
    {
        const string urnString = "sr:group:1234";
        var urn = Urn.Parse(urnString);

        Assert.NotNull(urn);
        Assert.Equal("sr", urn.Prefix);
        Assert.Equal("group", urn.Type);
        Assert.Equal(ResourceTypeGroup.Other, urn.TypeGroup);
        Assert.Equal(1234, urn.Id);
    }

    [Fact]
    public void ParseCustomTypeUrn()
    {
        const string urnString = "sr:abcde:1234";
        var urn = Urn.Parse(urnString, true);

        Assert.NotNull(urn);
        Assert.Equal("sr", urn.Prefix);
        Assert.Equal("abcde", urn.Type);
        Assert.Equal(ResourceTypeGroup.Unknown, urn.TypeGroup);
        Assert.Equal(1234, urn.Id);
    }

    [Fact]
    public void TryParseCustomTypeUrn()
    {
        const string urnString = "sr:abcde:1234";
        Urn.TryParse(urnString, true, out var urn);

        Assert.NotNull(urn);
        Assert.Equal("sr", urn.Prefix);
        Assert.Equal("abcde", urn.Type);
        Assert.Equal(ResourceTypeGroup.Unknown, urn.TypeGroup);
        Assert.Equal(1234, urn.Id);
    }

    [Fact]
    public void CustomStageEventUrn()
    {
        const string urnString = "ccc:stage:1234";
        var urn = Urn.Parse(urnString);

        Assert.NotNull(urn);
        Assert.Equal("ccc", urn.Prefix);
        Assert.Equal("stage", urn.Type);
        Assert.Equal(ResourceTypeGroup.Stage, urn.TypeGroup);
        Assert.Equal(1234, urn.Id);
    }
}
