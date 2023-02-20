/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class SpecificValueMappingValidatorTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [Fact]
        public void Validation_of_correct_value_returns_true()
        {
            var validator = _factory.Build("setnr=1");
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1")));
        }

        [Fact]
        public void Validation_of_correct_value_returns_false()
        {
            var validator = _factory.Build("setnr=1");
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=2")));
        }

        [Fact]
        public void Missing_specifier_causes_exception()
        {
            var validator = _factory.Build("setnr=1");
            Action action = () => validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2"));
            action.Should().Throw<InvalidOperationException>();
        }
    }
}
