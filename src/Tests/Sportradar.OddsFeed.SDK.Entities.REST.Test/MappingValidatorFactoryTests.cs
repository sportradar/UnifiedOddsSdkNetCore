/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class MappingValidatorFactoryTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [TestMethod]
        public void decimal_validator_X0_is_build()
        {
            var validator = _factory.Build("total~*.0");
            Assert.IsNotNull(validator);
            Assert.IsInstanceOfType(validator, typeof(DecimalValueMappingValidator));
        }

        [TestMethod]
        public void decimal_validator_X25_is_build()
        {
            var validator = _factory.Build("total~*.25");
            Assert.IsNotNull(validator);
            Assert.IsInstanceOfType(validator, typeof(DecimalValueMappingValidator));
        }

        [TestMethod]
        public void decimal_validator_X5_is_build()
        {
            var validator = _factory.Build("total~*.5");
            Assert.IsNotNull(validator);
            Assert.IsInstanceOfType(validator, typeof(DecimalValueMappingValidator));
        }

        [TestMethod]
        public void decimal_validator_X75_is_build()
        {
            var validator = _factory.Build("total~*.75");
            Assert.IsNotNull(validator);
            Assert.IsInstanceOfType(validator, typeof(DecimalValueMappingValidator));
        }

        [TestMethod]
        public void specific_value_validator_is_build()
        {
            var validator = _factory.Build("total=1");
            Assert.IsNotNull(validator);
            Assert.IsInstanceOfType(validator, typeof(SpecificValueMappingValidator));
        }

        [TestMethod]
        public void composite_validator_is_build()
        {
            var validator = _factory.Build("total~*.5|setnr=1");
            Assert.IsNotNull(validator);
            Assert.IsInstanceOfType(validator, typeof(CompositeMappingValidator));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void exception_is_thrown_when_value_is_not_specified()
        {
            _factory.Build("total");
        }
    }
}
