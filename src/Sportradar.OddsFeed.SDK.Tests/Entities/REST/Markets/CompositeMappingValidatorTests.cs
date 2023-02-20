/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class CompositeMappingValidatorTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [Fact]
        public void True_is_returned_when_all_validators_return_true()
        {
            var validator = _factory.Build("setnr=1|gamenr=1|total~*.5");
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=1|total=1.5")));
        }

        [Fact]
        public void False_is_returned_when_any_validator_returns_false()
        {
            var validator = _factory.Build("setnr=1|gamenr=1|total~*.5");
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=2|gamenr=1|total=1.5")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=2|total=1.5")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=1|total=1.75")));
        }

        [Fact]
        public void Missing_specifier_causes_exception()
        {
            var validator = _factory.Build("setnr=1|total~*.75");
            Action action = () => validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1"));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Specifier_with_non_decimal_value_causes_exception()
        {
            var validator = _factory.Build("setnr=1|total~*.75");
            Action action = () => validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|total=zero"));
            action.Should().Throw<InvalidOperationException>();
        }
    }
}
