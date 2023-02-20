/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class CategorySummaryTests
    {
        [Fact]
        public void WhenCreatingNewIdCantBeNull()
        {
            Action action = () => _ = new CategorySummary(null, TestData.Cultures.ToDictionary(c => c, c => string.Empty), "en");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenCreatingNewNamesCantBeNull()
        {
            Action action = () => _ = new CategorySummary(TestData.CategoryId, null, "en");
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenCreatingNewNamesCantBeEmpty()
        {
            Action action = () => _ = new CategorySummary(TestData.CategoryId, new Dictionary<CultureInfo, string>(), "en");
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ReturnValidId()
        {
            var item = new CategorySummary(TestData.CategoryId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName), "en");
            Assert.Equal(TestData.CategoryId, item.Id);
        }

        [Fact]
        public void ReturnValidName()
        {
            var item = new CategorySummary(TestData.CategoryId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName), "en");
            var name = item.GetName(TestData.Culture);
            Assert.Equal(3, item.Names.Count);
            Assert.True(!string.IsNullOrEmpty(name));
        }

        [Fact]
        public void ReturnInvalidName()
        {
            var item = new CategorySummary(TestData.CategoryId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName), "en");
            var name = item.GetName(new CultureInfo("gl"));
            Assert.Equal(3, item.Names.Count);
            Assert.Null(name);
        }
    }
}
