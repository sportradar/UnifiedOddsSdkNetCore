/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class DecimalValueMappingValidatorTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [Fact]
        public void Validation_of_correct_values_returns_true_for_X0()
        {
            var validator = _factory.Build("total~*.0");
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100")));
        }

        [Fact]
        public void Validation_of_incorrect_values_returns_false_for_X0()
        {
            var validator = _factory.Build("total~*.0");
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.5")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.5")));
        }

        [Fact]
        public void Validation_of_correct_values_returns_true_for_X25()
        {
            var validator = _factory.Build("total~*.25");
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.25")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3.25")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5.25")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17.25")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100.25")));
        }

        [Fact]
        public void Validation_of_incorrect_values_returns_false_for_X25()
        {
            var validator = _factory.Build("total~*.25");
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.5")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.75")));
        }

        [Fact]
        public void Validation_of_correct_values_returns_true_for_X5()
        {
            var validator = _factory.Build("total~*.5");
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.5")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.5")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3.5")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5.5")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17.5")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100.5")));
        }

        [Fact]
        public void Validation_of_incorrect_values_returns_false_for_X5()
        {
            var validator = _factory.Build("total~*.5");
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.75")));
        }

        [Fact]
        public void Validation_of_correct_values_returns_true_for_X75()
        {
            var validator = _factory.Build("total~*.75");
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.75")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.75")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3.75")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5.75")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17.75")));
            Assert.True(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100.75")));
        }

        [Fact]
        public void Validation_of_incorrect_values_returns_false_for_X75()
        {
            var validator = _factory.Build("total~*.75");
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.False(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.5")));
        }

        [Fact]
        public void Missing_specifier_causes_exception()
        {
            var validator = _factory.Build("total~*.75");
            Action action = () => validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1"));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Specifier_with_non_decimal_value_causes_exception()
        {
            var validator = _factory.Build("total~*.75");
            Action action = () => validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=first"));
            action.Should().Throw<InvalidOperationException>();
        }
    }
}
