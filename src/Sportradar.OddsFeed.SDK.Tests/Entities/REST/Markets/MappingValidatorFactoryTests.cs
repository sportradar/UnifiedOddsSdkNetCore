/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
{
    public class MappingValidatorFactoryTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [Fact]
        public void Decimal_validator_X0_is_build()
        {
            var validator = _factory.Build("total~*.0");
            validator.Should().NotBeNull();
            validator.Should().BeOfType<DecimalValueMappingValidator>();
        }

        [Fact]
        public void Decimal_validator_X25_is_build()
        {
            var validator = _factory.Build("total~*.25");
            validator.Should().NotBeNull();
            validator.Should().BeOfType<DecimalValueMappingValidator>();
        }

        [Fact]
        public void Decimal_validator_X5_is_build()
        {
            var validator = _factory.Build("total~*.5");
            validator.Should().NotBeNull();
            validator.Should().BeOfType<DecimalValueMappingValidator>();
        }

        [Fact]
        public void Decimal_validator_X75_is_build()
        {
            var validator = _factory.Build("total~*.75");
            validator.Should().NotBeNull();
            validator.Should().BeOfType<DecimalValueMappingValidator>();
        }

        [Fact]
        public void Specific_value_validator_is_build()
        {
            var validator = _factory.Build("total=1");
            validator.Should().NotBeNull();
            validator.Should().BeOfType<SpecificValueMappingValidator>();
        }

        [Fact]
        public void Composite_validator_is_build()
        {
            var validator = _factory.Build("total~*.5|setnr=1");
            validator.Should().NotBeNull();
            validator.Should().BeOfType<CompositeMappingValidator>();
        }

        [Fact]
        public void Exception_is_thrown_when_value_is_not_specified()
        {
            Action action = () => _factory.Build("total");
            action.Should().Throw<ArgumentException>();
        }
    }
}
