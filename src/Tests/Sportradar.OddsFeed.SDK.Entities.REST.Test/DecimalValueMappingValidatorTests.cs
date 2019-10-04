/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class DecimalValueMappingValidatorTests
    {
        private readonly IMappingValidatorFactory _factory = new MappingValidatorFactory();

        [TestMethod]
        public void validation_of_correct_values_returns_true_for_X0()
        {
            var validator = _factory.Build("total~*.0");
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100")));
        }

        [TestMethod]
        public void validation_of_incorrect_values_returns_false_for_X0()
        {
            var validator = _factory.Build("total~*.0");
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.5")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.5")));
        }

        [TestMethod]
        public void validation_of_correct_values_returns_true_for_X25()
        {
            var validator = _factory.Build("total~*.25");
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.25")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3.25")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5.25")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17.25")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100.25")));
        }

        [TestMethod]
        public void validation_of_incorrect_values_returns_false_for_X25()
        {
            var validator = _factory.Build("total~*.25");
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.5")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.75")));
        }

        [TestMethod]
        public void validation_of_correct_values_returns_true_for_X5()
        {
            var validator = _factory.Build("total~*.5");
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.5")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.5")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3.5")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5.5")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17.5")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100.5")));
        }

        [TestMethod]
        public void validation_of_incorrect_values_returns_false_for_X5()
        {
            var validator = _factory.Build("total~*.5");
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.75")));
        }

        [TestMethod]
        public void validation_of_correct_values_returns_true_for_X75()
        {
            var validator = _factory.Build("total~*.75");
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.75")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1.75")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.75")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=3.75")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=5.75")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=17.75")));
            Assert.IsTrue(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=100.75")));
        }

        [TestMethod]
        public void validation_of_incorrect_values_returns_false_for_X75()
        {
            var validator = _factory.Build("total~*.75");
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.25")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=0.5")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=1")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=2.25")));
            Assert.IsFalse(validator.Validate(FeedMapperHelper.GetValidForAttributes("total=15.5")));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void missing_specifier_causes_exception()
        {
            var validator = _factory.Build("total~*.75");
            validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void specifier_with_non_decimal_value_causes_exception()
        {
            var validator = _factory.Build("total~*.75");
            validator.Validate(FeedMapperHelper.GetValidForAttributes("setnr=first"));
        }
    }
}
