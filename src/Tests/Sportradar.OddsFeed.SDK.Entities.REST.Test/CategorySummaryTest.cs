/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class CategorySummaryTest
    {
        [TestMethod]
        [ExpectedException(typeof(Exception), "Id must not be null.", AllowDerivedTypes = true)]
        public void WhenCreatingNewIdCantBeNull()
        {
            var item = new CategorySummary(null, TestData.Cultures.ToDictionary(c => c, c => string.Empty), "en");
            Assert.IsNotNull(item);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Id must not be empty.", AllowDerivedTypes = true)]
        public void WhenCreatingNewIdCantBeEmpty()
        {
            var item = new CategorySummary(null, TestData.Cultures.ToDictionary(c => c, c => string.Empty), "en");
            Assert.IsNotNull(item);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Names must not be null.", AllowDerivedTypes = true)]
        public void WhenCreatingNewNamesCantBeNull()
        {
            var item = new CategorySummary(TestData.CategoryId, null, "en");
            Assert.IsNotNull(item);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Names must not be empty.", AllowDerivedTypes = true)]
        public void WhenCreatingNewNamesCantBeEmpty()
        {
            var item = new CategorySummary(TestData.CategoryId, new Dictionary<CultureInfo, string>(), "en");
            Assert.IsNotNull(item);
        }

        [TestMethod]
        public void ReturnValidId()
        {
            var item = new CategorySummary(TestData.CategoryId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName), "en");
            Assert.AreEqual(TestData.CategoryId, item.Id);
        }

        [TestMethod]
        public void ReturnValidName()
        {
            var item = new CategorySummary(TestData.CategoryId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName), "en");
            var name = item.GetName(TestData.Culture);
            Assert.AreEqual(3, item.Names.Count);
            Assert.IsTrue(!string.IsNullOrEmpty(name));
        }

        [TestMethod]
        public void ReturnInvalidName()
        {
            var item = new CategorySummary(TestData.CategoryId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName), "en");
            var name = item.GetName(new CultureInfo("gl"));
            Assert.AreEqual(3, item.Names.Count);
            Assert.IsNull(name);
        }
    }
}
