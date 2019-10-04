/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Test
{
    [TestClass]
    public class MessageMapperHelperTest
    {
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void EmptySpecifierNameThrowsAnException()
        {
            FeedMapperHelper.GetSpecifiers("=4");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SpecifierNameMustNotHaveSpacesOnly()
        {
            FeedMapperHelper.GetSpecifiers("    =4");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void EmptySpecifierValueThrowsAnException()
        {
            FeedMapperHelper.GetSpecifiers("periodnr=");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SpecifierValueMustNotHaveSpacesOnly()
        {
            FeedMapperHelper.GetSpecifiers("periodnr=     ");
        }

        [TestMethod]
        public void SingleSpecifiersIsParsedCorrectly()
        {
            var specifiers = FeedMapperHelper.GetSpecifiers("quarternr=4");
            Assert.IsNotNull(specifiers, "specifier should not be null");
            Assert.AreEqual("4", specifiers["quarternr"], "the value of the 'quarternr' specifier should be '4'");
        }

        [TestMethod]
        public void MultipleSpecifiersAreParsedCorrectly()
        {
            var specifiers = FeedMapperHelper.GetSpecifiers("quarternr=4|periodnr=1");
            Assert.AreEqual(2, specifiers.Count, "specifier count must be 2");

            Assert.AreEqual("4", specifiers["quarternr"], "Value of the 'quarternr' specifier must be '4'");
            Assert.AreEqual("1", specifiers["periodnr"], "Value of the 'periodnr' specifier must be 1");
        }

        [TestMethod]
        public void LeadingAndTrailingSpacesAreIgnored()
        {
            var specifiers = FeedMapperHelper.GetSpecifiers("   quarternr  =1|periodnr= 2 | score = 3 ");

            Assert.AreEqual(3, specifiers.Count, "specifier.Count must be 3");
            Assert.AreEqual("1", specifiers["quarternr"], "Value of the 'quarternr' specifier must be '1'");
            Assert.AreEqual("2", specifiers["periodnr"], "Value of the 'periodnr' specifier must be '2'");
            Assert.AreEqual("3", specifiers["score"], "Value of the 'score' specifier must be '3'");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DuplicatedSpecifiersAreNotAllowed()
        {
            var specifiers = FeedMapperHelper.GetSpecifiers("score=1| score =1");
            Assert.IsNotNull(specifiers);
        }
    }
}
