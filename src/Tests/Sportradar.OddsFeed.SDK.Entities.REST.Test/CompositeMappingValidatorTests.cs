/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class CompositeMappingValidatorTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [TestMethod]
        public void true_is_returned_when_all_validators_return_true()
        {
            var validator = _factory.Build("setnr=1|gamenr=1|total~*.5");
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=1|total=1.5")));
        }

        [TestMethod]
        public void false_is_returned_when_any_validator_returns_false()
        {
            var validator = _factory.Build("setnr=1|gamenr=1|total~*.5");
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=2|gamenr=1|total=1.5")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=2|total=1.5")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|gamenr=1|total=1.75")));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void missing_specifier_causes_exception()
        {
            var validator = _factory.Build("setnr=1|total~*.75");
            validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void specifier_with_non_decimal_value_causes_exception()
        {
            var validator = _factory.Build("setnr=1|total~*.75");
            validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1|total=zero"));
        }
    }
}
