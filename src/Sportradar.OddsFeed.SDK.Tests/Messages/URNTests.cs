/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Messages;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Messages
{
    // ReSharper disable once InconsistentNaming
    public class URNTests
    {
        [Fact]
        public void MissingPrefixIsNotAllowed()
        {
            Action action = () => URN.Parse("match:1234");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void MissingTypeIsNotAllowed()
        {
            Action action = () => URN.Parse("sr:1234");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void NumberInTypeIsNotAllowed()
        {
            Action action = () => URN.Parse("sr:match1:12333");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void MinusInTypeIsNotAllowed()
        {
            Action action = () => URN.Parse("sr:ma-tch:1232");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void UnsupportedTypeIsNotAllowed()
        {
            var urn = URN.Parse("sr:event_tournament:1232");
            Assert.Equal(ResourceTypeGroup.UNKNOWN, urn.TypeGroup);
        }

        [Fact]
        public void MissingIdIsNotAllowed()
        {
            Action action = () => URN.Parse("sr:match");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void LetterInIdIsNotAllowed()
        {
            Action action = () => URN.Parse("sr:match:12a34");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void UnderscoreInIdIsNotAllowed()
        {
            Action action = () => URN.Parse("sr:match:123_4");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void SportEventResourceIsSupported()
        {
            const string urnString = "sr:sport_event:1234";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
            Assert.Equal(ResourceTypeGroup.MATCH, urn.TypeGroup);
        }

        [Fact]
        public void VfPrefixIsAllowed()
        {
            const string urnString = "vf:match:1234";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
        }

        [Fact]
        public void RaceEventResourceIsSupported()
        {
            const string urnString = "sr:race_event:1234";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
            Assert.Equal(ResourceTypeGroup.STAGE, urn.TypeGroup);
        }

        [Fact]
        public void SeasonResourceIsSupported()
        {
            const string urnString = "sr:season:1234";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
            Assert.Equal(ResourceTypeGroup.SEASON, urn.TypeGroup);
        }

        [Fact]
        public void TournamentResourceIsSupported()
        {
            const string urnString = "sr:tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
            Assert.Equal(ResourceTypeGroup.TOURNAMENT, urn.TypeGroup);
        }

        [Fact]
        public void SimpleTournamentResourceIsSupported()
        {
            const string urnString = "sr:simple_tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
            Assert.Equal(ResourceTypeGroup.BASIC_TOURNAMENT, urn.TypeGroup);
        }

        [Fact]
        public void RaceTournamentResourceIsSupported()
        {
            const string urnString = "sr:race_tournament:1234";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
            Assert.Equal(ResourceTypeGroup.STAGE, urn.TypeGroup);
        }

        [Fact]
        public void ParsedUrnHasCorrectValues()
        {
            const string urnString = "sr:sport_event:1234";
            var urn = URN.Parse(urnString);

            Assert.NotNull(urn);
            Assert.Equal("sr", urn.Prefix);
            Assert.Equal("sport_event", urn.Type);
            Assert.Equal(ResourceTypeGroup.MATCH, urn.TypeGroup);
            Assert.Equal(1234, urn.Id);
        }

        [Fact]
        public void UrnWithNegativeId()
        {
            const string urnString = "wns:draw:-2143997118";
            var urn = URN.Parse(urnString);
            Assert.NotNull(urn);
            Assert.Equal(ResourceTypeGroup.DRAW, urn.TypeGroup);
        }

        [Fact]
        public void CustomEventUrn()
        {
            const string urnString = "ccc:match:1234";
            var urn = URN.Parse(urnString);

            Assert.NotNull(urn);
            Assert.Equal("ccc", urn.Prefix);
            Assert.Equal("match", urn.Type);
            Assert.Equal(ResourceTypeGroup.MATCH, urn.TypeGroup);
            Assert.Equal(1234, urn.Id);
        }

        [Fact]
        public void CustomSimpleTournamentEventUrn()
        {
            const string urnString = "ccc:simple_tournament:1234";
            var urn = URN.Parse(urnString);

            Assert.NotNull(urn);
            Assert.Equal("ccc", urn.Prefix);
            Assert.Equal("simple_tournament", urn.Type);
            Assert.Equal(ResourceTypeGroup.BASIC_TOURNAMENT, urn.TypeGroup);
            Assert.Equal(1234, urn.Id);
        }

        [Fact]
        public void SrGroupRoundUrn()
        {
            const string urnString = "sr:group:1234";
            var urn = URN.Parse(urnString);

            Assert.NotNull(urn);
            Assert.Equal("sr", urn.Prefix);
            Assert.Equal("group", urn.Type);
            Assert.Equal(ResourceTypeGroup.OTHER, urn.TypeGroup);
            Assert.Equal(1234, urn.Id);
        }

        [Fact]
        public void ParseCustomTypeUrn()
        {
            const string urnString = "sr:abcde:1234";
            var urn = URN.Parse(urnString, true);

            Assert.NotNull(urn);
            Assert.Equal("sr", urn.Prefix);
            Assert.Equal("abcde", urn.Type);
            Assert.Equal(ResourceTypeGroup.UNKNOWN, urn.TypeGroup);
            Assert.Equal(1234, urn.Id);
        }

        [Fact]
        public void TryParseCustomTypeUrn()
        {
            const string urnString = "sr:abcde:1234";
            URN.TryParse(urnString, true, out var urn);

            Assert.NotNull(urn);
            Assert.Equal("sr", urn.Prefix);
            Assert.Equal("abcde", urn.Type);
            Assert.Equal(ResourceTypeGroup.UNKNOWN, urn.TypeGroup);
            Assert.Equal(1234, urn.Id);
        }

        [Fact]
        public void CustomStageEventUrn()
        {
            const string urnString = "ccc:stage:1234";
            var urn = URN.Parse(urnString);

            Assert.NotNull(urn);
            Assert.Equal("ccc", urn.Prefix);
            Assert.Equal("stage", urn.Type);
            Assert.Equal(ResourceTypeGroup.STAGE, urn.TypeGroup);
            Assert.Equal(1234, urn.Id);
        }
    }
}
