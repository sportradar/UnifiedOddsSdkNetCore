/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SpecificValueMappingValidatorTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [TestMethod]
        public void validation_of_correct_value_returns_true()
        {
            var validator = _factory.Build("setnr=1");
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1")));
        }

        [TestMethod]
        public void validation_of_correct_value_returns_false()
        {
            var validator = _factory.Build("setnr=1");
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=2")));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void missing_specifier_causes_exception()
        {
            var validator = _factory.Build("setnr=1");
            validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2"));
        }
    }
}
