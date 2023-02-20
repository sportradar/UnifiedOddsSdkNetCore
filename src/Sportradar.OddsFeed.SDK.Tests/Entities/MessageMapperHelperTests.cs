/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities
{
    public class MessageMapperHelperTests
    {
        [Fact]
        public void EmptySpecifierNameThrowsAnException()
        {
            Action action = () => FeedMapperHelper.GetSpecifiers("=4");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void SpecifierNameMustNotHaveSpacesOnly()
        {
            Action action = () => FeedMapperHelper.GetSpecifiers("    =4");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void EmptySpecifierValueThrowsAnException()
        {
            Action action = () => FeedMapperHelper.GetSpecifiers("periodnr=");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void SpecifierValueMustNotHaveSpacesOnly()
        {
            Action action = () => FeedMapperHelper.GetSpecifiers("periodnr=     ");
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void SingleSpecifiersIsParsedCorrectly()
        {
            var specifiers = FeedMapperHelper.GetSpecifiers("quarternr=4");
            Assert.NotNull(specifiers);
            Assert.Equal("4", specifiers["quarternr"]);
        }

        [Fact]
        public void MultipleSpecifiersAreParsedCorrectly()
        {
            var specifiers = FeedMapperHelper.GetSpecifiers("quarternr=4|periodnr=1");
            Assert.Equal(2, specifiers.Count);

            Assert.Equal("4", specifiers["quarternr"]);
            Assert.Equal("1", specifiers["periodnr"]);
        }

        [Fact]
        public void LeadingAndTrailingSpacesAreIgnored()
        {
            var specifiers = FeedMapperHelper.GetSpecifiers("   quarternr  =1|periodnr= 2 | score = 3 ");

            Assert.Equal(3, specifiers.Count);
            Assert.Equal("1", specifiers["quarternr"]);
            Assert.Equal("2", specifiers["periodnr"]);
            Assert.Equal("3", specifiers["score"]);
        }

        [Fact]
        public void DuplicatedSpecifiersAreNotAllowed()
        {
            Action action = () => FeedMapperHelper.GetSpecifiers("score=1| score =1");
            action.Should().Throw<ArgumentException>();
        }
    }
}
